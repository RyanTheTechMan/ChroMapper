using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public abstract class SettingsBinder : Editor
{
    protected readonly Dictionary<string, FieldInfo> AllFieldInfos = GetAllFieldInfos();

    public virtual object ModifyValue(object value) { return value; }

    private static Dictionary<string, FieldInfo> GetAllFieldInfos(){ 
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

    protected static List<string> GenerateList(Dictionary<string, FieldInfo> allFieldInfos, Type type)
    {
        List<string> fieldNames = allFieldInfos.Keys.Where(s => Attribute.IsDefined(allFieldInfos[s], type)).ToList();

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