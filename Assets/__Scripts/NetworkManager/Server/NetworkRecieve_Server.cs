using KaymakNetwork;
using UnityEngine;

internal static class NetworkRecieve_Server
{
    internal static void PacketRouter()
    {
        NetworkConfig_Server.socket.PacketId[(int) ClientPackets.PING] = Packet_Ping;
        NetworkConfig_Server.socket.PacketId[(int) ClientPackets.UPDATE_LOCATION] = Packet_UpdatePlayerLocation;
        NetworkConfig_Server.socket.PacketId[(int) ClientPackets.UPDATE_ROTATION] = Packet_UpdatePlayerRotation;
    }

    private static void Packet_Ping(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        string msg = buffer.ReadString();
        
        NetworkManager_Server.Log(msg);
        
        GameManager_Server.CreatePlayer(connectionID);
        
        buffer.Dispose();
    }
    
    private static void Packet_UpdatePlayerLocation(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        float x = buffer.ReadSingle();
        float y = buffer.ReadSingle();
        float z = buffer.ReadSingle();

        buffer.Dispose();

        Player_Server ps = GameManager_Server.playerList[connectionID];
        ps.TryToMove(x,y,z);
    }
    
    private static void Packet_UpdatePlayerRotation(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        float x = buffer.ReadSingle();
        float y = buffer.ReadSingle();
        float z = buffer.ReadSingle();
        float w = buffer.ReadSingle();

        buffer.Dispose();

        Player_Server ps = GameManager_Server.playerList[connectionID];
        ps.TryToRotate(x,y,z,w);
    }
}
