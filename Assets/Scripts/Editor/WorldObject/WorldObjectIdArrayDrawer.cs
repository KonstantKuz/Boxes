using System.Collections.Generic;
using System.Linq;
using Infrastructure.World;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.WorldObject
{
    public class WorldObjectIdArrayDrawer : OdinAttributeDrawer<WorldObjectIdAttribute, string[]>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect totalRect = EditorGUILayout.BeginVertical();

            // Draw standard Odin array
            CallNextDrawer(label);

            EditorGUILayout.EndVertical();

            // Handle drag and drop on entire array area
            WorldObjectEditorUtility.HandleMultipleElementDragAndDrop(totalRect, ids =>
            {
                List<string> currentList = ValueEntry.SmartValue?.ToList() ?? new List<string>();
                foreach (var id in ids.Where(id => !currentList.Contains(id)))
                {
                    currentList.Add(id);
                }
                ValueEntry.SmartValue = currentList.ToArray();
            });
        }
    }
}
