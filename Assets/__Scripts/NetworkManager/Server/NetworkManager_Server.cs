using UnityEngine;

public class NetworkManager_Server : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        NetworkConfig_Server.InitNetwork();
        NetworkConfig_Server.socket.StartListening(5555,5,1);
        Log("Network Initialized");
    }
    
    public static void Log(string msg, dynamic parm1 = null)
    {
        string c0 = "</color>";
        string c1 = "<color=#09753c>";
        string c2 = "<color=#bd1212>";
        
        if(parm1 != null) Debug.LogFormat(c1 + "SERVER: " + msg + c0, c0 + c2 + parm1 + c0 + c1);
        else Debug.Log(c1 + "SERVER: " + msg + c0);
    }
}