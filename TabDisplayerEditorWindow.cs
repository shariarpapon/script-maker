using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Wiz.CustomEditor
{
    public abstract class TabDisplayerEditorWindow : EditorWindow
    {
        private const float BUTTON_SECTION_WIDTH = 180;
        private const float BUTTON_SECTION_TOP_OFFSET = 4;
        protected Color buttonSectionColor = new(147f / 255, 160f / 255, 166f / 255);
        protected Color tabDisplaySectionColor = new(91f / 255, 100f / 255, 110f / 255);

        private Tab ActiveTab = null;
        private List<Tab> allTabs;

        private void OnEnable()
        {
            OnWindowInitialize();
        }

        private void OnGUI()
        {
            GUILayout.BeginHorizontal();

            EditorGUI.DrawRect(new Rect(0, 0, BUTTON_SECTION_WIDTH, position.height), buttonSectionColor);
            DrawButtonSection();

            EditorGUI.DrawRect(new Rect(BUTTON_SECTION_WIDTH, 0, position.width - BUTTON_SECTION_WIDTH, position.height), tabDisplaySectionColor);
            DrawTabDisplaySection();

            GUILayout.EndHorizontal();
        }

        protected virtual void OnWindowInitialize()
        {
            Debug.Log("Tab window initialized : " + GetType().Name);
        }

        protected virtual void DrawButtonSection()
        {
            GUILayout.BeginVertical(GUILayout.Width(BUTTON_SECTION_WIDTH));
            GUILayout.Space(BUTTON_SECTION_TOP_OFFSET);
            if (allTabs != null)
            {
                for (int i = 0; i < allTabs.Count; i++)
                {
                    if (SelectTab(allTabs[i].name))
                    {
                        ActiveTab = allTabs[i];
                    }
                    GUILayout.Space(2);
                }
            }
            GUILayout.EndVertical();
        }

        protected virtual void DrawTabDisplaySection()
        {
            GUILayout.BeginVertical();
            ActiveTab?.drawer.Invoke();
            GUILayout.EndVertical();
        }

        public void SetDefaultTab(int index)
        {
            if (allTabs != null)
            {
                index %= allTabs.Count;
                if (index < 0 || allTabs.Count <= 0)
                {
                    return;
                }
                ActiveTab = allTabs[index];
            }
        }

        public bool SelectTab(string name)
        {
            return GUILayout.Button(name, GUILayout.Width(BUTTON_SECTION_WIDTH - 8));
        }

        public void SetDefaultTab(Tab tab)
        {
            if (tab != null && allTabs != null)
            {
                if (!allTabs.Contains(tab))
                {
                    allTabs.Add(tab);
                }
                ActiveTab = tab;
            }
        }

        public void AddTab(Tab tab)
        {
            if (tab == null) return;

            if (allTabs == null)
            {
                allTabs = new List<Tab>();
            }
            if (allTabs.Contains(tab) == false)
                allTabs.Add(tab);
        }

        public void AddTabs(params Tab[] tabs)
        {
            foreach (Tab tab in tabs)
            {
                AddTab(tab);
            }
        }

        public Tab NewTab(string name, System.Action renderer)
        {
            Tab tab = new Tab(name, renderer);
            AddTab(tab);
            return tab;
        }

        public void ClearTabs()
        {
            allTabs = null;
        }

        public void SetWindowTitle(string title)
        {
            titleContent = new GUIContent(title);
        }

        public void SetTabDisplaySectionColor(Color color)
        {
            tabDisplaySectionColor = color;
        }

        public void SetButtonSectionColor(Color color)
        {
            buttonSectionColor = color;
        }

        public void SetTabDisplaySectionColor(string colorHex)
        {
            tabDisplaySectionColor = HexToColor(colorHex);
        }

        public void SetButtonSectionColor(string colorHex)
        {
            buttonSectionColor = HexToColor(colorHex);
        }

        public void SetContentColor(Color color)
        {
            GUI.contentColor = color;
        }

        public void SetContentColor(string colorHex)
        {
            GUI.contentColor = HexToColor(colorHex);
        }

        public static Color HexToColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color output);
            return output;
        }

        public sealed class Tab
        {
            public readonly string name;
            public readonly System.Action drawer;

            public Tab(string name, System.Action drawer)
            {
                this.name = name;
                this.drawer = drawer;
            }
        }
    }
}