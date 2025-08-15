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
            PropertyInfo[] properties =
                typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

            foreach (PropertyInfo property in properties)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(property.Name);

                object currentValue = property.GetValue(config);

                if (property.PropertyType == typeof(float))
                {
                    float value = (float)currentValue;
                    string newText = GUILayout.TextField(value.ToString());
                    if (float.TryParse(newText, out float newValue))
                    {
                        property.SetValue(config, newValue);
                    }
                }
                else if (property.PropertyType == typeof(int))
                {
                    int value = (int)currentValue;
                    string newText = GUILayout.TextField(value.ToString());
                    if (int.TryParse(newText, out int newValue))
                    {
                        property.SetValue(config, newValue);
                    }
                }
                else if (property.PropertyType == typeof(bool))
                {
                    bool value = (bool)currentValue;
                    bool newValue = GUILayout.Toggle(value, "");
                    property.SetValue(config, newValue);
                }
                else if (property.PropertyType == typeof(string))
                {
                    string value = (string)currentValue;
                    string newValue = GUILayout.TextField(value);
                    property.SetValue(config, newValue);
                }
                else if (property.PropertyType.IsEnum)
                {
                    if (property.PropertyType.GetCustomAttribute<FlagsAttribute>() != null)
                    {
                        GUILayout.EndHorizontal();
                        GUILayout.BeginVertical("box");
                        Array enumValues = Enum.GetValues(property.PropertyType);
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

                                property.SetValue(config, Enum.ToObject(property.PropertyType, combinedValue));
                            }
                        }

                        GUILayout.EndVertical();
                        GUILayout.BeginHorizontal();
                    }
                    else
                    {
                        string[] enumNames = Enum.GetNames(property.PropertyType);
                        int currentEnumIndex = Array.IndexOf(enumNames, currentValue.ToString());
                        int newEnumIndex = GUILayout.SelectionGrid(
                            currentEnumIndex, enumNames, enumNames.Length > 3 ? 3 : enumNames.Length
                        );

                        if (newEnumIndex != currentEnumIndex)
                        {
                            object newEnumValue = Enum.Parse(property.PropertyType, enumNames[newEnumIndex]);
                            property.SetValue(config, newEnumValue);
                        }
                    }
                }

                GUILayout.EndHorizontal();
            }
        }
    }
}
