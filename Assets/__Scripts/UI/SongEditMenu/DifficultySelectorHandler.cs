using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DifficultySelectorHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform _selectedGameObject;
    private Coroutine _moveSelectArrowCoroutine;
    
    void Start()
    {
        
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        _selectedGameObject.SetParent(transform, true);
        _moveSelectArrowCoroutine = StartCoroutine(MoveSelectArrow());
    }
    
    private IEnumerator MoveSelectArrow()
    {
        if(_moveSelectArrowCoroutine != null) StopCoroutine(_moveSelectArrowCoroutine);
        
        float startTime = Time.time;

        Vector3 pos = _selectedGameObject.localPosition;
        
        while (true)
        {
            pos.y = Mathf.Lerp(pos.y, 1f, (Time.time / startTime) * 0.2f);
            if (Mathf.Abs(pos.y) <= 1.001f)
            {
                pos.y = 1f;
                _selectedGameObject.localPosition = pos;
                yield break;
            }
            _selectedGameObject.localPosition = pos;
            yield return new WaitForFixedUpdate();
        }
    }
}
