using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendLocationOverNetwork : MonoBehaviour
{
    private Coroutine sendLocationCoroutine;
    
    public void OnEnable()
    {
        sendLocationCoroutine = StartCoroutine(sendLocation());
    }

    private void OnDisable()
    {
        StopCoroutine(sendLocationCoroutine);
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        
    }

    private Vector3 lastPos = Vector3.up;
    
    private IEnumerator sendLocation()
    {
        yield return this;
        while (true)
        {
            yield return new WaitForSeconds(1);
            Vector3 pos = transform.position;
            if (pos != lastPos) //If you want, check to see if player position was minimal.
            {
                NetworkSend_Client.SendCurrentLocation(pos.x,pos.y,pos.z);
                lastPos = pos;
            }
        }
    }
}
