using System;
using UnityEditor;
using UnityEngine;

public class RenameEditor : EditorWindow
{
    private string _initialName;
    private Action<string> _renameCallback;

    private string _currentName;

    public static void Open(string initialName, Action<string> callback)
    {
        var editor = GetWindow<RenameEditor>(true);
        editor.minSize = new Vector2(300, 50);
        editor.maxSize = new Vector2(300, 50);
        editor._initialName = initialName;
        editor._renameCallback = callback;
        editor._currentName = initialName;
        editor.titleContent = new GUIContent($"Rename {initialName}");
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown)
        {
            switch (Event.current.keyCode)
            {
                case KeyCode.Escape:
                    Cancel();
                    break;
                case KeyCode.Return:
                case KeyCode.KeypadEnter:
                    Rename();
                    break;
                default:
                    break;
            }
        }

        GUI.SetNextControlName("NameField");
        _currentName = EditorGUILayout.TextField(GUIContent.none, _currentName);
        GUI.FocusControl("NameField");


        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button(new GUIContent("Reset")))
        {
            _currentName = _initialName;
            GUI.FocusControl(null);
        }
        
        if (GUILayout.Button(new GUIContent("Cancel")))
        {
            Cancel();
        }

        if (GUILayout.Button(new GUIContent("Rename")))
        {
            Rename();
        }

        EditorGUILayout.EndHorizontal();
    }

    private void Rename()
    {
        if (_currentName.Equals(_initialName))
        {
            Close();

            return;
        }

        _renameCallback(_currentName);
        Close();
    }

    private void Cancel()
    {
        Close();
    }
}
