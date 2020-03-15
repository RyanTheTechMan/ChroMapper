using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DifficultySelectorHandler : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private RectTransform _selectedGameObject;
    private Coroutine _moveSelectArrowCoroutine;

    private SelectItemFromList _selectItemFromList;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private TextMeshProUGUI placeHolder;
    [SerializeField] private SongInfoEditUI _songInfoEditUi;
    private int myIndex;
    
    void Start()
    {
        _selectItemFromList = GetComponentInParent<SelectItemFromList>();
        _selectItemFromList.OnSelect += ShowOrHide;
        myIndex = transform.GetSiblingIndex();
        
        inputField.onEndEdit.AddListener(s => _songInfoEditUi.OnDifficultySelect(myIndex, s));
    }

    private void ShowOrHide(int index)
    {
        Color color = text.color;
        color.a = index == myIndex ? 1 : .5f;
        text.color = color;
    }

    public float editWaitTime;
    
    public void OnPointerClick(PointerEventData eventData)
    {
        _selectedGameObject.SetParent(transform, true);
        _moveSelectArrowCoroutine = StartCoroutine(MoveSelectArrow());
        _selectItemFromList.SetSelected(transform.GetSiblingIndex());

        if (editWaitTime > 0)
        {
            inputField.Select();
            inputField.interactable = true;
        }
        
        editWaitTime = 50f;
        
        _songInfoEditUi.difficultyList.value = _songInfoEditUi.difficultyList.options.IndexOf(_songInfoEditUi.difficultyList.options.FirstOrDefault(x => x == _songInfoEditUi.songDifficultyData[myIndex].difficulty));
        _songInfoEditUi.difficultyList.name = _songInfoEditUi.songDifficultyData[myIndex].difficulty;
        _songInfoEditUi.selectedDifficultyIndex = myIndex;
        
        _songInfoEditUi.LoadDifficulty();
        
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
