using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor.WorldObject
{
    using WorldObject = Infrastructure.World.WorldObject;

    public static class WorldObjectEditorUtility
    {
        public static string GetWorldObjectId(WorldObject worldObject)
        {
            if (worldObject == null)
            {
                return null;
            }

            FieldInfo idField =
                typeof(WorldObject).GetField("id", BindingFlags.NonPublic | BindingFlags.Instance);

            return idField?.GetValue(worldObject) as string;
        }

        public static WorldObject TryGetWorldObjectFromDrag()
        {
            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is GameObject go)
                {
                    WorldObject worldObject = go.GetComponent<WorldObject>();
                    if (worldObject != null)
                    {
                        return worldObject;
                    }
                }
                else if (draggedObject is WorldObject wo)
                {
                    return wo;
                }
            }
            return null;
        }

        public static List<WorldObject> GetAllWorldObjectsFromDrag()
        {
            List<WorldObject> worldObjects = new List<WorldObject>();

            foreach (Object draggedObject in DragAndDrop.objectReferences)
            {
                if (draggedObject is GameObject go)
                {
                    WorldObject worldObject = go.GetComponent<WorldObject>();
                    if (worldObject != null)
                    {
                        worldObjects.Add(worldObject);
                    }
                }
                else if (draggedObject is WorldObject wo)
                {
                    worldObjects.Add(wo);
                }
            }

            return worldObjects;
        }

        public static bool HandleSingleElementDragAndDrop(Rect rect, Action<string> onIdReceived)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
            {
                return false;
            }

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                WorldObject worldObject = TryGetWorldObjectFromDrag();

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

        public static bool HandleMultipleElementDragAndDrop(Rect rect, Action<List<string>> onIdsReceived)
        {
            Event evt = Event.current;
            if (!rect.Contains(evt.mousePosition))
            {
                return false;
            }

            if (evt.type == EventType.DragUpdated || evt.type == EventType.DragPerform)
            {
                List<WorldObject> worldObjects = GetAllWorldObjectsFromDrag();

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

        public static void DrawWorldObjectField(string id, bool showWarning, float width = 150)
        {
            WorldObject worldObject = FindWorldObjectById(id);

            if (showWarning && string.IsNullOrEmpty(id))
            {
                GUIContent warningContent = EditorGUIUtility.IconContent("console.warnicon.sml");
                GUILayout.Label(warningContent, GUILayout.Width(20));
            }

            GUI.enabled = false;
            EditorGUILayout.ObjectField(worldObject?.gameObject, typeof(GameObject), true, GUILayout.Width(width));
            GUI.enabled = true;
        }

        public static WorldObject FindWorldObjectById(string idString)
        {
            if (string.IsNullOrEmpty(idString))
            {
                return null;
            }

            if (!Guid.TryParse(idString, out Guid targetId))
            {
                return null;
            }

            PrefabStage prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
            {
                WorldObject[] prefabWorldObjects =
                    prefabStage.prefabContentsRoot.GetComponentsInChildren<WorldObject>(true);

                foreach (WorldObject worldObject in prefabWorldObjects)
                {
                    string id = GetWorldObjectId(worldObject);
                    if (id == idString)
                    {
                        return worldObject;
                    }
                }
            }

            WorldObject[] allWorldObjects =
                Object.FindObjectsOfType<WorldObject>(true);

            foreach (WorldObject worldObject in allWorldObjects)
            {
                string id = GetWorldObjectId(worldObject);
                if (id == idString)
                {
                    return worldObject;
                }
            }

            return null;
        }
    }
}
