using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLocationOverNetwork : MonoBehaviour
{
    private Vector3 lastPos = Vector3.up;
    private Quaternion lastRot = Quaternion.Euler(0,0,0);
    
    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (pos.normalized != lastPos.normalized) //If you want, check to see if player position was minimal.
        {
            NetworkSend_Client.SendCurrentLocation(pos.x,pos.y,pos.z);
            lastPos = pos;
        }
        
        Quaternion rot = transform.rotation;
        if (rot.normalized != lastRot.normalized) //If you want, check to see if player position was minimal.
        {
            NetworkSend_Client.SendCurrentRotation(rot.x,rot.y,rot.z);
            lastRot = rot;
        }
    }
}
