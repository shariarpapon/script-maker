using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;

namespace ScriptMakerUtility
{
    [System.Serializable]
    public sealed class Config
    {
        public string scriptDirectory = "";
        public List<string> nsIncludeFilter = new List<string>();
    }

    [InitializeOnLoad]
    public static class ScriptMaker
    {
        private const string CONFIG_FILE_NAME = "scriptcreator_config.json";
        private const string MENU_PATH_ROOT = "Assets/Create/Scripts";
        private const int MENU_PRIORITY = 1;
        public static Config config;
        public static List<string> namespaces;

        static ScriptMaker()
        {
            if (config == null)
            {
                config = Load();
                if(config == null)
                    config = new Config();
            }
            LoadNamespaces();
        }

        [MenuItem(MENU_PATH_ROOT, priority = MENU_PRIORITY)]
        public static void TestScript()
        {
        }

        public static void Refresh() 
        {
            LoadNamespaces();
        }

        private static string MenuPath(string itemName)
            => Path.Combine(MENU_PATH_ROOT, itemName);

        public static void Save()
        {
            string json = JsonUtility.ToJson(config);
            if (string.IsNullOrEmpty(json))
                return;
            File.WriteAllText(ConfigFilePath(), json);
        }

        public static void ForceLoadConfig() 
        {
            config = Load();
        }

        private static Config Load()
        {
            string path = ConfigFilePath();
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);
            if (string.IsNullOrEmpty(json))
                return null;

            return JsonUtility.FromJson<Config>(json);
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
            string rootSearchDir = Application.dataPath + $"/{config.scriptDirectory}/";
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
            if (config.nsIncludeFilter == null || config.nsIncludeFilter.Count <= 0)
                return true;
            for (int i = 0; i < config.nsIncludeFilter.Count; i++)
                if (namespaceName.StartsWith(config.nsIncludeFilter[i]))
                    return true;
            return false;
        }
    }
}