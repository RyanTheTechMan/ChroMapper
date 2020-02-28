using System;
using System.Net.Sockets;
using UnityEngine;

public class NetworkManager_Server : MonoBehaviour
{
    void OnEnable()
    {
        DontDestroyOnLoad(this);
        NetworkConfig_Server.InitNetwork();
        try
        {
            NetworkConfig_Server.Socket.StartListening(5555,5,1);
        }
        catch (SocketException e)
        {
            Console.WriteLine(e);
            Log("Is the server already running?");
            return;
        }
        Log("Network Initialized");
    }

    private void OnDestroy()
    {   //todo kick all clients first
        NetworkConfig_Server.Socket.StopListening();
        NetworkConfig_Server.Socket.Dispose();
    }

    public static void Log(string msg, dynamic parm1 = null, dynamic parm2 = null, dynamic parm3 = null)
    {
        string c0 = "</color>";
        string c1 = "<color=#09753c>";
        string c2 = "<color=#bd1212>";
        string c3 = "<color=#06996f>";
        string c4 = "<color=#01C837>";
        
        if(parm1 != null && parm2 != null && parm3 != null) Debug.LogFormat(c1 + "SERVER: " + msg + c0, c0 + c2 + parm1 + c0 + c1, c0 + c3 + parm2 + c0 + c1, c0 + c4 + parm3 + c0 + c1);
        else if(parm1 != null && parm2 != null) Debug.LogFormat(c1 + "SERVER: " + msg + c0, c0 + c2 + parm1 + c0 + c1, c0 + c3 + parm2 + c0 + c1);
        else if(parm1 != null) Debug.LogFormat(c1 + "SERVER: " + msg + c0, c0 + c2 + parm1 + c0 + c1);
        else Debug.Log(c1 + "SERVER: " + msg + c0);
    }
}