using System.Collections.Generic;
using System.Linq;
using Infrastructure.World;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.WorldObject
{
    public class WorldObjectIdListDrawer : OdinAttributeDrawer<WorldObjectIdAttribute, List<string>>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect totalRect = EditorGUILayout.BeginVertical();

            CallNextDrawer(label);

            EditorGUILayout.EndVertical();

            WorldObjectEditorUtility.HandleMultipleElementDragAndDrop(totalRect, ids =>
            {
                Property.RecordForUndo("Add WorldObject IDs");
                List<string> currentList = ValueEntry.SmartValue ?? new List<string>();
                foreach (var id in ids.Where(id => !currentList.Contains(id)))
                {
                    currentList.Add(id);
                }
                ValueEntry.SmartValue = currentList;
            });
        }
    }

    public class WorldObjectIdStringElementDrawer : OdinValueDrawer<string>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            WorldObjectIdAttribute parentAttribute = Property.Parent?.Parent?.GetAttribute<WorldObjectIdAttribute>();

            if (parentAttribute == null)
            {
                CallNextDrawer(label);
                return;
            }

            EditorGUILayout.BeginHorizontal();

            CallNextDrawer(label);

            WorldObjectEditorUtility.DrawWorldObjectField(ValueEntry.SmartValue, parentAttribute.ShowWarning, 120);

            EditorGUILayout.EndHorizontal();
        }
    }
}
