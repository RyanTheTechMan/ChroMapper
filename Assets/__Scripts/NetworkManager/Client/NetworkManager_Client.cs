using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NetworkManager_Client : MonoBehaviour
{
    public GameObject playerPrefab;
    
    public static NetworkManager_Client instance;
    
    private void Awake()
    {
        instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        //DontDestroyOnLoad(this); this is prob not needed!
        Log("Starting Client");
        NetworkConfig_Client.InitNetwork();
        NetworkConfig_Client.ConnectToServer();
    }

    private void OnDisable()
    {
        NetworkConfig_Client.DisconnectFromServer();
    }

    public void InitNetworkPlayer(int connectionID)
    {
        GameObject go = Instantiate(playerPrefab);
        go.name = "Player: " + connectionID;
        
        GameManager_Client.instance.playerList.Add(connectionID, go);
    }
    
    public static void Log(dynamic msg)
    {
        Debug.Log("<color=#042a8a>CLIENT: " + msg + "</color>");
    }
}
