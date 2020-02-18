using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;

public class NetworkManager_Client : MonoBehaviour
{
    public GameObject playerPrefab;

    public int connectionID;
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

    public void InitNetworkPlayer(int connectionID, bool currentPlayer)
    {
        GameObject go = Instantiate(playerPrefab);
        go.name = "Player: " + connectionID;

        if (currentPlayer)
        {
            GameManager_Client.instance.LONScript.enabled = true;
            go.GetComponent<MeshRenderer>().enabled = false;
            go.name = "Player: YOU";
        }
        
        GameManager_Client.instance.playerList.Add(connectionID, go);
    }
    
    public static void Log(string msg, dynamic parm1 = null, dynamic parm2 = null)
    {
        string c0 = "</color>";
        string c1 = "<color=#042a8a>";
        string c2 = "<color=#bd1212>";
        string c3 = "<color=#06996f>";
        
        if(parm1 != null && parm2 != null) Debug.LogFormat(c1 + "CLIENT: " + msg + c0, c0 + c2 + parm1 + c0 + c1, c0 + c3 + parm2 + c0 + c1);
        else if(parm1 != null) Debug.LogFormat(c1 + "CLIENT: " + msg + c0, c0 + c2 + parm1 + c0 + c1);
        else Debug.Log(c1 + "CLIENT: " + msg + c0);
    }
}
