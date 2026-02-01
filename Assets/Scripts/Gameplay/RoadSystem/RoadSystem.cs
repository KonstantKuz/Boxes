using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Splines;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Gameplay.RoadSystem
{
    [ExecuteAlways]
    public class RoadSystem : MonoBehaviour
    {
        public class Node
        {
            public Vector3 Position;
            public List<Edge> Edges = new List<Edge>();

            public override string ToString()
            {
                return Position.ToString("F2");
            }
        }

        public class Edge
        {
            public Node StartNode;
            public Node EndNode;
            public int SplineIndex;
            public float StartT;
            public float EndT;
            public float Length;

            public Node GetOther(Node node)
            {
                return node == StartNode ? EndNode : StartNode;
            }
        }

        [SerializeField] private SplineContainer splineContainer;

        [Header("Graph Building Parameters")]
        [SerializeField] private float nodeSnapDistance = 0.5f;

        [Header("Graph Storage")]
        [SerializeField] private RoadGraph roadGraph;

        [Header("Node Selection")]
        [SerializeField] private List<int> selectedNodeIndices = new List<int>();
        [SerializeField] private Color selectedNodeColor = Color.cyan;
        [SerializeField] private float selectedNodeGizmoSize = 0.2f;

        [Header("Debug")]
        [SerializeField] private bool drawGizmos = true;
        [SerializeField] private Color nodeColor = Color.red;
        [SerializeField] private Color edgeColor = Color.green;
        [SerializeField] private Color pathColor = Color.yellow;
        [SerializeField] private float nodeGizmoSize = 0.15f;
        [SerializeField] private float edgeGizmoWidth = 4f;
        [SerializeField] private float pathGizmoWidth = 6f;
        [SerializeField] private Transform startTransform;
        [SerializeField] private Transform targetTransform;

        private List<Node> nodes = new List<Node>();
        private List<Edge> edges = new List<Edge>();
        private static List<int> copiedNodeIndices = new List<int>();

        public SplineContainer Container => splineContainer;
        public List<Node> Nodes => nodes;
        public List<Edge> Edges => edges;

        private void OnEnable()
        {
            LoadGraphIfNeeded();
        }

        private void LoadGraphIfNeeded()
        {
            if (roadGraph != null && !roadGraph.IsEmpty() && nodes.Count == 0)
            {
                LoadGraphFromAsset();
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                LoadGraphIfNeeded();
            }
        }
#endif

        [ContextMenu("Rebuild Graph")]
        public void RebuildGraph()
        {
            if (splineContainer == null)
            {
                Debug.LogWarning("RoadSystem: SplineContainer not assigned.");
                return;
            }

            nodes.Clear();
            edges.Clear();

            for (int index = 0; index < splineContainer.Splines.Count; index++)
            {
                Spline spline = splineContainer.Splines[index];

                if (spline.Count < 2)
                {
                    continue;
                }

                Vector3 startPos = transform.TransformPoint(spline[0].Position);
                Vector3 endPos = transform.TransformPoint(spline[spline.Count - 1].Position);

                Node startNode = GetOrCreateNode(startPos);
                Node endNode = GetOrCreateNode(endPos);

                if (!EdgeExists(startNode, endNode))
                {
                    CreateEdge(startNode, endNode, index, 0f, 1f);
                }
            }

            Debug.Log($"RoadSystem: Graph rebuilt. Nodes={nodes.Count}, Edges={edges.Count}");
            SaveGraphToAsset();
        }

        [ContextMenu("Save Graph")]
        public void SaveGraphToAsset()
        {
            if (roadGraph == null)
            {
                Debug.LogWarning("RoadSystem: RoadGraph asset not assigned. Create one via Assets/Create/Road System/Road Graph");
                return;
            }

            roadGraph.Clear();

            for (int index = 0; index < nodes.Count; index++)
            {
                Node node = nodes[index];
                RoadGraph.SerializedNode serializedNode = new RoadGraph.SerializedNode
                {
                    position = node.Position
                };

                foreach (Edge edge in node.Edges)
                {
                    int edgeIndex = edges.IndexOf(edge);
                    if (edgeIndex >= 0 && !serializedNode.edgeIndices.Contains(edgeIndex))
                    {
                        serializedNode.edgeIndices.Add(edgeIndex);
                    }
                }

                roadGraph.nodes.Add(serializedNode);
            }

            for (int index = 0; index < edges.Count; index++)
            {
                Edge edge = edges[index];
                RoadGraph.SerializedEdge serializedEdge = new RoadGraph.SerializedEdge
                {
                    startNodeIndex = nodes.IndexOf(edge.StartNode),
                    endNodeIndex = nodes.IndexOf(edge.EndNode),
                    splineIndex = edge.SplineIndex,
                    startT = edge.StartT,
                    endT = edge.EndT,
                    length = edge.Length
                };

                roadGraph.edges.Add(serializedEdge);
            }

#if UNITY_EDITOR
            EditorUtility.SetDirty(roadGraph);
#endif

            Debug.Log($"RoadSystem: Graph saved to {roadGraph.name}");
        }

        [ContextMenu("Load Graph")]
        public void LoadGraphFromAsset()
        {
            if (roadGraph == null)
            {
                Debug.LogWarning("RoadSystem: RoadGraph asset not assigned.");
                return;
            }

            if (roadGraph.IsEmpty())
            {
                Debug.LogWarning("RoadSystem: RoadGraph is empty. Build graph first.");
                return;
            }

            nodes.Clear();
            edges.Clear();

            for (int index = 0; index < roadGraph.nodes.Count; index++)
            {
                RoadGraph.SerializedNode serializedNode = roadGraph.nodes[index];
                Node node = new Node
                {
                    Position = serializedNode.position
                };
                nodes.Add(node);
            }

            for (int index = 0; index < roadGraph.edges.Count; index++)
            {
                RoadGraph.SerializedEdge serializedEdge = roadGraph.edges[index];
                Edge edge = new Edge
                {
                    StartNode = nodes[serializedEdge.startNodeIndex],
                    EndNode = nodes[serializedEdge.endNodeIndex],
                    SplineIndex = serializedEdge.splineIndex,
                    StartT = serializedEdge.startT,
                    EndT = serializedEdge.endT,
                    Length = serializedEdge.length
                };
                edges.Add(edge);
            }

            for (int index = 0; index < nodes.Count; index++)
            {
                Node node = nodes[index];
                RoadGraph.SerializedNode serializedNode = roadGraph.nodes[index];

                foreach (int edgeIndex in serializedNode.edgeIndices)
                {
                    if (edgeIndex >= 0 && edgeIndex < edges.Count)
                    {
                        node.Edges.Add(edges[edgeIndex]);
                    }
                }
            }

            Debug.Log($"RoadSystem: Graph loaded from {roadGraph.name}. Nodes={nodes.Count}, Edges={edges.Count}");
        }

        private Node GetOrCreateNode(Vector3 position)
        {
            foreach (Node node in nodes)
            {
                if (Vector3.Distance(node.Position, position) < nodeSnapDistance)
                {
                    return node;
                }
            }

            Node newNode = new Node { Position = position };
            nodes.Add(newNode);
            return newNode;
        }

        private void CreateEdge(Node start, Node end, int splineIndex, float startT, float endT)
        {
            float length = Vector3.Distance(start.Position, end.Position);

            Edge edge = new Edge
            {
                StartNode = start,
                EndNode = end,
                SplineIndex = splineIndex,
                StartT = startT,
                EndT = endT,
                Length = length
            };

            edges.Add(edge);
            start.Edges.Add(edge);
            end.Edges.Add(edge);
        }

        private bool EdgeExists(Node start, Node end)
        {
            foreach (Edge edge in edges)
            {
                if ((edge.StartNode == start && edge.EndNode == end) ||
                    (edge.StartNode == end && edge.EndNode == start))
                {
                    return true;
                }
            }
            return false;
        }

        public List<Node> FindClosestNodes(Vector3 position, int count = 1)
        {
            return nodes.OrderBy(node => Vector3.Distance(node.Position, position)).Take(count).ToList();
        }

        public List<Node> FindPath(Node start, Node goal)
        {
            if (start == null || goal == null)
            {
                return null;
            }

            if (!nodes.Contains(start) || !nodes.Contains(goal))
            {
                return null;
            }

            List<Node> openSet = new List<Node> { start };
            HashSet<Node> closedSet = new HashSet<Node>();
            Dictionary<Node, Node> cameFrom = new Dictionary<Node, Node>();
            Dictionary<Node, float> gScore = new Dictionary<Node, float>();
            Dictionary<Node, float> fScore = new Dictionary<Node, float>();

            foreach (Node node in nodes)
            {
                gScore[node] = float.PositiveInfinity;
                fScore[node] = float.PositiveInfinity;
            }

            gScore[start] = 0f;
            fScore[start] = Vector3.Distance(start.Position, goal.Position);

            while (openSet.Count > 0)
            {
                openSet.Sort((nodeA, nodeB) => fScore[nodeA].CompareTo(fScore[nodeB]));
                Node current = openSet[0];

                if (current == goal)
                {
                    return ReconstructPath(cameFrom, current);
                }

                openSet.RemoveAt(0);
                closedSet.Add(current);

                foreach (Edge edge in current.Edges)
                {
                    Node neighbor = edge.GetOther(current);

                    if (closedSet.Contains(neighbor))
                    {
                        continue;
                    }

                    if (!gScore.ContainsKey(neighbor))
                    {
                        gScore[neighbor] = float.PositiveInfinity;
                    }

                    if (!fScore.ContainsKey(neighbor))
                    {
                        fScore[neighbor] = float.PositiveInfinity;
                    }

                    float tentativeGScore = gScore[current] + edge.Length;

                    if (tentativeGScore < gScore[neighbor])
                    {
                        cameFrom[neighbor] = current;
                        gScore[neighbor] = tentativeGScore;
                        fScore[neighbor] = tentativeGScore + Vector3.Distance(neighbor.Position, goal.Position);

                        if (!openSet.Contains(neighbor))
                        {
                            openSet.Add(neighbor);
                        }
                    }
                }
            }

            return null;
        }

        private List<Node> ReconstructPath(Dictionary<Node, Node> cameFrom, Node current)
        {
            List<Node> path = new List<Node> { current };

            while (cameFrom.ContainsKey(current))
            {
                current = cameFrom[current];
                path.Add(current);
            }

            path.Reverse();
            return path;
        }

        public List<Edge> ConvertNodePathToEdges(List<Node> path)
        {
            if (path == null || path.Count < 2)
            {
                return null;
            }

            List<Edge> result = new List<Edge>();

            for (int index = 0; index < path.Count - 1; index++)
            {
                Edge edge = edges.Find(e =>
                    (e.StartNode == path[index] && e.EndNode == path[index + 1]) ||
                    (e.StartNode == path[index + 1] && e.EndNode == path[index]));

                if (edge != null)
                {
                    result.Add(edge);
                }
            }

            return result;
        }

        public List<List<Node>> FindAllCycles(int minCycleLength = 4, int maxCycleLength = 20)
        {
            List<List<Node>> cycles = new List<List<Node>>();
            HashSet<string> foundCycles = new HashSet<string>();

            foreach (Node startNode in nodes)
            {
                List<List<Node>> nodeCycles = FindCyclesFromNode(startNode, minCycleLength, maxCycleLength);

                foreach (List<Node> cycle in nodeCycles)
                {
                    string cycleKey = GetCycleKey(cycle);
                    if (!foundCycles.Contains(cycleKey))
                    {
                        foundCycles.Add(cycleKey);
                        cycles.Add(cycle);
                    }
                }
            }

            cycles.Sort((a, b) => a.Count.CompareTo(b.Count));
            return cycles;
        }

        private List<List<Node>> FindCyclesFromNode(Node startNode, int minLength, int maxLength)
        {
            List<List<Node>> cycles = new List<List<Node>>();
            List<Node> path = new List<Node> { startNode };
            HashSet<Node> visited = new HashSet<Node> { startNode };

            DFSFindCycles(startNode, startNode, path, visited, cycles, minLength, maxLength);

            return cycles;
        }

        private void DFSFindCycles(Node current, Node start, List<Node> path, HashSet<Node> visited,
            List<List<Node>> cycles, int minLength, int maxLength)
        {
            if (path.Count > maxLength)
            {
                return;
            }

            foreach (Edge edge in current.Edges)
            {
                Node neighbor = edge.GetOther(current);

                if (neighbor == start && path.Count >= minLength)
                {
                    List<Node> cycle = new List<Node>(path);
                    cycles.Add(cycle);
                    continue;
                }

                if (!visited.Contains(neighbor) && path.Count < maxLength)
                {
                    path.Add(neighbor);
                    visited.Add(neighbor);

                    DFSFindCycles(neighbor, start, path, visited, cycles, minLength, maxLength);

                    path.RemoveAt(path.Count - 1);
                    visited.Remove(neighbor);
                }
            }
        }

        private string GetCycleKey(List<Node> cycle)
        {
            List<int> indices = new List<int>();
            foreach (Node node in cycle)
            {
                indices.Add(nodes.IndexOf(node));
            }

            int minIndex = indices.IndexOf(indices.Min());
            List<int> normalized = new List<int>();

            for (int i = 0; i < indices.Count; i++)
            {
                normalized.Add(indices[(minIndex + i) % indices.Count]);
            }

            List<int> reversed = new List<int>(normalized);
            reversed.Reverse();

            string forward = string.Join(",", normalized);
            string backward = string.Join(",", reversed);

            return string.Compare(forward, backward) < 0 ? forward : backward;
        }

        public float CalculateCycleLength(List<Node> cycle)
        {
            float totalLength = 0f;

            for (int i = 0; i < cycle.Count; i++)
            {
                Node current = cycle[i];
                Node next = cycle[(i + 1) % cycle.Count];

                foreach (Edge edge in current.Edges)
                {
                    if (edge.GetOther(current) == next)
                    {
                        totalLength += edge.Length;
                        break;
                    }
                }
            }

            return totalLength;
        }

#if UNITY_EDITOR
        [ContextMenu("Clear Node Selection")]
        public void ClearNodeSelection()
        {
            selectedNodeIndices.Clear();
            Debug.Log("RoadSystem: Node selection cleared");
        }

        [ContextMenu("Copy Selected Nodes")]
        public void CopySelectedNodes()
        {
            if (selectedNodeIndices.Count == 0)
            {
                Debug.LogWarning("RoadSystem: No nodes selected to copy");
                return;
            }

            copiedNodeIndices.Clear();
            copiedNodeIndices.AddRange(selectedNodeIndices);
            Debug.Log($"RoadSystem: Copied {copiedNodeIndices.Count} nodes to clipboard");
        }

        public void ToggleNodeSelection(int nodeIndex)
        {
            if (nodeIndex < 0 || nodeIndex >= nodes.Count)
            {
                return;
            }

            if (selectedNodeIndices.Contains(nodeIndex))
            {
                selectedNodeIndices.Remove(nodeIndex);
            }
            else
            {
                selectedNodeIndices.Add(nodeIndex);
            }

            EditorUtility.SetDirty(this);
        }

        public bool IsNodeSelected(int nodeIndex)
        {
            return selectedNodeIndices.Contains(nodeIndex);
        }

        public static List<int> GetCopiedNodeIndices()
        {
            return new List<int>(copiedNodeIndices);
        }

        public List<Node> GetSelectedNodes()
        {
            List<Node> result = new List<Node>();
            foreach (int index in selectedNodeIndices)
            {
                if (index >= 0 && index < nodes.Count)
                {
                    result.Add(nodes[index]);
                }
            }
            return result;
        }
        private void OnDrawGizmos()
        {
            if (!drawGizmos || splineContainer == null)
            {
                return;
            }

            if (!Application.isPlaying && nodes.Count == 0 && edges.Count == 0)
            {
                return;
            }

            DrawSplines();
            DrawNodes();
            DrawEdges();
            DrawDebugPath();
        }

        private void DrawSplines()
        {
            Gizmos.color = Color.gray;

            foreach (Spline spline in splineContainer.Splines)
            {
                Vector3 previousPosition = transform.TransformPoint(spline.EvaluatePosition(0f));

                for (float param = 0.01f; param <= 1.0f + Mathf.Epsilon; param += 0.01f)
                {
                    Vector3 currentPosition = transform.TransformPoint(spline.EvaluatePosition(param));
                    Gizmos.DrawLine(previousPosition, currentPosition);
                    previousPosition = currentPosition;
                }
            }
        }

        private void DrawNodes()
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                Node node = nodes[i];
                bool isSelected = selectedNodeIndices.Contains(i);

                Gizmos.color = isSelected ? selectedNodeColor : nodeColor;
                float size = isSelected ? selectedNodeGizmoSize : nodeGizmoSize;

                Gizmos.DrawSphere(node.Position, size);
            }
        }

        private void DrawEdges()
        {
            if (Camera.current == null)
            {
                return;
            }

            Handles.color = edgeColor;

            foreach (Edge edge in edges)
            {
                Handles.DrawAAPolyLine(edgeGizmoWidth, edge.StartNode.Position, edge.EndNode.Position);
            }
        }

        private void DrawDebugPath()
        {
            if (Camera.current == null)
            {
                return;
            }

            if (startTransform == null || targetTransform == null)
            {
                return;
            }

            if (nodes.Count == 0 || edges.Count == 0)
            {
                return;
            }

            (int startSplineIndex, float startT, Vector3 startProjectedPos) = FindClosestPointOnSplines(startTransform.position);
            (int targetSplineIndex, float targetT, Vector3 targetProjectedPos) = FindClosestPointOnSplines(targetTransform.position);

            if (startSplineIndex < 0 || targetSplineIndex < 0)
            {
                return;
            }

            Gizmos.color = pathColor;
            Gizmos.DrawSphere(startProjectedPos, nodeGizmoSize * 1.5f);
            Gizmos.DrawSphere(targetProjectedPos, nodeGizmoSize * 1.5f);

            Edge startEdge = FindEdgeContainingPoint(startSplineIndex, startT);
            Edge targetEdge = FindEdgeContainingPoint(targetSplineIndex, targetT);

            if (startEdge == null || targetEdge == null)
            {
                return;
            }

            Handles.color = pathColor;

            if (startEdge == targetEdge)
            {
                Handles.DrawAAPolyLine(pathGizmoWidth, startProjectedPos, targetProjectedPos);
                return;
            }

            (Node startNode, Node startNextNode) = GetNodesForPathfinding(startEdge, startT);
            (Node targetNode, Node targetPrevNode) = GetNodesForPathfinding(targetEdge, targetT);

            if (startNode == null || targetNode == null)
            {
                return;
            }

            if (startNode == targetNode)
            {
                Handles.DrawAAPolyLine(pathGizmoWidth, startProjectedPos, startNode.Position, targetProjectedPos);
                return;
            }

            List<Node> nodePath = FindPath(startNode, targetNode);

            if (nodePath == null || nodePath.Count == 0)
            {
                return;
            }

            Handles.DrawAAPolyLine(pathGizmoWidth, startProjectedPos, nodePath[0].Position);

            for (int index = 0; index < nodePath.Count - 1; index++)
            {
                Handles.DrawAAPolyLine(pathGizmoWidth, nodePath[index].Position, nodePath[index + 1].Position);
            }

            Handles.DrawAAPolyLine(pathGizmoWidth, nodePath[nodePath.Count - 1].Position, targetProjectedPos);
        }

        private (int splineIndex, float t, Vector3 position) FindClosestPointOnSplines(Vector3 worldPosition)
        {
            int closestSplineIndex = -1;
            float closestT = 0f;
            float minDistance = float.MaxValue;

            for (int index = 0; index < splineContainer.Splines.Count; index++)
            {
                Spline spline = splineContainer.Splines[index];
                Vector3 localPosition = transform.InverseTransformPoint(worldPosition);

                for (float param = 0; param <= 1f; param += 0.01f)
                {
                    Vector3 pointOnSpline = spline.EvaluatePosition(param);
                    float distance = Vector3.Distance(pointOnSpline, localPosition);

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        closestSplineIndex = index;
                        closestT = param;
                    }
                }
            }

            if (closestSplineIndex >= 0)
            {
                Spline spline = splineContainer.Splines[closestSplineIndex];
                Vector3 position = transform.TransformPoint(spline.EvaluatePosition(closestT));
                return (closestSplineIndex, closestT, position);
            }

            return (-1, 0f, Vector3.zero);
        }

        private Edge FindEdgeContainingPoint(int splineIndex, float param)
        {
            foreach (Edge edge in edges)
            {
                if (edge.SplineIndex != splineIndex)
                {
                    continue;
                }

                float minT = Mathf.Min(edge.StartT, edge.EndT);
                float maxT = Mathf.Max(edge.StartT, edge.EndT);

                if (param >= minT && param <= maxT)
                {
                    return edge;
                }
            }

            return null;
        }

        private (Node mainNode, Node nextNode) GetNodesForPathfinding(Edge edge, float param)
        {
            float midT = (edge.StartT + edge.EndT) * 0.5f;

            if (Mathf.Abs(param - midT) < 0.01f)
            {
                return (edge.StartNode, edge.EndNode);
            }

            bool closerToStart = Mathf.Abs(param - edge.StartT) < Mathf.Abs(param - edge.EndT);

            if (closerToStart)
            {
                return (edge.StartNode, edge.EndNode);
            }
            else
            {
                return (edge.EndNode, edge.StartNode);
            }
        }
#endif
    }
}
