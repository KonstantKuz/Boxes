using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;
using Random = UnityEngine.Random;

namespace Gameplay.RoadSystem
{
    [ExecuteAlways]
    [RequireComponent(typeof(Transform))]
    public class CarAgent : MonoBehaviour
    {
        [Header("References")]
        public RoadSystem roadSystem;
        public Transform targetTransform;
        [SerializeField] private Rigidbody carRigidbody;

        [Header("Movement")]
        public float speed = 5f;
        public float rotationSpeed = 5f;
        public float nodeReachDistance = 0.5f;
        public float laneOffset;

        [Header("Route Mode")]
        public bool useRandomRoute = false;
        public bool useAssignedRoute = false;
        public bool isDriving = false;
        public bool useAllowedNodesOnly = false;

        [Header("Target Settings")]
        public float targetReachedDistance = 1f;

        [Header("Allowed Nodes")]
        [SerializeField] private List<int> allowedNodeIndices = new List<int>();

        private RoadSystem.Node currentNode;
        private RoadSystem.Edge currentEdge;
        private RoadSystem.Node previousNode;
        private RoadSystem.Edge previousEdge;
        private float currentT;
        private bool movingForward;
        private float startT;
        private float endT;
        private float splineLength;
        private bool isInitialized;
        private bool hasReachedTarget;
        private bool isFirstEdge;
        private List<RoadSystem.Node> assignedRoute;
        private RoadSystem.Edge targetEdge;

        [ContextMenu("Start Driving")]
        public void SetDriving(bool value)
        {
            if (!value)
            {
                isDriving = false;
                return;
            }

            if (roadSystem == null)
            {
                Debug.LogWarning("CarAgent: RoadSystem not assigned.");
                return;
            }

            bool wasAlreadyDriving = isDriving;
            isDriving = true;

            if (targetTransform != null && hasReachedTarget)
            {
                hasReachedTarget = false;
            }

            if (!wasAlreadyDriving && isInitialized && currentEdge != null)
            {
                Spline spline = roadSystem.Container.Splines[currentEdge.SplineIndex];
                float newT = FindClosestTOnSpline(spline, transform.position, startT, endT);
                currentT = Mathf.Clamp(newT, Mathf.Min(startT, endT), Mathf.Max(startT, endT));
            }
        }

        public void SetTarget(Transform target)
        {
            targetTransform = target;
            hasReachedTarget = false;
            isFirstEdge = true;
            targetEdge = null;
        }

        public void RemoveTarget()
        {
            targetTransform = null;
        }

        private void Update()
        {
            if (!isDriving)
            {
                return;
            }

            if (roadSystem == null || roadSystem.Nodes.Count == 0)
            {
                return;
            }

            if (!isInitialized)
            {
                InitializeDriving();
                return;
            }

            if (carRigidbody == null)
            {
                UpdateDriving(Time.deltaTime);
            }
        }

        private void FixedUpdate()
        {
            if (!isDriving || !isInitialized || carRigidbody == null)
            {
                return;
            }

            UpdateDriving(Time.fixedDeltaTime);
        }

        private void InitializeDriving()
        {
            if (useAssignedRoute && assignedRoute != null && assignedRoute.Count > 0)
            {
                currentNode = assignedRoute[0];
            }
            else
            {
                RoadSystem.Edge closestEdge = FindClosestEdge(transform.position);
                if (closestEdge == null)
                {
                    Debug.LogError("CarAgent: No edges found in road system");
                    isDriving = false;
                    return;
                }

                currentNode = GetCloserNodeOnEdge(closestEdge, transform.position);
            }

            if (useAllowedNodesOnly && allowedNodeIndices.Count > 0)
            {
                bool hasValidNeighbor = false;
                foreach (RoadSystem.Edge edge in currentNode.Edges)
                {
                    RoadSystem.Node neighbor = edge.GetOther(currentNode);
                    if (IsNodeAllowed(neighbor))
                    {
                        hasValidNeighbor = true;
                        break;
                    }
                }

                if (!hasValidNeighbor)
                {
                    RoadSystem.Node closestAllowedNode = FindClosestAllowedNode();
                    if (closestAllowedNode != null)
                    {
                        currentNode = closestAllowedNode;
                        Debug.Log($"CarAgent: Using closest allowed node at {currentNode.Position}");
                    }
                    else
                    {
                        Debug.LogError("CarAgent: No allowed nodes found!");
                        isDriving = false;
                        return;
                    }
                }
            }

            previousNode = null;
            previousEdge = null;
            hasReachedTarget = false;
            isFirstEdge = true;
            targetEdge = null;

            if (currentNode.Edges.Count == 0)
            {
                Debug.LogError("CarAgent: No valid edges from start node");
                isDriving = false;
                return;
            }

            StartNewEdge();
            isInitialized = true;
        }

        private void UpdateDriving(float deltaTime)
        {
            if (currentEdge == null)
            {
                StartNewEdge();
                return;
            }

            Spline spline = roadSystem.Container.Splines[currentEdge.SplineIndex];
            float direction = movingForward ? 1f : -1f;

            if (!useRandomRoute && targetTransform != null && ShouldStopOnEdge(spline))
            {
                hasReachedTarget = true;
                return;
            }

            float tStep = (speed * deltaTime) / splineLength * Mathf.Abs(endT - startT);
            currentT += tStep * direction;

            Vector3 centerPosition = roadSystem.transform.TransformPoint(spline.EvaluatePosition(currentT));
            Vector3 tangent = GetTangentAtT(spline, currentT, direction);

            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            centerPosition += right * laneOffset;

            if (carRigidbody != null)
            {
                carRigidbody.MovePosition(centerPosition);
            }
            else
            {
                transform.position = centerPosition;
            }

            if (tangent.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(tangent);
                Quaternion newRotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * deltaTime);

                if (carRigidbody != null)
                {
                    carRigidbody.MoveRotation(newRotation);
                }
                else
                {
                    transform.rotation = newRotation;
                }
            }

            RoadSystem.Node targetNode = currentEdge.GetOther(currentNode);
            float distanceToTarget = Vector3.Distance(transform.position, targetNode.Position);

            if (distanceToTarget <= nodeReachDistance)
            {
                previousNode = currentNode;
                previousEdge = currentEdge;
                currentNode = targetNode;
                currentEdge = null;
                return;
            }

            if ((direction > 0 && currentT >= endT) || (direction < 0 && currentT <= endT))
            {
                previousNode = currentNode;
                previousEdge = currentEdge;
                currentNode = targetNode;
                currentEdge = null;
            }
        }

        private bool ShouldStopOnEdge(Spline spline)
        {
            if (hasReachedTarget)
            {
                return true;
            }

            if (targetEdge == null)
            {
                targetEdge = FindClosestEdgeToTarget();
            }

            if (targetEdge != currentEdge)
            {
                return false;
            }

            float closestT = FindClosestTOnSpline(spline, targetTransform.position, startT, endT);
            float distanceFromCarToClosest = Mathf.Abs(currentT - closestT) * splineLength / Mathf.Abs(endT - startT);

            if (distanceFromCarToClosest <= nodeReachDistance)
            {
                currentT = closestT;
                Vector3 centerPosition = roadSystem.transform.TransformPoint(spline.EvaluatePosition(currentT));
                Vector3 right = Vector3.Cross(Vector3.up, GetTangentAtT(spline, currentT, movingForward ? 1f : -1f)).normalized;
                centerPosition += right * laneOffset;

                if (carRigidbody != null)
                {
                    carRigidbody.MovePosition(centerPosition);
                }
                else
                {
                    transform.position = centerPosition;
                }

                return true;
            }

            return false;
        }

        private RoadSystem.Edge FindClosestEdgeToTarget()
        {
            RoadSystem.Edge closestEdge = null;
            float minDistance = float.MaxValue;

            foreach (RoadSystem.Edge edge in roadSystem.Edges)
            {
                Spline spline = roadSystem.Container.Splines[edge.SplineIndex];
                float closestT = FindClosestTOnSpline(spline, targetTransform.position, edge.StartT, edge.EndT);
                Vector3 closestPoint = roadSystem.transform.TransformPoint(spline.EvaluatePosition(closestT));
                float distance = Vector3.Distance(closestPoint, targetTransform.position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEdge = edge;
                }
            }

            return closestEdge;
        }

        private void StartNewEdge()
        {
            RoadSystem.Edge nextEdge = ChooseNextEdge();

            if (nextEdge == null)
            {
                Debug.LogError($"No valid edge from node at {currentNode.Position}, edges count: {currentNode.Edges.Count}");
                isDriving = false;
                return;
            }

            currentEdge = nextEdge;

            Spline spline = roadSystem.Container.Splines[currentEdge.SplineIndex];
            movingForward = currentEdge.StartNode == currentNode;

            startT = movingForward ? currentEdge.StartT : currentEdge.EndT;
            endT = movingForward ? currentEdge.EndT : currentEdge.StartT;

            if (isFirstEdge)
            {
                currentT = FindClosestTOnSpline(spline, transform.position, startT, endT);
                isFirstEdge = false;
            }
            else
            {
                currentT = startT;
            }

            splineLength = CalculateSplineLength(spline, startT, endT);
        }

        private RoadSystem.Node GetClosestNode(Vector3 position)
        {
            List<RoadSystem.Node> closestNodes = roadSystem.FindClosestNodes(position, 1);
            return closestNodes.Count > 0 ? closestNodes[0] : null;
        }

        private RoadSystem.Edge FindClosestEdge(Vector3 position)
        {
            RoadSystem.Edge closestEdge = null;
            float minDistance = float.MaxValue;

            foreach (RoadSystem.Edge edge in roadSystem.Edges)
            {
                Vector3 closestPoint = GetClosestPointOnEdge(edge, position);
                float distance = Vector3.Distance(closestPoint, position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestEdge = edge;
                }
            }

            return closestEdge;
        }

        private Vector3 GetClosestPointOnEdge(RoadSystem.Edge edge, Vector3 position)
        {
            Vector3 lineStart = edge.StartNode.Position;
            Vector3 lineEnd = edge.EndNode.Position;
            Vector3 lineDirection = lineEnd - lineStart;
            float lineLength = lineDirection.magnitude;

            if (lineLength < 0.001f)
            {
                return lineStart;
            }

            lineDirection /= lineLength;

            float param = Vector3.Dot(position - lineStart, lineDirection);
            param = Mathf.Clamp(param, 0f, lineLength);

            return lineStart + lineDirection * param;
        }

        private RoadSystem.Node GetCloserNodeOnEdge(RoadSystem.Edge edge, Vector3 position)
        {
            float distToStart = Vector3.Distance(edge.StartNode.Position, position);
            float distToEnd = Vector3.Distance(edge.EndNode.Position, position);

            return distToStart < distToEnd ? edge.StartNode : edge.EndNode;
        }

        public void SetAssignedRoute(List<RoadSystem.Node> route)
        {
            assignedRoute = route;
            useAssignedRoute = true;
        }

        [ContextMenu("Paste Allowed Nodes")]
        public void PasteAllowedNodes()
        {
            if (roadSystem == null)
            {
                Debug.LogWarning("CarAgent: RoadSystem not assigned.");
                return;
            }

            List<int> copiedIndices = RoadSystem.GetCopiedNodeIndices();

            if (copiedIndices.Count == 0)
            {
                Debug.LogWarning("CarAgent: No nodes copied in RoadSystem clipboard.");
                return;
            }

            allowedNodeIndices.Clear();
            allowedNodeIndices.AddRange(copiedIndices);

            Debug.Log($"CarAgent: Pasted {allowedNodeIndices.Count} allowed nodes");
        }

        [ContextMenu("Clear Allowed Nodes")]
        public void ClearAllowedNodes()
        {
            allowedNodeIndices.Clear();
            Debug.Log("CarAgent: Allowed nodes cleared");
        }

        private bool IsNodeAllowed(RoadSystem.Node node)
        {
            if (!useAllowedNodesOnly || allowedNodeIndices.Count == 0)
            {
                return true;
            }

            int nodeIndex = roadSystem.Nodes.IndexOf(node);
            return allowedNodeIndices.Contains(nodeIndex);
        }

        private RoadSystem.Node FindClosestAllowedNode()
        {
            if (allowedNodeIndices.Count == 0)
            {
                return null;
            }

            RoadSystem.Node closest = null;
            float minDistance = float.MaxValue;

            foreach (int index in allowedNodeIndices)
            {
                if (index < 0 || index >= roadSystem.Nodes.Count)
                {
                    continue;
                }

                RoadSystem.Node node = roadSystem.Nodes[index];
                float distance = Vector3.Distance(transform.position, node.Position);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = node;
                }
            }

            return closest;
        }

        private RoadSystem.Edge ChooseNextEdge()
        {
            if (currentNode.Edges.Count == 0)
            {
                return null;
            }

            if (useAssignedRoute && assignedRoute != null && assignedRoute.Count > 0)
            {
                return ChooseEdgeForAssignedRoute();
            }

            if (useRandomRoute || targetTransform == null)
            {
                return ChooseRandomEdge();
            }

            return ChooseEdgeTowardsTarget();
        }

        private RoadSystem.Edge ChooseEdgeForAssignedRoute()
        {
            int currentNodeIndexInRoute = assignedRoute.IndexOf(currentNode);

            if (currentNodeIndexInRoute == -1)
            {
                RoadSystem.Node closestRouteNode = FindClosestNodeInRoute();
                if (closestRouteNode != null)
                {
                    List<RoadSystem.Node> pathToRoute = roadSystem.FindPath(currentNode, closestRouteNode);
                    if (pathToRoute != null && pathToRoute.Count >= 2)
                    {
                        List<RoadSystem.Edge> edgesToRoute = roadSystem.ConvertNodePathToEdges(pathToRoute);
                        if (edgesToRoute != null && edgesToRoute.Count > 0)
                        {
                            return edgesToRoute[0];
                        }
                    }
                }

                useAssignedRoute = false;
                return ChooseRandomEdge();
            }

            int nextRouteIndex = (currentNodeIndexInRoute + 1) % assignedRoute.Count;
            RoadSystem.Node targetRouteNode = assignedRoute[nextRouteIndex];

            foreach (RoadSystem.Edge edge in currentNode.Edges)
            {
                if (edge.GetOther(currentNode) == targetRouteNode)
                {
                    return edge;
                }
            }

            List<RoadSystem.Node> pathToNext = roadSystem.FindPath(currentNode, targetRouteNode);
            if (pathToNext != null && pathToNext.Count >= 2)
            {
                List<RoadSystem.Edge> edgesToNext = roadSystem.ConvertNodePathToEdges(pathToNext);
                if (edgesToNext != null && edgesToNext.Count > 0)
                {
                    return edgesToNext[0];
                }
            }

            useAssignedRoute = false;
            return ChooseRandomEdge();
        }

        private RoadSystem.Node FindClosestNodeInRoute()
        {
            if (assignedRoute == null || assignedRoute.Count == 0)
            {
                return null;
            }

            RoadSystem.Node closest = null;
            float minDistance = float.MaxValue;

            foreach (RoadSystem.Node node in assignedRoute)
            {
                float distance = Vector3.Distance(currentNode.Position, node.Position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closest = node;
                }
            }

            return closest;
        }

        private RoadSystem.Edge ChooseRandomEdge()
        {
            List<RoadSystem.Edge> forwardEdges = new List<RoadSystem.Edge>();
            List<RoadSystem.Edge> sidewaysEdges = new List<RoadSystem.Edge>();

            foreach (RoadSystem.Edge edge in currentNode.Edges)
            {
                if (IsSameEdge(edge, previousEdge))
                {
                    continue;
                }

                RoadSystem.Node neighbor = edge.GetOther(currentNode);

                if (!IsNodeAllowed(neighbor))
                {
                    continue;
                }

                if (neighbor != previousNode)
                {
                    forwardEdges.Add(edge);
                }
                else
                {
                    sidewaysEdges.Add(edge);
                }
            }

            List<RoadSystem.Edge> validEdges = forwardEdges.Count > 0 ? forwardEdges : sidewaysEdges;

            if (validEdges.Count == 0)
            {
                return null;
            }

            return validEdges[Random.Range(0, validEdges.Count)];
        }

        private bool IsSameEdge(RoadSystem.Edge edge1, RoadSystem.Edge edge2)
        {
            if (edge1 == null || edge2 == null)
            {
                return false;
            }

            return (edge1.StartNode == edge2.StartNode && edge1.EndNode == edge2.EndNode) ||
                   (edge1.StartNode == edge2.EndNode && edge1.EndNode == edge2.StartNode);
        }

        private RoadSystem.Edge ChooseEdgeTowardsTarget()
        {
            if (targetEdge == null)
            {
                targetEdge = FindClosestEdgeToTarget();
            }

            if (targetEdge == null)
            {
                return ChooseRandomEdge();
            }

            foreach (RoadSystem.Edge edge in currentNode.Edges)
            {
                if (edge == targetEdge)
                {
                    return edge;
                }
            }

            RoadSystem.Node targetNode1 = targetEdge.StartNode;
            RoadSystem.Node targetNode2 = targetEdge.EndNode;

            List<RoadSystem.Node> path1 = roadSystem.FindPath(currentNode, targetNode1);
            List<RoadSystem.Node> path2 = roadSystem.FindPath(currentNode, targetNode2);

            List<RoadSystem.Node> chosenPath = null;

            if (path1 != null && path2 != null)
            {
                chosenPath = path1.Count <= path2.Count ? path1 : path2;
            }
            else if (path1 != null)
            {
                chosenPath = path1;
            }
            else if (path2 != null)
            {
                chosenPath = path2;
            }

            if (chosenPath != null && chosenPath.Count >= 2)
            {
                RoadSystem.Node nextNode = chosenPath[1];

                foreach (RoadSystem.Edge edge in currentNode.Edges)
                {
                    if (edge.GetOther(currentNode) == nextNode)
                    {
                        return edge;
                    }
                }
            }

            return ChooseRandomEdge();
        }

        private float CalculateSplineLength(Spline spline, float startParam, float endParam)
        {
            float length = 0f;
            float minParam = Mathf.Min(startParam, endParam);
            float maxParam = Mathf.Max(startParam, endParam);

            Vector3 prevPos = roadSystem.transform.TransformPoint(spline.EvaluatePosition(minParam));

            for (float param = minParam + 0.01f; param <= maxParam; param += 0.01f)
            {
                Vector3 currentPos = roadSystem.transform.TransformPoint(spline.EvaluatePosition(param));
                length += Vector3.Distance(prevPos, currentPos);
                prevPos = currentPos;
            }

            Vector3 endPos = roadSystem.transform.TransformPoint(spline.EvaluatePosition(maxParam));
            length += Vector3.Distance(prevPos, endPos);

            return Mathf.Max(length, 0.1f);
        }

        private Vector3 GetTangentAtT(Spline spline, float param, float direction)
        {
            float lookAheadDistance = 0.01f * direction;
            float lookAheadParam = Mathf.Clamp01(param + lookAheadDistance);

            Vector3 currentPos = roadSystem.transform.TransformPoint(spline.EvaluatePosition(param));
            Vector3 lookAheadPos = roadSystem.transform.TransformPoint(spline.EvaluatePosition(lookAheadParam));

            return (lookAheadPos - currentPos).normalized;
        }

        private float FindClosestTOnSpline(Spline spline, Vector3 worldPosition, float minT, float maxT)
        {
            Vector3 localPosition = roadSystem.transform.InverseTransformPoint(worldPosition);
            float closestT = minT;
            float minDistance = float.MaxValue;

            float start = Mathf.Min(minT, maxT);
            float end = Mathf.Max(minT, maxT);

            for (float t = start; t <= end; t += 0.01f)
            {
                Vector3 pointOnSpline = spline.EvaluatePosition(t);
                float distance = Vector3.Distance(pointOnSpline, localPosition);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestT = t;
                }
            }

            return closestT;
        }
    }
}
