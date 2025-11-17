using Infrastructure.World;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;

namespace Editor.WorldObject
{
    public class WorldObjectIdDrawer : OdinAttributeDrawer<WorldObjectIdAttribute, string>
    {
        protected override void DrawPropertyLayout(GUIContent label)
        {
            Rect rect = EditorGUILayout.GetControlRect();

            if (label != null)
            {
                rect = EditorGUI.PrefixLabel(rect, label);
            }

            Rect fieldRect = new Rect(rect.x, rect.y, rect.width - 40, rect.height);
            Rect findButtonRect = new Rect(rect.x + rect.width - 35, rect.y, 35, rect.height);

            string newValue = EditorGUI.TextField(fieldRect, this.ValueEntry.SmartValue);

            if (newValue != ValueEntry.SmartValue)
            {
                this.ValueEntry.SmartValue = newValue;
            }

            GUI.enabled = !string.IsNullOrEmpty(this.ValueEntry.SmartValue);
            if (GUI.Button(findButtonRect, "Find"))
            {
                WorldObjectEditorUtility.FindAndSelectWorldObject(this.ValueEntry.SmartValue);
            }
            GUI.enabled = true;

            WorldObjectEditorUtility.HandleSingleElementDragAndDrop(rect, id =>
            {
                this.ValueEntry.SmartValue = id;
            });
        }
    }
}
