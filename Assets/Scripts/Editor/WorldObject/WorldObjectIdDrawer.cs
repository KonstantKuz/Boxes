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
            EditorGUILayout.BeginHorizontal();

            if (label != null)
            {
                EditorGUILayout.PrefixLabel(label);
            }

            Rect stringFieldRect = EditorGUILayout.GetControlRect(GUILayout.ExpandWidth(true));
            string newValue = EditorGUI.TextField(stringFieldRect, ValueEntry.SmartValue);

            if (newValue != ValueEntry.SmartValue)
            {
                ValueEntry.SmartValue = newValue;
            }

            WorldObjectEditorUtility.HandleSingleElementDragAndDrop(stringFieldRect, id =>
            {
                Property.RecordForUndo("Change WorldObject ID");
                ValueEntry.SmartValue = id;
            });

            WorldObjectEditorUtility.DrawWorldObjectField(ValueEntry.SmartValue, Attribute.ShowWarning);

            EditorGUILayout.EndHorizontal();
        }
    }
}
