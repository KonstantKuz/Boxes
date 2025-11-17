using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.WorldObject
{
    public static class WorldObjectEditorUtility
    {
        public static string GetWorldObjectId(Infrastructure.World.WorldObject worldObject)
        {
            if (worldObject == null) return null;

            var idField = typeof(Infrastructure.World.WorldObject).GetField(
                "id", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance
            );

            return idField?.GetValue(worldObject) as string;
        }

        public static Infrastructure.World.WorldObject TryGetWorldObjectFromDrag()
        {
            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is GameObject go)
                {
                    Infrastructure.World.WorldObject worldObject = go.GetComponent<Infrastructure.World.WorldObject>();
                    if (worldObject != null)
                    {
                        return worldObject;
                    }
                }
                else if (draggedObject is Infrastructure.World.WorldObject wo)
                {
                    return wo;
                }
            }
            return null;
        }

        public static List<Infrastructure.World.WorldObject> GetAllWorldObjectsFromDrag()
        {
            List<Infrastructure.World.WorldObject> worldObjects = new List<Infrastructure.World.WorldObject>();

            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is GameObject go)
                {
                    Infrastructure.World.WorldObject worldObject = go.GetComponent<Infrastructure.World.WorldObject>();
                    if (worldObject != null)
                    {
                        worldObjects.Add(worldObject);
                    }
                }
                else if (draggedObject is Infrastructure.World.WorldObject wo)
                {
                    worldObjects.Add(wo);
                }
            }

            return worldObjects;
        }

        public static bool HandleSingleElementDragAndDrop(Rect rect, System.Action<string> onIdReceived)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
                return false;

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                Infrastructure.World.WorldObject worldObject = TryGetWorldObjectFromDrag();

                if (worldObject != null)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        string id = GetWorldObjectId(worldObject);
                        if (!string.IsNullOrEmpty(id))
                        {
                            onIdReceived?.Invoke(id);
                        }
                    }

                    evt.Use();
                    return true;
                }
            }

            return false;
        }

        public static bool HandleMultipleElementDragAndDrop(Rect rect, System.Action<List<string>> onIdsReceived)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
                return false;

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                List<Infrastructure.World.WorldObject> worldObjects = GetAllWorldObjectsFromDrag();

                if (worldObjects.Count > 0)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;

                    if (evt.type == EventType.DragPerform)
                    {
                        DragAndDrop.AcceptDrag();

                        List<string> ids = worldObjects
                            .Select(GetWorldObjectId)
                            .Where(id => !string.IsNullOrEmpty(id))
                            .ToList();

                        if (ids.Count > 0)
                        {
                            onIdsReceived?.Invoke(ids);
                        }
                    }

                    evt.Use();
                    return true;
                }
            }

            return false;
        }

        public static void DrawFindButton(string id, float width = 50)
        {
            GUI.enabled = !string.IsNullOrEmpty(id);
            if (GUILayout.Button("Find", GUILayout.Width(width)))
            {
                FindAndSelectWorldObject(id);
            }
            GUI.enabled = true;
        }

        public static void FindAndSelectWorldObject(string idString)
        {
            if (string.IsNullOrEmpty(idString))
                return;

            if (!Guid.TryParse(idString, out Guid targetId))
            {
                Debug.LogWarning($"Invalid GUID format: {idString}");
                return;
            }

            // If in prefab mode, search there first
            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                Infrastructure.World.WorldObject[] prefabWorldObjects = prefabStage.prefabContentsRoot.GetComponentsInChildren<Infrastructure.World.WorldObject>(true);

                foreach (var worldObject in prefabWorldObjects)
                {
                    string id = GetWorldObjectId(worldObject);
                    if (id == idString)
                    {
                        EditorGUIUtility.PingObject(worldObject.gameObject);
                        return;
                    }
                }
            }

            // Search in all loaded scenes
            Infrastructure.World.WorldObject[] allWorldObjects = Object.FindObjectsOfType<Infrastructure.World.WorldObject>(true);

            foreach (var worldObject in allWorldObjects)
            {
                string id = GetWorldObjectId(worldObject);
                if (id == idString)
                {
                    EditorGUIUtility.PingObject(worldObject.gameObject);
                    return;
                }
            }

            Debug.LogWarning($"WorldObject with ID {idString} not found in any loaded scene or prefab.");
        }
    }
}
