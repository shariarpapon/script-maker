using UnityEditor;
using UnityEngine;
using System.Runtime.InteropServices;
using System.IO;

namespace CustomScriptMakerUtility
{
    public class ScriptNamingEditorWindow : EditorWindow
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct CursorPosition
        {
            public int x;
            public int y;
            public static implicit operator Vector2(CursorPosition position)
                => new Vector2(position.x, position.y);
        }

#if UNITY_EDITOR_WIN
        [DllImport("user32.dll")]
        public static extern bool GetCursorPos(out CursorPosition cursorPosition);
#else
        public static void GetCursorPos(out CursorPosition cursorPosition) 
        {
            cursorPosition = new CursorPosition();
            cursorPosition.x = (int)Input.mousePosition.x;
            cursorPosition.y = (int)Input.mousePosition.y;
        }
#endif

        private const string FOCUS_CONTROL_ID = "NameField";
        private static readonly Vector2 size = new Vector2(200, 40);
        private static ScriptNamingEditorWindow Instance = null;
        private string scritptName = "";
        private System.Action<string> onConfirmCallback;

        public static Vector2 GetCursorPosition()
        {
            GetCursorPos(out CursorPosition cursorPosition);
            return (Vector2)cursorPosition;
        }

        public static void Open(System.Action<string> onConfirmCallback)
        {
            if (Instance == null)
                Instance = GetWindow<ScriptNamingEditorWindow>("Script Name", true);
            else
                return;

            Vector2 mp = GetCursorPosition();
            GUI.backgroundColor = Color.cyan;
            Instance.onConfirmCallback = onConfirmCallback;
            Instance.maxSize = size;
            Instance.minSize = size;
            Instance.position = new Rect(mp.x - size.x/2, mp.y - size.y/2, size.x, size.y);
        }


        private void OnGUI()
        {
            if (GUI.GetNameOfFocusedControl() != FOCUS_CONTROL_ID)
                GUI.FocusControl(FOCUS_CONTROL_ID);
            else if(Event.current != null)
            {
                if (Event.current.isKey && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
                    TryConfirm();
            }

            GUI.SetNextControlName(FOCUS_CONTROL_ID);
            scritptName = GUILayout.TextField(scritptName, GUILayout.ExpandWidth(true));

            if (GUILayout.Button("Create [Enter]")) 
            {
                TryConfirm();
            }
        }

        public void TryConfirm() 
        {
            if (!string.IsNullOrEmpty(scritptName))
            {
                if (!IsValidFileName(scritptName, out char invalidChar))
                {
                    Debug.LogWarning($"File names cannot contain the character `{invalidChar}`");
                    return;
                }
                onConfirmCallback?.Invoke(scritptName);
                Instance.Close();
            }
            else Debug.LogWarning("Script name cannot be null or empty.");
        }

        private void CheckForConfirmation() 
        {
            if (Event.current?.keyCode == KeyCode.Return && Event.current.type == EventType.KeyDown)
                TryConfirm();
        }

        private static bool IsValidFileName(string fileName, out char invalidChar) 
        {
            invalidChar = default;
            char[] invalidChars = Path.GetInvalidFileNameChars();
            for (int i = 0; i < invalidChars.Length; i++)
                if (fileName.Contains(invalidChars[i]))
                {
                    invalidChar = invalidChars[i];
                    return false;
                }
            return true;
        }
    }
}