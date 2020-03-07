using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlatformListManager : MonoBehaviour
{
    [SerializeField] private GameObject textItemPrefab;

    [SerializeField] private RectTransform catTitle;
    [SerializeField] private Scrollbar _scrollbar;

    private int _environmentLength;
    private int _customPlatLength;
    
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

        _environmentLength = transform.GetChild(0).transform.childCount + 1;
        _customPlatLength = customPlats.transform.childCount;
    }


    private static readonly Vector3 EPos = new Vector3(0,-49,0);
    private static readonly Vector3 CPos = new Vector3(0,-8,0);

    private bool lastSectionBool;
    private Coroutine _moveUpCoroutine;
    
    private void LateUpdate()
    {
        bool b = 1 - _scrollbar.value >= (float)_environmentLength / (_environmentLength + _customPlatLength);
        
        if (lastSectionBool != b)
        {
            _moveUpCoroutine = StartCoroutine(MoveSectionTitle(b));
            lastSectionBool = b;
        }
    }
    
    private IEnumerator MoveSectionTitle(bool moveUp)
    {
        if(_moveUpCoroutine != null) StopCoroutine(_moveUpCoroutine);
        
        float startTime = Time.time;
        
        while (true)
        {
            catTitle.localPosition = Vector3.Lerp(catTitle.localPosition, moveUp ? CPos : EPos, (Time.time / startTime) * 0.2f);
            if (catTitle.localPosition == CPos)
            {
                catTitle.localPosition = CPos;
                yield break;
            }
            else if (catTitle.localPosition == EPos)
            {
                catTitle.localPosition = EPos;
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
