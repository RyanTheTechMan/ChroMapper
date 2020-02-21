using System;
using System.Collections;
using System.IO;
using System.Linq;
using Discord;
using UnityEngine;

public class SendMapData : MonoBehaviour
{
    private void OnEnable()
    {
    
        StartCoroutine(RequestForMapData());
    }

    private IEnumerator RequestForMapData()
    {
        while (GameManager_Client.instance.mapDataRequest != NetworkMapData_Type.NONE) {yield return new WaitForEndOfFrame();}
        NetworkSend_Client.SendRequestForMapData(NetworkMapData_Type.INFO);
        
        while (GameManager_Client.instance.mapDataRequest != NetworkMapData_Type.NONE) {yield return new WaitForEndOfFrame();}
        NetworkSend_Client.SendRequestForMapData(NetworkMapData_Type.DIFFICULTY);
        
        while (GameManager_Client.instance.mapDataRequest != NetworkMapData_Type.NONE) {yield return new WaitForEndOfFrame();}
        NetworkSend_Client.SendRequestForMapData(NetworkMapData_Type.SONG);
        
        while (GameManager_Client.instance.mapDataRequest != NetworkMapData_Type.NONE) {yield return new WaitForEndOfFrame();}
        

        NetworkManager_Client.Log("Received all Map Data! {0}", 
            (GameManager_Client.TemporaryDirectory.GetFiles("*", 
                SearchOption.AllDirectories).Sum(fi => fi.Length) / 1000.0 / 1000.0).ToString("F2") + " Mega Bytes");
        
        yield return this;
    }
}