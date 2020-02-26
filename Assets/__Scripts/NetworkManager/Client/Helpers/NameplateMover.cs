using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameplateMover : MonoBehaviour
{
    [SerializeField] private Transform _nameplateTransform;

    private void LateUpdate()
    {
        Transform transform1 = transform;
        Vector3 pos = transform1.position;
        pos.y += 1;
        _nameplateTransform.position = pos;
        _nameplateTransform.LookAt(GameManager.instance.currentCamera.transform);
    }

    private void OnDestroy()
    {
        Destroy(_nameplateTransform.gameObject);
    }
}
