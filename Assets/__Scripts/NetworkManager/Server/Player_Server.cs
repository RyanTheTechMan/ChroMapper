using UnityEngine;

public class Player_Server
{
    public int connectionID;
    public bool inGame;
    public Vector3 position;

    public void TryToMove(float x, float y, float z)
    {
        Vector3 loc = new Vector3(x,y,z);
        
        position = loc;

        NetworkSend_Server.SendPlayerMove(connectionID, x,y,z);
    }
}