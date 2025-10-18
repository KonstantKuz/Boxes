using System.Collections.Generic;
using UnityEngine;

namespace Gameplay.RoadSystem
{
    [CreateAssetMenu(fileName = "RoadGraph", menuName = "Road System/Road Graph")]
    public class RoadGraph : ScriptableObject
    {
        [System.Serializable]
        public class SerializedNode
        {
            public Vector3 position;
            public List<int> edgeIndices = new List<int>();
        }

        [System.Serializable]
        public class SerializedEdge
        {
            public int startNodeIndex;
            public int endNodeIndex;
            public int splineIndex;
            public float startT;
            public float endT;
            public float length;
        }

        public List<SerializedNode> nodes = new List<SerializedNode>();
        public List<SerializedEdge> edges = new List<SerializedEdge>();

        public void Clear()
        {
            nodes.Clear();
            edges.Clear();
        }

        public bool IsEmpty()
        {
            return nodes.Count == 0 && edges.Count == 0;
        }
    }
}
