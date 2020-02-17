using KaymakNetwork;
using UnityEngine;

internal class NetworkSend_Server
{ 
    public static void WelcomeMsg(int connectionID, string msg)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ServerPackets.WELCOME_MSG);
        buffer.WriteString(msg);
        NetworkConfig_Server.socket.SendDataTo(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
    }


    private static ByteBuffer PlayerData(int connectionID, Player_Server player)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ServerPackets.Instantiate_Player);
        buffer.WriteInt32(connectionID);

        return buffer;
    }
    
    public static void InitNetworkPlayer(int connectionID, Player_Server player)
    {
        for (int i = 1; i < GameManager_Server.playerList.Count; i++)
        {
            if (GameManager_Server.playerList[i] != null && GameManager_Server.playerList[i].inGame && i != connectionID)
            {
                ByteBuffer buffer = PlayerData(i, player);
                NetworkConfig_Server.socket.SendDataTo(connectionID, buffer.Data, buffer.Head);
                buffer.Dispose();
            }
        }
        
        ByteBuffer buf = PlayerData(connectionID, player);
        NetworkConfig_Server.socket.SendDataToAll(buf.Data, buf.Head);
        buf.Dispose();
    }
}
