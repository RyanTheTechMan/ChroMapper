using KaymakNetwork;
using UnityEngine;

internal static class NetworkReceive_Client
{
    internal static void PacketRouter()
    {
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.WELCOME_MSG] = Packet_WelcomeMsg;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.Instantiate_Player] = Packet_InitNetworkPlayer;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.PLAYER_MOVE] = Packet_PlayerMove;
    }

    private static void Packet_WelcomeMsg(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        string msg = buffer.ReadString();
        buffer.Dispose();

        NetworkManager_Client.instance.connectionID = connectionID;
        
        NetworkSend_Client.SendPing();

    }

    private static void Packet_InitNetworkPlayer(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();

        NetworkManager_Client.instance.InitNetworkPlayer(connectionID, connectionID == NetworkManager_Client.instance.connectionID);

        buffer.Dispose();
    }

    private static void Packet_PlayerMove(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        float x = buffer.ReadSingle();
        float y = buffer.ReadSingle();
        float z = buffer.ReadSingle();
        buffer.Dispose();

        if(!GameManager_Client.instance.playerList.ContainsKey(connectionID)) return;
        
        GameManager_Client.instance.playerList[connectionID].transform.position = new Vector3(x,y,z);
    }
}
