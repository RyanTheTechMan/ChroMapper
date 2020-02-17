using System.Collections;
using System.Collections.Generic;
using KaymakNetwork;
using UnityEngine;

internal static class NetworkSend_Client
{
    public static void SendPing()
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.PING);
        buffer.WriteString("Hi, I am a client.");
        NetworkConfig_Client.socket.SendData(buffer.Data, buffer.Head);
        
        buffer.Dispose();
    }
}
