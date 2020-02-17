using KaymakNetwork;

internal static class NetworkReceive_Client
{
    internal static void PacketRouter()
    {
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.WELCOME_MSG] = Packet_WelcomeMsg;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.Instantiate_Player] = Packet_InitNetworkPlayer;
    }

    private static void Packet_WelcomeMsg(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        string msg = buffer.ReadString();
        buffer.Dispose();
        
        NetworkManager_Client.Log(msg);
        
        NetworkSend_Client.SendPing();
    }

    private static void Packet_InitNetworkPlayer(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        
        NetworkManager_Client.instance.InitNetworkPlayer(connectionID);
        
        buffer.Dispose();
    }
}
