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
        buffer.WriteString("Hi, I am a client sending a PING. If you see this, then you are the server!");
        NetworkConfig_Client.socket.SendData(buffer.Data, buffer.Head);
        
        buffer.Dispose();
    }
    
    public static void SendCurrentLocation(float x, float y, float z)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.UPDATE_LOCATION);
        buffer.WriteSingle(x);
        buffer.WriteSingle(y);
        buffer.WriteSingle(z);
        NetworkConfig_Client.socket.SendData(buffer.Data, buffer.Head);
        
        buffer.Dispose();
    }

    public static void SendCurrentRotation(float x, float y, float z, float w)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.UPDATE_ROTATION);
        buffer.WriteSingle(x);
        buffer.WriteSingle(y);
        buffer.WriteSingle(z);
        buffer.WriteSingle(w);
        NetworkConfig_Client.socket.SendData(buffer.Data, buffer.Head);
        
        buffer.Dispose();
    }
}
