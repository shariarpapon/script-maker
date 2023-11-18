using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace CustomScriptMakerUtility
{
    public class ScriptMakerConfigEditorWindow : TabDisplayerEditorWindow
    {
        public List<string> defaultNamespaces = ScriptMaker.config.nsDefault;

        private const string _MENU_PATH = "Tools/Script Maker/Configurations";
        private const string _WINDOW_TITLE = "Script Creator Config";
        private static ScriptMakerConfigEditorWindow _Instance = null;
        private static GUIStyle _horizontalLine;
        private Vector2 _nsFilterScrollPosition;
        private Vector2 _nsDebugScrollPosition;
        private string _namespaceFilter;
        private SerializedObject serializedProperties;

        protected override void OnWindowInitialize()
        {
            if (ScriptMaker.config == null)
            {
                Debug.LogError("Config object was not initialized.");
            }
            DefineHorizontalLine();
            SetButtonSectionColor("#323130"); 
            SetTabDisplaySectionColor("#3E3D3B");
            SetDefaultTab(NewTab("Settings", DrawSettings));
            NewTab("Script Templates", DrawScriptTemplate);
            NewTab("Config Data", DrawConfigData);
            NewTab("Debugger", DrawDebugger);
            EditorUtility.SetDirty(this);
        }

        [MenuItem(_MENU_PATH)]
        public static void OpenWindow()
        {
            if (_Instance == null)
                _Instance = GetWindow<ScriptMakerConfigEditorWindow>(_WINDOW_TITLE, true);

            _Instance.minSize = new Vector2(500, 500);
            _Instance.Show();
        }

        private void OnDisable()
        {
            ScriptMaker.Refresh();
            ScriptMaker.Save();
        }

        private void DefineHorizontalLine() 
        {
            _horizontalLine = new GUIStyle();
            _horizontalLine.normal.background = EditorGUIUtility.whiteTexture;
            _horizontalLine.margin = new RectOffset(0, 0, 4, 4);
            _horizontalLine.fixedHeight = 1;
        }

        private void DrawScriptTemplate() 
        {
            EditorGUILayout.LabelField("These namespaces will be included on generated scripts.", EditorStyles.miniLabel);
            DrawListField(nameof(defaultNamespaces), ref ScriptMaker.config.nsDefault);
        }

        private void DrawDebugger() 
        {
            EditorGUILayout.BeginVertical();
            GUILayout.Label($"Target Namespaces", EditorStyles.largeLabel, GUILayout.Height(18));
            GUILayout.Label($"Filter Count: {ScriptMaker.config.nsIncludeFilters.Count}", EditorStyles.miniLabel);
            GUILayout.Label($"Root Search Directory: Assets/{ScriptMaker.config.scriptSearchDirectory}", EditorStyles.miniLabel);
            if (GUILayout.Button("Refresh", GUILayout.ExpandWidth(false)))
                ScriptMaker.Refresh();
            EditorGUILayout.EndVertical();
            EditorGUILayout.Space();
            _nsDebugScrollPosition = EditorGUILayout.BeginScrollView(_nsDebugScrollPosition);
            for(int i= 0; i < ScriptMaker.namespaces.Count; i++) 
                GUILayout.Label($"[{i}] {ScriptMaker.namespaces[i]}", EditorStyles.miniLabel);
            EditorGUILayout.EndScrollView();
        }

        private void DrawConfigData() 
        {
            const uint _BUTTON_HEIGHT = 20;
            if (GUILayout.Button("Save Data", GUILayout.Height(_BUTTON_HEIGHT)))
            {
                ScriptMaker.Save();
                Debug.Log("ScriptCreator config file saved.");
            }
            if (GUILayout.Button("Force Load", GUILayout.Height(_BUTTON_HEIGHT)))
            {
                ScriptMaker.ForceLoadConfig();
                Debug.Log("ScriptCreator config file loaded.");
            }
            if (GUILayout.Button("Delete Data", GUILayout.Height(_BUTTON_HEIGHT)))
            {
                ScriptMaker.Delete();
                Debug.Log("ScriptCreator config file deleted.");
            }
        }

        private void DrawSettings()
        {
            if (GUILayout.Button("Generate Menu"))
                ScriptMaker.GenerateMenu();
                
            EditorGUILayout.Space(15);
            GUILayout.Label("Scripts Search Directory", EditorStyles.largeLabel, GUILayout.Height(18));
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Assets/", GUILayout.ExpandWidth(false));
            ScriptMaker.config.scriptSearchDirectory = EditorGUILayout.TextField(ScriptMaker.config.scriptSearchDirectory);
            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrEmpty(ScriptMaker.config.scriptSearchDirectory)) 
            {
                EditorGUILayout.HelpBox("No script directory is provided for namespace detection, " +
                                        "all C# scripts in the project will be searched."
                                        , MessageType.Warning);
            }
            EditorGUILayout.Space();
            const uint _BUTTON_WIDTH = 34;

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label($"Namespace Filters [{ScriptMaker.config.nsIncludeFilters.Count}]", EditorStyles.largeLabel, GUILayout.ExpandWidth(false));
            EditorGUILayout.Space();
            if (ScriptMaker.config.nsIncludeFilters.Count > 0 && GUILayout.Button("Clear All", GUILayout.ExpandWidth(false)))
            {
                ScriptMaker.config.nsIncludeFilters.Clear();
                ScriptMaker.Refresh();
            }
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.Space();
            GUILayout.BeginHorizontal();
            _namespaceFilter = EditorGUILayout.TextField(_namespaceFilter);
            if (GUILayout.Button("Add", GUILayout.Width(_BUTTON_WIDTH)))
            {
                if (!string.IsNullOrEmpty(_namespaceFilter))
                {
                    if (ScriptMaker.config.nsIncludeFilters.Contains(_namespaceFilter))
                        Debug.Log("Namespace filter already exists.");
                    else
                    { 
                        ScriptMaker.config.nsIncludeFilters.Add(_namespaceFilter);
                        ScriptMaker.Refresh();
                    }
                }
            }
            GUILayout.EndHorizontal();
            if (ScriptMaker.config.nsIncludeFilters.Count <= 0) return;
            _nsFilterScrollPosition = EditorGUILayout.BeginScrollView(_nsFilterScrollPosition);
            for (int i = 0; i < ScriptMaker.config.nsIncludeFilters.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                ScriptMaker.config.nsIncludeFilters[i] = EditorGUILayout.TextField(ScriptMaker.config.nsIncludeFilters[i], EditorStyles.miniTextField);
                if (GUILayout.Button("-", GUILayout.Width(_BUTTON_WIDTH)))
                {
                    ScriptMaker.config.nsIncludeFilters.RemoveAt(i);
                    ScriptMaker.Refresh();
                    EditorGUILayout.EndHorizontal();
                    break; 
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.Space(8);
        }

        private void DrawListField<T>(string listPropertyName, ref List<T> targetList)
        {
            if (serializedProperties == null)
                serializedProperties = new SerializedObject(this);
            SerializedProperty list = serializedProperties.FindProperty(listPropertyName);
            EditorGUILayout.PropertyField(list, true);
            serializedProperties.ApplyModifiedProperties();
            targetList = defaultNamespaces as List<T>;
        }

        private static void HorizontalLine(Color color)
        {
            var c = GUI.color;
            GUI.color = color;
            GUILayout.Box(GUIContent.none, _horizontalLine);
            GUI.color = c;
        }
    }
}