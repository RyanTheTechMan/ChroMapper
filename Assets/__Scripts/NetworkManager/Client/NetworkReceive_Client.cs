using Boo.Lang;
using KaymakNetwork;
using UnityEngine;

internal static class NetworkReceive_Client
{
    internal static void PacketRouter()
    {
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.WELCOME_MSG] = Packet_WelcomeMsg;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.Instantiate_Player] = Packet_InitNetworkPlayer;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.PLAYER_MOVE] = Packet_PlayerMove;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.PLAYER_ROTATE] = Packet_PlayerRotate;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.ACTION] = Packet_Action;
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
    
    private static void Packet_PlayerRotate(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        float x = buffer.ReadSingle();
        float y = buffer.ReadSingle();
        float z = buffer.ReadSingle();
        float w = buffer.ReadSingle();
        buffer.Dispose();

        if(!GameManager_Client.instance.playerList.ContainsKey(connectionID)) return;
        
        GameManager_Client.instance.playerList[connectionID].transform.rotation = new Quaternion(x,y,z, w);
    }
    
    private static void Packet_Action(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        string beatmapObject = buffer.ReadString();
        BeatmapActionType beatmapActionType = (BeatmapActionType) buffer.ReadInt32();
        BeatmapObject.Type beatmapObjectType = (BeatmapObject.Type) buffer.ReadInt32();
        buffer.Dispose();

        NetworkManager_Client.Log("Received {0} and object type {1}", beatmapObject, beatmapObjectType.ToString());
        
        switch (beatmapActionType)
        {
            case BeatmapActionType.NORMAL:
                
                break;
            default:
                break;
        }
        
    }
}
