using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectItemFromList : MonoBehaviour
{

    [SerializeField] private GameObject _textPrefab;
    
    public int value
    {
        set
        {
            OnSelect?.Invoke(value);
            _value = value;
            name = transform.GetChild(value).name;
        }
        get => _value;
    }

    public List<string> options
    {
        get
        {
            List<string> ss = new List<string>();
            for (int i = 0; i < transform.childCount; i++)
            {
                ss.Add(transform.GetChild(i).name);
            }
            return ss;
        }
    }

    private int _value;
    public string name;

    public Action<int> OnSelect;

    public void SetSelected(int index)
    {
        value = index;
    }

    public void ClearOptions()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i));
        }
    }

    public void AddOptions(List<string> options)
    {
        foreach (string o in options)
        { 
            GameObject go = Instantiate(_textPrefab, transform);
            go.GetComponentInChildren<TextMeshProUGUI>().text = o;
        }
       
        
    }
}