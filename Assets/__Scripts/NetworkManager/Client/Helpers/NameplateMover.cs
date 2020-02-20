using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NameplateMover : MonoBehaviour
{
    [SerializeField] private Transform _nameplateTransform;

    private void LateUpdate()//Make this follow where the camera is, so the name is always visable
    {
        Vector3 pos = transform.position;
        pos.y += 1;
        _nameplateTransform.position = pos;
    }
}
