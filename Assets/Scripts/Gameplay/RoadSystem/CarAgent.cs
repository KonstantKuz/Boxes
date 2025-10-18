using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Gameplay.RoadSystem
{
    [ExecuteAlways]
    [RequireComponent(typeof(Transform))]
    public class CarAgent : MonoBehaviour
    {
        [Header("References")]
        public RoadSystem roadSystem;
        public Transform targetTransform;

        [Header("Movement")]
        public float speed = 5f;
        public float rotationSpeed = 5f;
        public float nodeReachDistance = 0.5f;
        public float laneOffset;

        [Header("Route Mode")]
        public bool useRandomRoute = false;
        public bool useAssignedRoute = false;
        public bool isDriving = false;

        [Header("Target Settings")]
        public float targetReachedDistance = 1f;
        public float targetMovementThreshold = 2f;

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
        private Vector3 lastTargetPosition;
        private RoadSystem.Node cachedTargetNode;
        private List<RoadSystem.Node> assignedRoute;
        private int currentRouteIndex;

        [ContextMenu("Start Driving")]
        public void SetDriving(bool value)
        {
            if (!value)
            {
                isDriving = false;
                isInitialized = false;
                return;
            }

            if (roadSystem == null)
            {
                Debug.LogWarning("CarAgent: RoadSystem not assigned.");
                return;
            }

            isDriving = true;
            isInitialized = false;
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

            if (!useRandomRoute && targetTransform != null)
            {
                CheckTargetMovement();
            }

            UpdateDriving();
        }

        private void CheckTargetMovement()
        {
            float movedDistance = Vector3.Distance(targetTransform.position, lastTargetPosition);

            if (hasReachedTarget)
            {
                if (movedDistance > targetReachedDistance * 0.5f)
                {
                    hasReachedTarget = false;
                    cachedTargetNode = null;
                }
            }
            else if (movedDistance > targetMovementThreshold)
            {
                RoadSystem.Node newTargetNode = GetClosestNode(targetTransform.position);
                if (newTargetNode != cachedTargetNode)
                {
                    cachedTargetNode = newTargetNode;
                }
            }

            lastTargetPosition = targetTransform.position;
        }

        private void InitializeDriving()
        {
            // Если есть назначенный маршрут, начинаем с первой ноды маршрута
            if (useAssignedRoute && assignedRoute != null && assignedRoute.Count > 0)
            {
                currentNode = assignedRoute[0];
                currentRouteIndex = 0;
            }
            else
            {
                // Иначе ищем ближайший край
                RoadSystem.Edge closestEdge = FindClosestEdge(transform.position);
                if (closestEdge == null)
                {
                    Debug.LogError("CarAgent: No edges found in road system");
                    isDriving = false;
                    return;
                }

                currentNode = GetCloserNodeOnEdge(closestEdge, transform.position);
            }

            previousNode = null;
            previousEdge = null;
            hasReachedTarget = false;
            cachedTargetNode = null;

            if (targetTransform != null)
            {
                lastTargetPosition = targetTransform.position;
                cachedTargetNode = GetClosestNode(targetTransform.position);
            }

            List<RoadSystem.Edge> validEdges = GetValidEdgesFromNode(currentNode);
            if (validEdges.Count == 0)
            {
                Debug.LogError("CarAgent: No valid edges from start node");
                isDriving = false;
                return;
            }

            StartNewEdge();
            isInitialized = true;
        }

        private RoadSystem.Node FindNodeWithEdges()
        {
            RoadSystem.Node bestNode = null;
            float minDistance = float.MaxValue;

            foreach (RoadSystem.Node node in roadSystem.Nodes)
            {
                if (node.Edges.Count > 0)
                {
                    float distance = Vector3.Distance(transform.position, node.Position);
                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        bestNode = node;
                    }
                }
            }

            return bestNode;
        }

        private void UpdateDriving()
        {
            if (!useRandomRoute && targetTransform != null && IsCloseToTarget())
            {
                if (!hasReachedTarget)
                {
                    hasReachedTarget = true;
                    currentEdge = null;
                    previousEdge = null;
                }
                return;
            }

            if (currentEdge == null)
            {
                StartNewEdge();
                return;
            }

            Spline spline = roadSystem.Container.Splines[currentEdge.SplineIndex];
            float direction = movingForward ? 1f : -1f;

            Vector3 centerPosition = roadSystem.transform.TransformPoint(spline.EvaluatePosition(currentT));
            Vector3 tangent = GetTangentAtT(spline, currentT, direction);

            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            centerPosition += right * laneOffset;
            transform.position = centerPosition;

            if (tangent.sqrMagnitude > 0.001f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(tangent);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }

            if (!useRandomRoute && targetTransform != null && ShouldStopOnEdge(spline))
            {
                hasReachedTarget = true;
                currentEdge = null;
                previousEdge = null;
                return;
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

            float tStep = (speed * Time.deltaTime) / splineLength * Mathf.Abs(endT - startT);
            currentT += tStep * direction;

            if ((direction > 0 && currentT >= endT) || (direction < 0 && currentT <= endT))
            {
                previousNode = currentNode;
                previousEdge = currentEdge;
                currentNode = targetNode;
                currentEdge = null;
            }
        }

        private bool IsCloseToTarget()
        {
            float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
            return distanceToTarget <= targetReachedDistance;
        }

        private bool ShouldStopOnEdge(Spline spline)
        {
            Vector3 closestPointOnSpline = FindClosestPointOnCurrentEdge(spline);
            float distanceFromClosestToTarget = Vector3.Distance(closestPointOnSpline, targetTransform.position);
            float distanceFromCarToClosest = Vector3.Distance(transform.position, closestPointOnSpline);

            return distanceFromCarToClosest <= nodeReachDistance && distanceFromClosestToTarget <= targetReachedDistance;
        }

        private Vector3 FindClosestPointOnCurrentEdge(Spline spline)
        {
            Vector3 localTargetPosition = roadSystem.transform.InverseTransformPoint(targetTransform.position);
            float closestT = currentT;
            float minDistance = float.MaxValue;

            float minT = Mathf.Min(startT, endT);
            float maxT = Mathf.Max(startT, endT);

            for (float param = minT; param <= maxT; param += 0.01f)
            {
                Vector3 pointOnSpline = spline.EvaluatePosition(param);
                float distance = Vector3.Distance(pointOnSpline, localTargetPosition);

                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestT = param;
                }
            }

            return roadSystem.transform.TransformPoint(spline.EvaluatePosition(closestT));
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
            currentT = startT;

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
            currentRouteIndex = 0;
            useAssignedRoute = true;
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
                return ChooseRandomValidEdge();
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
                return ChooseRandomValidEdge();
            }

            int nextRouteIndex = (currentNodeIndexInRoute + 1) % assignedRoute.Count;
            RoadSystem.Node targetRouteNode = assignedRoute[nextRouteIndex];

            foreach (RoadSystem.Edge edge in currentNode.Edges)
            {
                if (edge.GetOther(currentNode) == targetRouteNode)
                {
                    currentRouteIndex = nextRouteIndex;
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
            return ChooseRandomValidEdge();
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

        private RoadSystem.Edge ChooseRandomValidEdge()
        {
            List<RoadSystem.Edge> forwardEdges = new List<RoadSystem.Edge>();

            foreach (RoadSystem.Edge edge in currentNode.Edges)
            {
                if (IsSameEdge(edge, previousEdge))
                {
                    continue;
                }

                RoadSystem.Node neighbor = edge.GetOther(currentNode);
                if (neighbor != previousNode)
                {
                    forwardEdges.Add(edge);
                }
            }

            if (forwardEdges.Count > 0)
            {
                int randomIndex = Random.Range(0, forwardEdges.Count);
                return forwardEdges[randomIndex];
            }

            List<RoadSystem.Edge> edgesWithoutPrevious = new List<RoadSystem.Edge>();
            foreach (RoadSystem.Edge edge in currentNode.Edges)
            {
                if (!IsSameEdge(edge, previousEdge))
                {
                    edgesWithoutPrevious.Add(edge);
                }
            }

            if (edgesWithoutPrevious.Count > 0)
            {
                int randomIndex = Random.Range(0, edgesWithoutPrevious.Count);
                return edgesWithoutPrevious[randomIndex];
            }

            if (currentNode.Edges.Count > 0)
            {
                int randomIndex = Random.Range(0, currentNode.Edges.Count);
                return currentNode.Edges[randomIndex];
            }

            return null;
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
            if (cachedTargetNode == null)
            {
                cachedTargetNode = GetClosestNode(targetTransform.position);
            }

            if (cachedTargetNode == null)
            {
                return ChooseRandomValidEdge();
            }

            List<RoadSystem.Node> path = roadSystem.FindPath(currentNode, cachedTargetNode);

            if (path == null || path.Count < 2)
            {
                return ChooseRandomValidEdge();
            }

            List<RoadSystem.Edge> pathEdges = roadSystem.ConvertNodePathToEdges(path);

            if (pathEdges == null || pathEdges.Count == 0)
            {
                return ChooseRandomValidEdge();
            }

            return pathEdges[0];
        }

        private List<RoadSystem.Edge> GetValidEdgesFromNode(RoadSystem.Node node)
        {
            return node.Edges;
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
    }
}
