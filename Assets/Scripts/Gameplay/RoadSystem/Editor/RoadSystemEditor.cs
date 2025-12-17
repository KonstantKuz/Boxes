using UnityEngine;
using UnityEditor;

namespace Gameplay.RoadSystem
{
    [CustomEditor(typeof(RoadSystem))]
    public class RoadSystemEditor : Editor
    {
        private RoadSystem roadSystem;
        private const float NODE_CLICK_DISTANCE = 0.3f;

        private void OnEnable()
        {
            roadSystem = (RoadSystem)target;
        }

        protected void OnSceneGUI()
        {
            if (roadSystem == null || roadSystem.Nodes == null || roadSystem.Nodes.Count == 0)
            {
                return;
            }

            Event e = Event.current;

            if (e.type == EventType.MouseDown && e.button == 0 && e.shift)
            {
                Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);

                float closestDistance = float.MaxValue;
                int closestNodeIndex = -1;

                for (int i = 0; i < roadSystem.Nodes.Count; i++)
                {
                    RoadSystem.Node node = roadSystem.Nodes[i];
                    Vector3 nodePosition = node.Position;

                    float distance = HandleUtility.DistanceToCircle(nodePosition, NODE_CLICK_DISTANCE);

                    if (distance < closestDistance && distance < 10f)
                    {
                        closestDistance = distance;
                        closestNodeIndex = i;
                    }
                }

                if (closestNodeIndex >= 0)
                {
                    roadSystem.ToggleNodeSelection(closestNodeIndex);
                    e.Use();
                    SceneView.RepaintAll();
                }
            }

            if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
            {
                roadSystem.ClearNodeSelection();
                e.Use();
                SceneView.RepaintAll();
            }
        }
    }
}