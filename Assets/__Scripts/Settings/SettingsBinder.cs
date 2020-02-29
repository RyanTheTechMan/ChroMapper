using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public abstract class SettingsBinder : Editor
{
    protected Dictionary<string, FieldInfo> AllFieldInfos;
    public abstract object ModifyValue(object value);
}

public static class SettingsBinder_Editor
{

    public static Dictionary<string, FieldInfo> AllFieldInfos(){ 
        Dictionary<string, FieldInfo> d = new Dictionary<string, FieldInfo>();
        
        Type type = typeof(Settings);
        MemberInfo[] infos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
        foreach (MemberInfo info in infos)
        {
            if (!(info is FieldInfo field)) continue;
            d.Add(field.Name, field);
        }

        return d;
    }

    public static List<string> GenerateList(Dictionary<string, FieldInfo> allFieldInfos, string type)
    {
        List<string> fieldNames = new List<string>();

        foreach (string s in allFieldInfos.Keys)
        {
            Type fType = allFieldInfos[s].FieldType;
            
            switch (type)
            {
                case "BetterToggle" when fType == typeof(bool):
                    fieldNames.Add(s);
                    break;
                case "BetterSlider" when fType == typeof(float) || fType == typeof(int):
                    fieldNames.Add(s);
                    break;
                case "VolumeSlider" when fType == typeof(float):
                    fieldNames.Add(s);
                    break;
                case "BetterInputField" when fType == typeof(float) || fType == typeof(int) || fType == typeof(string):
                    fieldNames.Add(s);
                    break;
                case "TMP_Dropdown" when fType == typeof(int):
                    fieldNames.Add(s);
                    break;
            }
        }

        if (fieldNames.Count == 0)
        {
            fieldNames.Add("Something Broke");
            fieldNames.Add("Found Nothing");
            fieldNames.Add("Big Sad");
            fieldNames.Add(":(");
            return fieldNames;
        }
        
        fieldNames.Insert(0, "None");
        
        return fieldNames;
    }
}