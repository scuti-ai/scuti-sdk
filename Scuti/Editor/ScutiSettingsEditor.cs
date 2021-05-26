using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(ScutiSettings))]
public class ScutiSettingsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        ScutiSettings settings = base.target as ScutiSettings;
        if (string.IsNullOrEmpty(settings.developerKey))
        {
            EditorGUILayout.LabelField("You need to fill in your developer key.");
        } 
        if(GUILayout.Button("Dashboard login"))
        {
            Application.OpenURL("https://frontend.scuti.dev/");
        }
    }
}
