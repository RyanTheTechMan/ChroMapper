using System;
using System.Collections;
using System.IO;
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
        NetworkSend_Client.SendRequestForMapData(NetworkMapData_Type.INFO);
        
        //NetworkSend_Client.SendMapData(NetworkMapData_Type.INFO, BeatSaberSongContainer.Instance.song.directory + "/info.dat");
        //NetworkSend_Client.SendMapData(NetworkMapData_Type.DIFFICULTY, BeatSaberSongContainer.Instance.map.directoryAndFile);
        //NetworkSend_Client.SendMapData(NetworkMapData_Type.SONG, BeatSaberSongContainer.Instance.song.directory + "/" + BeatSaberSongContainer.Instance.song.songFilename);
        //string s = GameManager_Client.instance.TemporaryDirectory.FullName;
        yield return this;
    }
}