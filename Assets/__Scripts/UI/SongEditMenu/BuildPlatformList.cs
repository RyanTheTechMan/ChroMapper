using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildPlatformList : MonoBehaviour
{
    [SerializeField] private GameObject textItemPrefab;
    void Start()
    {
        GameObject customPlats = transform.GetChild(1).gameObject;
        foreach (string plat in CustomPlatformsLoader.Instance.GetAllEnvironmentIds())
        {
            GameObject go = Instantiate(textItemPrefab, customPlats.transform);
            go.GetComponentInChildren<TextMeshProUGUI>().text = plat;
            go.name = plat;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
    }
}
