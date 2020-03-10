using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemHoverGlow : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private CanvasGroup hoverBox;
    private Coroutine _hoverCoroutine;
    
    private void Start()
    {
        hoverBox = GetComponentInChildren<CanvasGroup>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hoverCoroutine = StartCoroutine(FadeHoverBox(true));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hoverCoroutine = StartCoroutine(FadeHoverBox(false));
    }

    private IEnumerator FadeHoverBox(bool show)
    {
        if(_hoverCoroutine != null) StopCoroutine(_hoverCoroutine);
        
        float startTime = Time.time;
        
        while (true)
        {
            hoverBox.alpha = Mathf.Lerp(hoverBox.alpha, show ? 1 : 0, (Time.time / startTime) * 0.2f);
            if (hoverBox.alpha >= 0.98)
            {
                hoverBox.alpha = 1;
                yield break;
            }
            else if (hoverBox.alpha <= 0.02)
            {
                hoverBox.alpha = 0;
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}
