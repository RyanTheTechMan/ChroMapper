using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using Object = UnityEngine.Object;

[CustomEditor(typeof(BetterInputField)), CanEditMultipleObjects] 
public class InputFieldBuilder : SettingsBinder
{
    private bool showHiddenSettings = false;
    
    private BetterInputField _inputField;
    
    private void OnEnable()
    {
        _inputField = (BetterInputField) target;
    }

    public override void OnInspectorGUI() //Why is this broken on BUILD
    {
        try
        {
            _inputField._description.text = EditorGUILayout.TextField("Description", _inputField._description.text);

            List<string> possibleValues = GenerateList(AllFieldInfos, typeof(BetterInputFieldAttribute)).ToList();
            int valueToChangeVal = possibleValues.IndexOf(_inputField.valueToChange);
            if (valueToChangeVal == -1) valueToChangeVal = 0;
            _inputField.valueToChange = possibleValues[EditorGUILayout.Popup("On Value Change Set", valueToChangeVal, possibleValues.ToArray())];
            
            EditorGUILayout.Separator();
            EditorGUILayout.Separator();
            
            showHiddenSettings = EditorGUILayout.Toggle("Show Hidden Settings", showHiddenSettings);
            if (showHiddenSettings) base.OnInspectorGUI();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(_inputField);
                EditorUtility.SetDirty(_inputField._description);
            }
        }
        catch (NullReferenceException)
        {
            EditorGUILayout.HelpBox("Error while loading custom editor, showing standard settings.", MessageType.Error);
            base.OnInspectorGUI();
        }
    }

    public override object ModifyValue(object value)
    {
        return value;
    }
}