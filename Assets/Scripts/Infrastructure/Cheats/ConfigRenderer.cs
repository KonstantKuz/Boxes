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

                GUILayout.EndHorizontal();
            }
        }
    }
}
