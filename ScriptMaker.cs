using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace CustomScriptMakerUtility
{
    public enum  ScriptType
    {
        MonoBehaviour,
        Class,
        Struct,
        Abstract,
        Interface,
        Enum
    }

    [System.Serializable]
    public sealed class Config
    {
        public string scriptSearchDirectory = "";
        public List<string> nsIncludeFilters = new List<string>();
        public List<string> nsDefault = new List<string>() { "UnityEngine" };
    }

    [InitializeOnLoad]
    public static class ScriptMaker
    {
        private const int SCRIPT_UID = 0;
        private const string CONFIG_FILE_NAME = "scriptmakerconfig.json";
        private const string MENU_PATH_ROOT = "Assets/Create/Scripts";
        public static Config config;
        public static List<string> namespaces;

        private static bool IsMenuGeneratedOnce 
        {
            get { return SessionState.GetBool(nameof(IsMenuGeneratedOnce), false); }
            set  {  SessionState.SetBool(nameof(IsMenuGeneratedOnce), value); }
        }

        private static readonly ScriptType[] scriptTypes = 
        {
            ScriptType.Abstract,
            ScriptType.Interface,
            ScriptType.Struct,
            ScriptType.Class,
            ScriptType.Enum,
            ScriptType.MonoBehaviour,
        };

        static ScriptMaker()
        {
            if (config == null)
            {
                config = Load();
                if(config == null)
                    config = new Config();
            }
            LoadNamespaces();


            if (!File.Exists(MenuItemScriptGenerator.GetScriptPath(SCRIPT_UID)))
                GenerateMenu();
            else if (!IsMenuGeneratedOnce) 
            {
                IsMenuGeneratedOnce = true;
                GenerateMenu();
            }
        }

        public static void GenerateMenu() 
        {
            MenuMethodInfo[] info = new MenuMethodInfo[scriptTypes.Length];
            for (int i = 0; i < scriptTypes.Length; i++) 
            {
                int t = (int)scriptTypes[i];
                info[i] = new MenuMethodInfo(scriptTypes[i].ToString(), $"Menu item for creating {scriptTypes[i]} script.", $"ScriptMaker.OnClickAction({t}, \"#ns-arg#\");");
            }
            MenuItemScriptGenerator.GenerateMenuScript(namespaces.ToArray(), SCRIPT_UID, info);
        }

        private static bool TryGetActiveFolderPath(out string path)
        {
            var _tryGetActiveFolderPath = typeof(ProjectWindowUtil).GetMethod("TryGetActiveFolderPath", BindingFlags.Static | BindingFlags.NonPublic);

            object[] args = new object[] { null };
            bool found = (bool)_tryGetActiveFolderPath.Invoke(null, args);
            path = (string)args[0];

            return found;
        }

        public static void OnClickAction(int scriptTypeId, string namespaceName)
        {
            TryGetActiveFolderPath(out string relativeDir);
            string assetRoot = Path.GetDirectoryName(Application.dataPath);
            ScriptType scriptType = (ScriptType)scriptTypeId;
            string dir = Path.Combine(assetRoot, relativeDir);
            if (string.IsNullOrEmpty(relativeDir) || !Directory.Exists(dir))
            {
                Debug.LogError("Unable to create file at: " + dir);
                return;
            }

            ScriptNamingEditorWindow.Open(
            delegate (string confirmedName)
            {
                ScriptTemplate template;
                switch (scriptType)
                {
                    default:
                        template = new ScriptTemplate(namespaceName, "class", confirmedName, "");
                        break;
                    case ScriptType.Class:
                        template = new ScriptTemplate(namespaceName, "class", confirmedName, "");
                        break;
                    case ScriptType.Struct:
                        template = new ScriptTemplate(namespaceName, "struct", confirmedName, "");
                        break;
                    case ScriptType.Abstract:
                        template = new ScriptTemplate(namespaceName, "class", confirmedName, " abstract");
                        break;
                    case ScriptType.Interface:
                        template = new ScriptTemplate(namespaceName, "interface", confirmedName, "");
                        break;
                    case ScriptType.Enum:
                        template = new ScriptTemplate(namespaceName, "enum", confirmedName, "");
                        break;
                    case ScriptType.MonoBehaviour:
                        template = new ScriptTemplate(namespaceName, "class", confirmedName + " : MonoBehaviour", "");
                        break;
                }

                string scriptContent = template.GenerateScriptContent();
                if (!string.IsNullOrEmpty(scriptContent))
                {
                    string scriptPath = Path.Combine(dir, confirmedName + ".cs");
                    File.WriteAllText(scriptPath, scriptContent);
                    AssetDatabase.Refresh();
                }
            });
        }

        public static void Refresh() 
        {
            LoadNamespaces();
        }

        private static string MenuPath(string itemName)
            => Path.Combine(MENU_PATH_ROOT, itemName);

        public static void Save()
        {
            SerializationUtils.TrySaveToJSON(ConfigFilePath(),config);
        }

        public static void ForceLoadConfig() 
        {
            config = Load();
        }

        private static Config Load()
        {
            SerializationUtils.TryLoadFromJSON(ConfigFilePath(), out Config c);
            return c;
        }

        public static void Delete()
        {
            string path = ConfigFilePath();
            if (File.Exists(path))
                File.Delete(path);
            config = new Config();
        }

        private static string ConfigFilePath()
            => Path.Combine(Path.GetDirectoryName(Application.dataPath), CONFIG_FILE_NAME);

        public static void LoadNamespaces()
        {
            string rootSearchDir = Application.dataPath + $"/{config.scriptSearchDirectory}/";
            if (!Directory.Exists(rootSearchDir)) 
            {
                Debug.LogWarning("Directory not found: " + rootSearchDir);
                namespaces.Clear();
                return;
            }

            string[] scriptPaths = Directory.GetFiles(rootSearchDir, "*.cs", SearchOption.AllDirectories);
            List<string> nsList = new List<string>();
            foreach (string scriptPath in scriptPaths)
            {
                string scriptContent = File.ReadAllText(scriptPath);
                var matches = Regex.Matches(scriptContent, @"namespace\s+([\w\.]+)\s*{");
                foreach (Match match in matches)
                    if (match.Success)
                    {
                        string nsName = match.Groups[1].Value;
                        bool isFilteredIn = IsFilteredIn(nsName);
                        if (!nsList.Contains(nsName) && isFilteredIn)
                            nsList.Add(nsName);
                    }
            }
            namespaces = nsList;
        }

        private static bool IsFilteredIn(string namespaceName)
        {
            if (config.nsIncludeFilters == null || config.nsIncludeFilters.Count <= 0)
                return true;
            for (int i = 0; i < config.nsIncludeFilters.Count; i++)
                if (namespaceName.StartsWith(config.nsIncludeFilters[i]))
                    return true;
            return false;
        }
    }
}