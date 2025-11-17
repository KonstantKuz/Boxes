using System.Collections.Generic;
using System.Linq;
using Infrastructure.World;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.WorldObject
{
    public class WorldObjectIdListDrawer : OdinAttributeDrawer<WorldObjectIdAttribute, List<string>>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect totalRect = EditorGUILayout.BeginVertical();

            // Draw standard Odin list
            CallNextDrawer(label);

            EditorGUILayout.EndVertical();

            // Handle drag and drop on entire list area
            WorldObjectEditorUtility.HandleMultipleElementDragAndDrop(totalRect, ids =>
            {
                List<string> currentList = ValueEntry.SmartValue ?? new List<string>();
                foreach (var id in ids.Where(id => !currentList.Contains(id)))
                {
                    currentList.Add(id);
                }
                ValueEntry.SmartValue = currentList;
            });
        }
    }
}
