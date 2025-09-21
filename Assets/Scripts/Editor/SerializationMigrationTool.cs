#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace Editor
{
    public sealed class SerializationMigrationTool : OdinEditorWindow
    {
        [Serializable]
        public sealed class Rule
        {
            [LabelText("On")]
            [TableColumnWidth(45, Resizable = false)]
            [ToggleLeft]
            public bool Enabled = true;

            [LabelText("Old Assembly (asmdef)")]
            [TableColumnWidth(160)]
            [Tooltip("Старая сборка (имя asmdef), например Game.UI.Desktop")]
            public string OldAssembly = "Game.UI.Desktop";

            [LabelText("New Assembly (asmdef)")]
            [TableColumnWidth(160)]
            [Tooltip("Новая сборка (имя asmdef), например Game.UI.Shared")]
            public string NewAssembly = "Game.UI.Shared";

            [LabelText("Old Namespace Prefix")]
            [TableColumnWidth(360)]
            [Tooltip("Старый префикс namespace, например AtlantisClient.Game.UI.Desktop")]
            public string OldNsPrefix = "AtlantisClient.Game.UI.Desktop";

            [LabelText("New Namespace Prefix")]
            [TableColumnWidth(360)]
            [Tooltip("Новый префикс namespace, например AtlantisClient.Game.UI.Shared")]
            public string NewNsPrefix = "AtlantisClient.Game.UI.Shared";
        }

        [TitleGroup("Rules", "Правила миграции (добавьте нужные пары)", alignment: TitleAlignments.Left)]
        [TableList(ShowIndexLabels = true, IsReadOnly = false, AlwaysExpanded = true, DrawScrollView = true, NumberOfItemsPerPage = 8)]
        [PropertyOrder(0)]
        public List<Rule> Rules = new List<Rule>()
        {
            new Rule()
            {
                Enabled     = true,
                OldAssembly = "Game.UI.Desktop",
                NewAssembly = "Game.UI.Shared",
                OldNsPrefix = "AtlantisClient.Game.UI.Desktop",
                NewNsPrefix = "AtlantisClient.Game.UI.Shared"
            }
        };

        [PropertyOrder(1)]
        [HorizontalGroup("Rules/Btns")]
        [Button(ButtonSizes.Medium)]
        [GUIColor(0.65f, 0.85f, 1.0f)]
        [LabelText("Добавить правило")]
        private void AddRuleRow()
        {
            Rule r = new Rule();
            Rules.Add(r);
        }

        [PropertyOrder(2)]
        [InfoBox("Перед запуском убедитесь, что на перенесённых классах, попадающих в SerializeReference, добавлен [MovedFrom]. Это позволяет Unity правильно ремапить старые типы в новые ещё на стадии загрузки, а наш инструмент уже фиксирует YAML/строки и сохраняет managedReferenceId.", InfoMessageType.Warning)]

        [TitleGroup("Options", "Опции обработки", alignment: TitleAlignments.Left)]
        [HorizontalGroup("Options/row1")]
        [LabelText(".prefab")]
        public bool IncludePrefabs = true;

        [PropertyOrder(10)]
        [HorizontalGroup("Options/row1")]
        [LabelText(".asset")]
        public bool IncludeAssets = true;

        [PropertyOrder(10)]
        [HorizontalGroup("Options/row1")]
        [LabelText(".unity (только текст-патч)")]
        public bool IncludeScenes = false;

        [PropertyOrder(11)]
        [HorizontalGroup("Options/row2")]
        [LabelText("Dry-Run (только отчёт)")]
        [Tooltip("Ничего не пишет на диск, выводит только лог изменений")]
        public bool DryRun = false;

        [PropertyOrder(11)]
        [HorizontalGroup("Options/row2")]
        [LabelText("Verbose лог")]
        public bool Verbose = true;

        [PropertyOrder(11)]
        [HorizontalGroup("Options/row2")]
        [LabelText("*.bak при текст-патче")]
        public bool MakeBakFiles = false;

        [TitleGroup("Actions", "Действия", alignment: TitleAlignments.Left)]
        [PropertyOrder(20)]
        [HorizontalGroup("Actions/top")]
        [GUIColor(0.8f, 0.95f, 1f)]
        [Button(ButtonSizes.Large)]
        [LabelText("Audit Selection")]
        public void AuditSelection()
        {
            List<string> paths = CollectSelectionPaths();
            RunAudit(paths);
        }

        [PropertyOrder(20)]
        [HorizontalGroup("Actions/top")]
        [GUIColor(0.8f, 0.95f, 1f)]
        [Button(ButtonSizes.Large)]
        [LabelText("Audit Whole Project")]
        public void AuditWholeProject()
        {
            List<string> paths = CollectAllAssetPaths();
            RunAudit(paths);
        }

        [PropertyOrder(21)]
        [HorizontalGroup("Actions/bot")]
        [GUIColor(0.85f, 1f, 0.85f)]
        [Button(ButtonSizes.Large)]
        [LabelText("Fix Selection")]
        public void FixSelection()
        {
            List<string> paths = CollectSelectionPaths();
            RunFix(paths);
        }

        [PropertyOrder(21)]
        [HorizontalGroup("Actions/bot")]
        [GUIColor(1f, 0.8f, 0.8f)]
        [Button(ButtonSizes.Large)]
        [LabelText("Fix Whole Project")]
        public void FixWholeProject()
        {
            List<string> paths = CollectAllAssetPaths();
            RunFix(paths);
        }

        [MenuItem("Tools/Serialization/Migration Tool")]
        public static void ShowWindow()
        {
            SerializationMigrationTool w = GetWindow<SerializationMigrationTool>("Migration");
            w.minSize = new Vector2(900, 560);
        }

        private List<string> CollectSelectionPaths()
        {
            UnityEngine.Object[] selection = Selection.objects ?? Array.Empty<UnityEngine.Object>();
            List<string> seed = selection
                .Select(AssetDatabase.GetAssetPath)
                .Where(p => !string.IsNullOrEmpty(p))
                .ToList();

            string[] exts = CollectEnabledExts();
            List<string> expanded = ExpandToAssetsRecursively(seed, exts);

            List<string> result = expanded
                .Where(IsProcessablePath)
                .Distinct()
                .ToList();

            return result;
        }

        private List<string> CollectAllAssetPaths()
        {
            string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { "Assets" });
            string[] exts = CollectEnabledExts();
            List<string> list = new List<string>(guids.Length);

            for (int i = 0; i < guids.Length; i++)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[i]);

                if (IsExtensionEnabled(Path.GetExtension(path), exts) && IsProcessablePath(path))
                {
                    list.Add(path);
                }
            }

            List<string> distinct = list.Distinct().ToList();
            return distinct;
        }

        private string[] CollectEnabledExts()
        {
            List<string> exts = new List<string>(3);

            if (IncludePrefabs)
            {
                exts.Add(".prefab");
            }

            if (IncludeAssets)
            {
                exts.Add(".asset");
            }

            if (IncludeScenes)
            {
                exts.Add(".unity");
            }

            if (exts.Count == 0)
            {
                return new[] { ".prefab", ".asset" };
            }

            return exts.ToArray();
        }

        private static bool IsExtensionEnabled(string extension, string[] enabled)
        {
            if (string.IsNullOrEmpty(extension))
            {
                return false;
            }

            for (int i = 0; i < enabled.Length; i++)
            {
                if (string.Equals(extension, enabled[i], StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private static List<string> ExpandToAssetsRecursively(List<string> selected, string[] exts)
        {
            List<string> result = new List<string>();

            for (int i = 0; i < selected.Count; i++)
            {
                string path = selected[i];

                if (string.IsNullOrEmpty(path))
                {
                    continue;
                }

                if (AssetDatabase.IsValidFolder(path))
                {
                    string[] guids = AssetDatabase.FindAssets(string.Empty, new[] { path });

                    for (int j = 0; j < guids.Length; j++)
                    {
                        string p = AssetDatabase.GUIDToAssetPath(guids[j]);

                        if (IsExtensionEnabled(Path.GetExtension(p), exts) && IsProcessablePath(p))
                        {
                            result.Add(p);
                        }
                    }
                }
                else
                {
                    if (IsExtensionEnabled(Path.GetExtension(path), exts) && IsProcessablePath(path))
                    {
                        result.Add(path);
                    }
                }
            }

            return result;
        }

        private static bool IsProcessablePath(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                return false;
            }

            if (!path.StartsWith("Assets/", StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        private void RunAudit(List<string> paths)
        {
            if (paths == null || paths.Count == 0)
            {
                Debug.Log("[Migration] Нет ассетов для аудита.");
                return;
            }

            int hits = 0;

            try
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    string p = paths[i];
                    EditorUtility.DisplayProgressBar("Audit", $"{i + 1}/{paths.Count}: {p}", (float)(i + 1) / paths.Count);

                    if (FileHasOldRefs_TextScan(p, Rules))
                    {
                        Debug.Log($"[Audit] {p} — обнаружены старые ссылки");
                        hits++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            Debug.Log($"[Audit] Готово. Файлов с потенциально старыми ссылками: {hits}");
        }

        private void RunFix(List<string> paths)
        {
            if (paths == null || paths.Count == 0)
            {
                Debug.Log("[Migration] Нет ассетов для фикса.");
                return;
            }

            int filesChanged = 0;
            int propsChanged = 0;
            int textPatched = 0;

            try
            {
                for (int i = 0; i < paths.Count; i++)
                {
                    string path = paths[i];
                    EditorUtility.DisplayProgressBar("Fix", $"{i + 1}/{paths.Count}: {path}", (float)(i + 1) / paths.Count);

                    bool changed = false;
                    string ext = Path.GetExtension(path).ToLowerInvariant();

                    if (ext == ".prefab")
                    {
                        if (FixPrefabAssetWithoutInstantiation(path, ref propsChanged))
                        {
                            changed = true;
                        }
                    }
                    else if (ext == ".asset")
                    {
                        if (FixPlainAsset(path, ref propsChanged))
                        {
                            changed = true;
                        }
                    }

                    if (FileHasOldRefs_TextScan(path, Rules))
                    {
                        if (PatchYamlText_ByRules(path, Rules))
                        {
                            textPatched++;
                            changed = true;
                        }
                    }

                    if (changed)
                    {
                        filesChanged++;
                    }
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (!DryRun)
            {
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }

            Debug.Log($"[Migration] Files changed: {filesChanged}, Serialized props fixed: {propsChanged}, Text patches: {textPatched}");
        }

        private bool FixPrefabAssetWithoutInstantiation(string path, ref int propsFixed)
        {
            if (DryRun)
            {
                return false;
            }

            bool changed = false;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null)
            {
                return false;
            }

            Component[] comps = prefab.GetComponentsInChildren<Component>(true);

            for (int i = 0; i < comps.Length; i++)
            {
                Component comp = comps[i];

                if (comp == null)
                {
                    continue;
                }

                try
                {
                    if (FixObjectSerialized(comp, ref propsFixed))
                    {
                        changed = true;
                        EditorUtility.SetDirty(comp);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[FixPrefabAsset] {path} / {comp.GetType().Name}: {e.Message}");
                }
            }

            if (changed)
            {
                EditorUtility.SetDirty(prefab);
            }

            return changed;
        }

        private bool FixPlainAsset(string path, ref int propsFixed)
        {
            if (DryRun)
            {
                return false;
            }

            bool changed = false;
            UnityEngine.Object[] objs = AssetDatabase.LoadAllAssetsAtPath(path);

            for (int i = 0; i < objs.Length; i++)
            {
                UnityEngine.Object obj = objs[i];

                if (obj == null)
                {
                    continue;
                }

                try
                {
                    if (FixObjectSerialized(obj, ref propsFixed))
                    {
                        changed = true;
                        EditorUtility.SetDirty(obj);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"[FixAsset] {path} / {obj.name}: {e.Message}");
                }
            }

            return changed;
        }

        private bool FixObjectSerialized(UnityEngine.Object host, ref int propsFixed)
        {
            bool changed = false;

            SerializedObject so = new SerializedObject(host);
            SerializedProperty it = so.GetIterator();

            if (it.Next(true))
            {
                do
                {
                    if (it.propertyType == SerializedPropertyType.ManagedReference)
                    {
                        if (TryRetargetManagedRefByRules(so, it))
                        {
                            propsChangedIncrement(ref propsFixed);
                            changed = true;
                        }
                    }

                    if (it.propertyType == SerializedPropertyType.String && it.name == "m_TargetAssemblyTypeName")
                    {
                        string oldValue = it.stringValue;

                        if (!string.IsNullOrEmpty(oldValue))
                        {
                            string newer;

                            if (TryRetargetUnityEventString(oldValue, out newer))
                            {
                                if (Verbose)
                                {
                                    Debug.Log($"[EventRetarget] {host.name} | {it.propertyPath}\nOLD: {oldValue}\nNEW: {newer}");
                                }

                                it.stringValue = newer;
                                propsChangedIncrement(ref propsFixed);
                                changed = true;
                            }
                        }
                    }
                }
                while (it.NextVisible(false));
            }

            if (changed && !DryRun)
            {
                so.ApplyModifiedPropertiesWithoutUndo();
            }

            return changed;
        }

        private static void propsChangedIncrement(ref int counter)
        {
            counter = counter + 1;
        }

        private bool TryRetargetUnityEventString(string oldValue, out string newer)
        {
            newer = null;

            int comma = oldValue.LastIndexOf(',');
            string typePart = comma >= 0 ? oldValue.Substring(0, comma).Trim() : oldValue.Trim();
            string asmPart = comma >= 0 ? oldValue.Substring(comma + 1).Trim() : string.Empty;

            for (int i = 0; i < Rules.Count; i++)
            {
                Rule r = Rules[i];

                if (!r.Enabled)
                {
                    continue;
                }

                if (!string.IsNullOrEmpty(asmPart) &&
                    string.Equals(asmPart, r.OldAssembly, StringComparison.Ordinal) &&
                    typePart.StartsWith(r.OldNsPrefix, StringComparison.Ordinal))
                {
                    string newType = r.NewNsPrefix + typePart.Substring(r.OldNsPrefix.Length);
                    newer = string.Concat(newType, ", ", r.NewAssembly);
                    return !string.Equals(newer, oldValue, StringComparison.Ordinal);
                }
            }

            return false;
        }

        private bool TryRetargetManagedRefByRules(SerializedObject hostSO, SerializedProperty p)
        {
            string full = p.managedReferenceFullTypename;

            if (string.IsNullOrEmpty(full))
            {
                return false;
            }

            int sp = full.IndexOf(' ');

            if (sp <= 0)
            {
                return false;
            }

            string asmLeft = full.Substring(0, sp);
            string typeName = full.Substring(sp + 1);

            for (int i = 0; i < Rules.Count; i++)
            {
                Rule r = Rules[i];

                if (!r.Enabled)
                {
                    continue;
                }

                if (!string.Equals(asmLeft, r.OldAssembly, StringComparison.Ordinal))
                {
                    continue;
                }

                if (!typeName.StartsWith(r.OldNsPrefix, StringComparison.Ordinal))
                {
                    continue;
                }

                string tail = typeName.Substring(r.OldNsPrefix.Length);
                string newTypeName = r.NewNsPrefix + tail;

                Type newType = ResolveType(newTypeName, r.NewAssembly);

                if (newType == null)
                {
                    if (Verbose)
                    {
                        Debug.LogWarning(
                            $"[Retarget] NEW TYPE NOT FOUND: '{newTypeName}' (ожидалась сборка '{r.NewAssembly}')\n" +
                            $"Host: {hostSO.targetObject.name} | Property: {p.propertyPath}\nOLD: '{full}'"
                        );
                    }

                    return false;
                }

                try
                {
                    object oldObj = p.managedReferenceValue;
                    long oldId = p.managedReferenceId;

                    object newObj = Activator.CreateInstance(newType);

                    if (oldObj != null)
                    {
                        string json = EditorJsonUtility.ToJson(oldObj, false);
                        EditorJsonUtility.FromJsonOverwrite(json, newObj);
                    }

                    if (!DryRun)
                    {
                        p.managedReferenceValue = newObj;

                        if (oldId > 0 && newObj != null)
                        {
                            ManagedReferenceUtility.SetManagedReferenceIdForObject(hostSO.targetObject, newObj, oldId);
                        }
                    }

                    if (Verbose)
                    {
                        Debug.Log(
                            $"[Retarget] OK: {hostSO.targetObject.name} | {p.propertyPath}\n" +
                            $"OLD: {full}\nNEW: {newType.Assembly.GetName().Name} {newType.FullName}"
                        );
                    }

                    return true;
                }
                catch (Exception e)
                {
                    Debug.LogWarning(
                        $"[Retarget] FAIL '{full}' → '{newTypeName}, {r.NewAssembly}'\n" +
                        $"Host: {hostSO.targetObject.name} | {p.propertyPath}\n{e.Message}"
                    );

                    return false;
                }
            }

            return false;
        }

        private static Type ResolveType(string fullName, string preferredAssembly)
        {
            Type t = Type.GetType(string.Concat(fullName, ", ", preferredAssembly), throwOnError: false);

            if (t != null)
            {
                return t;
            }

            System.Reflection.Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            for (int i = 0; i < assemblies.Length; i++)
            {
                System.Reflection.Assembly asm = assemblies[i];

                try
                {
                    t = asm.GetType(fullName, throwOnError: false);

                    if (t != null)
                    {
                        return t;
                    }
                }
                catch
                {

                }
            }

            return null;
        }

        private static bool FileHasOldRefs_TextScan(string path, List<Rule> rules)
        {
            if (!File.Exists(path))
            {
                return false;
            }

            string txt = File.ReadAllText(path);

            for (int i = 0; i < rules.Count; i++)
            {
                Rule r = rules[i];

                if (!r.Enabled)
                {
                    continue;
                }

                if (txt.Contains(string.Concat(r.OldAssembly, " ", r.OldNsPrefix), StringComparison.Ordinal))
                {
                    return true;
                }

                if (txt.Contains(string.Concat(r.OldNsPrefix, "."), StringComparison.Ordinal) &&
                    txt.Contains(string.Concat(", ", r.OldAssembly), StringComparison.Ordinal))
                {
                    return true;
                }

                if (txt.Contains(string.Concat("ns: ", r.OldNsPrefix), StringComparison.Ordinal) &&
                    txt.Contains(string.Concat("asm: ", r.OldAssembly), StringComparison.Ordinal))
                {
                    return true;
                }
            }

            return false;
        }

        private bool PatchYamlText_ByRules(string path, List<Rule> rules)
        {
            if (DryRun)
            {
                return false;
            }

            string txt = File.ReadAllText(path, Encoding.UTF8);
            string orig = txt;

            for (int i = 0; i < rules.Count; i++)
            {
                Rule r = rules[i];

                if (!r.Enabled)
                {
                    continue;
                }

                txt = Regex.Replace(
                    txt,
                    @"(?<=\bvalue:\s*)" + Regex.Escape(r.OldAssembly) + @"\s+" + Regex.Escape(r.OldNsPrefix) + @"\.",
                    string.Concat(r.NewAssembly, " ", r.NewNsPrefix, "."),
                    RegexOptions.Compiled
                );

                txt = Regex.Replace(
                    txt,
                    @"(m_TargetAssemblyTypeName:\s*)" + Regex.Escape(r.OldNsPrefix) + @"\.([A-Za-z0-9_\.]+),\s*" + Regex.Escape(r.OldAssembly),
                    "$1" + r.NewNsPrefix + @".$2, " + r.NewAssembly,
                    RegexOptions.Compiled
                );

                txt = Regex.Replace(
                    txt,
                    @"type:\s*\{\s*class:\s*[^,]+,\s*ns:\s*" + Regex.Escape(r.OldNsPrefix) + @"([^}]*)asm:\s*" + Regex.Escape(r.OldAssembly) + @"\s*\}",
                    delegate (Match m)
                    {
                        string block = m.Value;

                        block = Regex.Replace(
                            block,
                            @"ns:\s*" + Regex.Escape(r.OldNsPrefix),
                            "ns: " + r.NewNsPrefix
                        );

                        block = Regex.Replace(
                            block,
                            @"asm:\s*" + Regex.Escape(r.OldAssembly),
                            "asm: " + r.NewAssembly
                        );

                        return block;
                    },
                    RegexOptions.Compiled
                );
            }

            if (!string.Equals(txt, orig, StringComparison.Ordinal))
            {
                if (MakeBakFiles)
                {
                    File.WriteAllText(path + ".bak", orig, Encoding.UTF8);
                }

                File.WriteAllText(path, txt, Encoding.UTF8);
                AssetDatabase.ImportAsset(path);
                return true;
            }

            return false;
        }
    }
}
#endif
