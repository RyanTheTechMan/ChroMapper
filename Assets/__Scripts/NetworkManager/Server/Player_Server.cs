using UnityEngine;

public class Player_Server
{
    public int connectionID;
    public bool inGame;
    public Vector3 position;
    public Quaternion rotation;

    public void TryToMove(float x, float y, float z)
    {
        Vector3 loc = new Vector3(x,y,z);
        position = loc;
        
        NetworkSend_Server.SendPlayerMove(connectionID, x,y,z);
    }

    public void TryToRotate(float x, float y, float z)
    {
        Quaternion rot = Quaternion.Euler(x,y,z);
        rotation = rot;

        NetworkSend_Server.SendPlayerRotate(connectionID, x,y,z);
    }
}