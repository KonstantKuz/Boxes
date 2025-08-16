using System;
using System.Reflection;
using UnityEngine;

namespace Infrastructure.Cheats
{
    public class ConfigRenderer<T> where T : ScriptableObject
    {
        private readonly T config;

        public bool IsOpen { get; set; }

        public ConfigRenderer(T config)
        {
            this.config = config;
        }

        public void RenderCheats()
        {
            FieldInfo[] fields =
                typeof(T).GetFields(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (FieldInfo field in fields)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(field.Name);

                object currentValue = field.GetValue(config);

                if (field.FieldType == typeof(float))
                {
                    float value = (float)currentValue;
                    string newText = GUILayout.TextField(value.ToString());
                    if (float.TryParse(newText, out float newValue))
                    {
                        field.SetValue(config, newValue);
                    }
                }
                else if (field.FieldType == typeof(int))
                {
                    int value = (int)currentValue;
                    string newText = GUILayout.TextField(value.ToString());
                    if (int.TryParse(newText, out int newValue))
                    {
                        field.SetValue(config, newValue);
                    }
                }
                else if (field.FieldType == typeof(bool))
                {
                    bool value = (bool)currentValue;
                    bool newValue = GUILayout.Toggle(value, "");
                    field.SetValue(config, newValue);
                }
                else if (field.FieldType == typeof(string))
                {
                    string value = (string)currentValue;
                    string newValue = GUILayout.TextField(value);
                    field.SetValue(config, newValue);
                }
                else if (field.FieldType.IsEnum)
                {
                    if (field.FieldType.GetCustomAttribute<FlagsAttribute>() != null)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginVertical("box");
                        Array enumValues = Enum.GetValues(field.FieldType);
                        int combinedValue = (int)currentValue;

                        foreach (var enumValue in enumValues)
                        {
                            int enumInt = (int)enumValue;
                            if (enumInt == 0) continue;

                            bool isSet = (combinedValue & enumInt) != 0;
                            bool newIsSet = GUILayout.Toggle(isSet, enumValue.ToString());

                            if (newIsSet != isSet)
                            {
                                if (newIsSet)
                                {
                                    combinedValue |= enumInt;
                                }
                                else
                                {
                                    combinedValue &= ~enumInt;
                                }

                                field.SetValue(config, Enum.ToObject(field.FieldType, combinedValue));
                            }
                        }

                        GUILayout.EndVertical();
                        GUILayout.BeginHorizontal();
                    }
                    else
                    {
                        string[] enumNames = Enum.GetNames(field.FieldType);
                        int currentEnumIndex = Array.IndexOf(enumNames, currentValue.ToString());
                        int newEnumIndex = GUILayout.SelectionGrid(
                            currentEnumIndex, enumNames, enumNames.Length > 3 ? 3 : enumNames.Length
                        );

                        if (newEnumIndex != currentEnumIndex)
                        {
                            object newEnumValue = Enum.Parse(field.FieldType, enumNames[newEnumIndex]);
                            field.SetValue(config, newEnumValue);
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}
