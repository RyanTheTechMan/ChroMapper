using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;


public class BuildTextList : MonoBehaviour
{
    public GameObject prefab;
    public string[] text;
}

[CustomEditor(typeof(BuildTextList))]
public class BuildTextList_Editor : Editor
{
    private BuildTextList btl;
    private bool showHiddenSettings = false;
    
    private void OnEnable()
    {
        btl = (BuildTextList) target;
    }
    
    public override void OnInspectorGUI() //Why is this broken on BUILD
    {
        try
        {
            base.OnInspectorGUI();

            if (GUILayout.Button("Update List"))
            {
                btl.text = btl.GetComponentsInChildren<TextMeshProUGUI>().Select(t => t.text).ToArray();
            }
            EditorGUILayout.HelpBox("This button WILL clear all text above", MessageType.Warning);
            
            EditorGUILayout.Separator();
            if(GUILayout.Button("Save / Update"))
            {
                btl.GetComponentsInChildren<ItemHoverGlow>().Select(g => g.gameObject).ToList().ForEach(DestroyImmediate);
                
                foreach (string s in btl.text)
                {
                    GameObject go = Instantiate(btl.prefab, btl.transform);
                    go.name = s;
                    go.GetComponentInChildren<TextMeshProUGUI>().text = s;
                    EditorUtility.SetDirty(go);
                }
            }
            EditorGUILayout.HelpBox("This button WILL clear all child GameObjects", MessageType.Warning);
        }
        catch (NullReferenceException)
        {
            EditorGUILayout.HelpBox("Error while loading custom editor, showing standard settings.", MessageType.Error);
            base.OnInspectorGUI();
        }
    }
}