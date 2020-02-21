using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class NetworkManager_Client : MonoBehaviour
{
    public GameObject playerPrefab;

    public int connectionID;
    public static NetworkManager_Client instance;
    
    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        //DontDestroyOnLoad(this);
    }

    internal void SetupNetwork()
    {
        Log("Starting Client");
        NetworkConfig_Client.InitNetwork();
        NetworkConfig_Client.ConnectToServer();
        
    }

    private void OnDisable()
    {
        NetworkConfig_Client.DisconnectFromServer();
    }

    public void InitNetworkPlayer(int connectionID, bool currentPlayer, string username, string avatar)
    {
        GameObject go = Instantiate(playerPrefab, GameManager_Client.instance.transform, true);
        go.name = "Player: " + connectionID + " (" + username + ")";
        TextMeshPro text = go.GetComponentInChildren<TextMeshPro>();

        MeshRenderer mr = go.GetComponent<MeshRenderer>();
        
        if (currentPlayer)
        {
            GameManager_Client.instance.LONScript.enabled = true;
            mr.enabled = false;
            go.name = "Player: YOU";
            text.enabled = false;
        }
        else
        {
            text.text = username;
            StartCoroutine(SetPlayerAvatar(avatar, mr));
        }

        text.transform.SetParent(GameManager_Client.instance.transform, true);
        text.name = "Nameplate: " + connectionID + " (" + username + ")";

        GameManager_Client.instance.playerList[connectionID] = go;
    }

    private IEnumerator SetPlayerAvatar(string link, MeshRenderer mr)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(link);
        yield return www.SendWebRequest();

        if(www.isNetworkError || www.isHttpError) {
            Debug.Log(www.error);
            Debug.Log("Received: " + link);
        }
        else {
            mr.material.mainTexture = ((DownloadHandlerTexture)www.downloadHandler).texture;
        }
    }

    public static void Log(string msg, dynamic parm1 = null, dynamic parm2 = null, dynamic parm3 = null)
    {
        string c0 = "</color>";
        string c1 = "<color=#042a8a>";
        string c2 = "<color=#bd1212>";
        string c3 = "<color=#06996f>";
        string c4 = "<color=#01C837>";
        
        if(parm1 != null && parm2 != null && parm3 != null) Debug.LogFormat(c1 + "CLIENT: " + msg + c0, c0 + c2 + parm1 + c0 + c1, c0 + c3 + parm2 + c0 + c1, c0 + c4 + parm3 + c0 + c1);
        else if(parm1 != null && parm2 != null) Debug.LogFormat(c1 + "CLIENT: " + msg + c0, c0 + c2 + parm1 + c0 + c1, c0 + c3 + parm2 + c0 + c1);
        else if(parm1 != null) Debug.LogFormat(c1 + "CLIENT: " + msg + c0, c0 + c2 + parm1 + c0 + c1);
        else Debug.Log(c1 + "CLIENT: " + msg + c0);
    }
}
