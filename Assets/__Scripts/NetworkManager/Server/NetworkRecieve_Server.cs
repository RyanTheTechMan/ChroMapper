using KaymakNetwork;

internal static class NetworkRecieve_Server
{
    internal static void PacketRouter()
    {
        NetworkConfig_Server.socket.PacketId[(int) ClientPackets.PING] = Packet_Ping;
    }

    private static void Packet_Ping(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        string msg = buffer.ReadString();
        
        NetworkManager_Server.Log(msg);
        
        GameManager_Server.CreatePlayer(connectionID);
        
        buffer.Dispose();
    }
}
