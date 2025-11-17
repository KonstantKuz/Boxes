using System.Collections.Generic;
using System.Linq;
using Infrastructure.World;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.WorldObject
{
    public class WorldObjectIdArrayDrawer : OdinAttributeDrawer<WorldObjectIdAttribute, string[]>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect totalRect = EditorGUILayout.BeginVertical();

            CallNextDrawer(label);

            EditorGUILayout.EndVertical();

            WorldObjectEditorUtility.HandleMultipleElementDragAndDrop(totalRect, ids =>
            {
                Property.RecordForUndo("Add WorldObject IDs");
                List<string> currentList = ValueEntry.SmartValue?.ToList() ?? new List<string>();
                foreach (string id in ids.Where(id => !currentList.Contains(id)))
                {
                    currentList.Add(id);
                }
                ValueEntry.SmartValue = currentList.ToArray();
            });
        }
    }
}
