using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLocationOverNetwork : MonoBehaviour
{
    private Coroutine sendLocationCoroutine;
    
    public void OnEnable()
    {
        //sendLocationCoroutine = StartCoroutine(SendLocation());
    }

    private void OnDisable()
    {
        //StopCoroutine(sendLocationCoroutine);
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        Vector3 pos = transform.position;
        if (pos != lastPos) //If you want, check to see if player position was minimal.
        {
            NetworkSend_Client.SendCurrentLocation(pos.x,pos.y,pos.z);
            lastPos = pos;
        }
    }

    private Vector3 lastPos = Vector3.up;
    
    private IEnumerator SendLocation()
    {
        yield return this;
        int frameMax = 2;
        int frame = 0;
        while (true)
        {
            yield return new WaitForEndOfFrame();
            frame++;
            if(frame <= frameMax) continue;
            frame = 0;
            Vector3 pos = transform.position;
            if (pos != lastPos) //If you want, check to see if player position was minimal.
            {
                NetworkSend_Client.SendCurrentLocation(pos.x,pos.y,pos.z);
                lastPos = pos;
            }
        }
    }
}
