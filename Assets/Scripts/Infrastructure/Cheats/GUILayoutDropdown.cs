using System;
using UnityEngine;

namespace Infrastructure.Cheats
{
    public class GUILayoutDropdown<T>
    {
        private bool isOpen;
        private int selectedIndex;

        public int SelectedIndex => selectedIndex;
        public T SelectedValue => items[selectedIndex];

        private readonly T[] items;
        private readonly string[] labels;

        public GUILayoutDropdown(T[] items, int defaultIndex = 0, Func<T, string> labelSelector = null)
        {
            if (items == null || items.Length == 0)
                throw new ArgumentException("Dropdown items cannot be null or empty");

            this.items = items;
            this.labels = new string[items.Length];

            for (int i = 0; i < items.Length; i++)
            {
                labels[i] = labelSelector != null ? labelSelector(items[i]) : items[i].ToString();
            }

            selectedIndex = Mathf.Clamp(defaultIndex, 0, items.Length - 1);
        }

        public void OnGUI()
        {
            GUILayout.BeginVertical();

            if (GUILayout.Button(labels[selectedIndex], "box"))
            {
                isOpen = !isOpen;
            }

            if (isOpen)
            {
                for (int i = 0; i < items.Length; i++)
                {
                    if (i == selectedIndex) continue;

                    if (GUILayout.Button(labels[i]))
                    {
                        selectedIndex = i;
                        isOpen = false;
                    }
                }
            }

            GUILayout.EndVertical();
        }
    }
}
