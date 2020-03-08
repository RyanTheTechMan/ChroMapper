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
    private float _testNum;

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

        const int offset = -1;
        _testNum = (float) (_environmentLength + offset) / (_environmentLength + _customPlatLength);
    }
    
    private bool _lastSectionBool;
    private Coroutine _moveUpCoroutine;
    
    private void LateUpdate()
    {
        if(_customPlatLength == 0) return;

        bool b = 1 - _scrollbar.value >= _testNum;
        
        if (_lastSectionBool != b)
        {
            _moveUpCoroutine = StartCoroutine(MoveSectionTitle(b));
            _lastSectionBool = b;
        }
    }
    
    private IEnumerator MoveSectionTitle(bool moveUp)
    {
        if(_moveUpCoroutine != null) StopCoroutine(_moveUpCoroutine);
        
        float startTime = Time.time;

        const float randomOffset = 12;
        const float cPos = -3f+randomOffset;
        const float ePos = -45.5f+randomOffset;
        
        while (true)
        {
            var localPosition = catTitle.localPosition;
            float pos = localPosition.y;
            pos = Mathf.Lerp(pos, moveUp ? cPos : ePos, (Time.time / startTime) * 0.2f);
            Vector3 v = localPosition;
            v.y = pos;
            localPosition = v;
            catTitle.localPosition = localPosition;
            if (moveUp && pos >= cPos)
            {
                v.y = cPos;
                localPosition = v;
                catTitle.localPosition = localPosition;
                yield break;
            }
            else if (!moveUp && pos <= ePos)
            {
                v.y = ePos;
                localPosition = v;
                catTitle.localPosition = localPosition;
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
