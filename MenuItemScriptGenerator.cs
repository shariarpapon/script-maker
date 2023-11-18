using UnityEngine;
using UnityEditor;
using System.IO;

namespace CustomScriptMakerUtility
{
    public sealed class MenuMethodInfo
    {
        public readonly string menuName;
        public readonly string methodComment;
        public readonly string callMethod;
        public MenuMethodInfo(string menuName, string methodComment, string callMethod) 
        {
            this.menuName = menuName;
            this.methodComment = methodComment;
            this.callMethod = callMethod;
        }
    }

    public static class MenuItemScriptGenerator
    {
        private const string REQUIRED_NAMESPACE = "using Wiz.CustomEditor.ScriptMakerUtility;";
        private const int MENU_PRIO = 1;
        private const string CLASS_TEMP = "#required-ns#\r\nusing UnityEditor;\r\n\r\npublic static class __ScriptMaker_DynamicAssetMenuItems_ID#id#__\r\n{\r\n";
        private const string CLASS_ID_PH = "#id#";
        private const string MENU_PATH_PH = "__menupath__";
        private const string METHOD_NAME_PH = "#method-name#";
        private const string REQUIRED_NAMESPACE_PH = "#required-ns#";
        private const string CALL_METHOD_PH = "#call-method#";
        private const string METHOD_COMMENT_PH = "#comment#";
        private const string ATTR_MENU_PATH_PH = "#path#";
        private const string ATTR_MENU_PRIO_PH = "#prio#";
        private const string METHOD_TEMP = "  [MenuItem(#path#, #prio#)]\r\n  public static void #method-name#()\r\n  {\r\n    #call-method#\r\n    #comment#\n  }";

        public static void GenerateMenuScript(string[] rawNamespaces, uint scriptId, params MenuMethodInfo[] menuMethodInfo)
        {
            string[] formattedNamespaces = FormatNamespaces(rawNamespaces);
            string scriptContent = CLASS_TEMP.Replace(CLASS_ID_PH, scriptId.ToString())
                                             .Replace(REQUIRED_NAMESPACE_PH, REQUIRED_NAMESPACE);
            foreach (MenuMethodInfo m in menuMethodInfo)
            {
                string mpath_template = GenerateMenuPathTemplate(m.menuName);
                for(int i = 0; i < formattedNamespaces.Length; i++)
                {
                    string formattedNs = formattedNamespaces[i];
                    string rawNs = rawNamespaces[i];
                    string methodName = $"MENUITEM_{m.menuName}_{formattedNs}";
                    string menuPath = mpath_template.Replace(MENU_PATH_PH, rawNs);
                    string method = GenerateMethod(methodName, rawNs, m.methodComment, m.callMethod, menuPath);
                    scriptContent += method;
                    scriptContent += "\n\n";
                }
            }
            scriptContent += "}";
            string scriptPath = GetScriptPath(scriptId);
            File.WriteAllText(scriptPath, scriptContent);
            Debug.Log("ScriptMaker menu generated. Refreshing database...");
            AssetDatabase.Refresh();
        }

        private static string[] FormatNamespaces(string[] raw)
        {
            string[] formatted = new string[raw.Length];
            for (int i = 0; i < raw.Length; i++)
                formatted[i] = raw[i].Replace(".", "");
            return formatted;
        }

        private static string GenerateMenuPathTemplate(params string[] menuSequence) 
        {
            string path = "Assets/Create/Scripts/";
            foreach (string m in menuSequence)
                path += $"/{m}";

            path += $"/{MENU_PATH_PH}";
            return $"\"{path}\"";
        }
        
        private static string GenerateMethod(string methodName, string nsUnformatted, string comment, string callMethod, string menuPath) 
        {
            string codeComment = "//"+comment;
            string method = METHOD_TEMP.Replace(METHOD_NAME_PH, methodName)
                                       .Replace(ATTR_MENU_PATH_PH, menuPath)
                                       .Replace(ATTR_MENU_PRIO_PH, $"priority={MENU_PRIO}")
                                       .Replace(METHOD_COMMENT_PH, codeComment)
                                       .Replace(CALL_METHOD_PH, 
                                            callMethod.Replace("#ns-arg#", nsUnformatted));
            return method;
        }

        public static string GetScriptPath(uint id) 
        {
            string scriptName = $"__ScriptMaker_DynamicAssetMenuItems_ID{id}__.cs";
            string dir = Path.Combine(Application.dataPath, "Editor");
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            string path = Path.Combine(dir, scriptName);
            return path;
        }

        //Test
        private static void OnClickAction(int scriptType) 
        {
            ScriptType type = (ScriptType)scriptType;
            string path = Selection.activeContext.name;
            Debug.Log($"<color=cyan>{type} : </color>" + path);
        }
    }
}
