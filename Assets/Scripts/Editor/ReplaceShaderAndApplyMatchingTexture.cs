using System.IO;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class ReplaceShaderAndAssignMatchingTexture : EditorWindow
    {
        private Shader newShader;
        private string folderPath = "Assets/MyMaterials";
        private string textureProperty = "_MainTex";

        [MenuItem("Tools/Replace Shader & Match Textures")]
        static void ShowWindow()
        {
            GetWindow<ReplaceShaderAndAssignMatchingTexture>("Replace Shader & Match Textures");
        }

        private void OnGUI()
        {
            GUILayout.Label("Shader & Texture Replacement", EditorStyles.boldLabel);

            newShader = (Shader)EditorGUILayout.ObjectField("New Shader", newShader, typeof(Shader), false);
            textureProperty = EditorGUILayout.TextField("Texture Property", textureProperty);

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Folder (relative to Assets/)");
            folderPath = EditorGUILayout.TextField(folderPath);

            GUILayout.Space(20);

            if (GUILayout.Button("Process Materials"))
            {
                ProcessMaterials();
            }
        }

        private void ProcessMaterials()
        {
            string[] materialGuids = AssetDatabase.FindAssets("t:Material", new[] { folderPath });
            int processed = 0;

            foreach (string guid in materialGuids)
            {
                string materialPath = AssetDatabase.GUIDToAssetPath(guid);
                Material mat = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                if (mat == null) continue;

                string materialName = Path.GetFileNameWithoutExtension(materialPath);
                string materialDir = Path.GetDirectoryName(materialPath);

                // ищем «хвост» имени после Mat_
                string suffix = materialName.StartsWith("Mat_") ? materialName.Substring(4) : materialName;

                // ищем текстуру Tex_<suffix> в этой же папке
                string textureName = $"Tex_{suffix}";
                string texturePath = null;

                string[] textureExtensions = { ".png", ".jpg", ".tga", ".psd", ".jpeg" };
                foreach (string ext in textureExtensions)
                {
                    string candidatePath = Path.Combine(materialDir, textureName + ext).Replace("\\", "/");
                    if (File.Exists(Path.Combine(Application.dataPath.Replace("Assets", ""), candidatePath)))
                    {
                        texturePath = candidatePath;
                        break;
                    }
                }

                Texture2D texture = null;
                if (!string.IsNullOrEmpty(texturePath))
                {
                    texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                }

                Undo.RecordObject(mat, "Replace Shader and Assign Texture");

                if (newShader != null)
                {
                    mat.shader = newShader;
                }

                if (texture != null && mat.HasProperty(textureProperty))
                {
                    mat.SetTexture(textureProperty, texture);
                    Debug.Log($"✅ {materialName} → {textureName} assigned.");
                }
                else
                {
                    Debug.LogWarning($"⚠️ No matching texture found for material: {materialName}");
                }

                EditorUtility.SetDirty(mat);
                processed++;
            }

            AssetDatabase.SaveAssets();
            Debug.Log($"🎯 Processed {processed} materials in folder: {folderPath}");
        }
    }
}
