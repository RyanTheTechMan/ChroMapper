using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using KaymakNetwork;
using UnityEngine;

internal static class NetworkSend_Client
{
    public static void SendPing()
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.PING);
        buffer.WriteString(Application.version); //todo, possibly also send username? Get it from discord?
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
    
    public static void SendBeatmapAction(string beatmapObject, BeatmapActionType actionType, BeatmapObject.Type beatmapObjectType)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ClientPackets.ACTION);
        buffer.WriteString(beatmapObject);
        buffer.WriteInt32((int)actionType);
        buffer.WriteInt32((int)beatmapObjectType);
        NetworkConfig_Client.socket.SendData(buffer.Data, buffer.Head);
        
        buffer.Dispose();
    }

    public static void SendRequestForMapData()
    
    public static void SendMapData(NetworkMapData_Type type, string data)
    { //buffer is in size of bytes. So, 1000 = 1 byte //todo make it so saving is disabled while sending map
        int bufferSize = 500; //todo make it so the user can change this value. In Options, in new category, multiplayer.

        if (type == NetworkMapData_Type.SONG) bufferSize = 1000;
        
        FileStream fs = new FileStream(data, FileMode.Open, FileAccess.Read);
        int noOfChunks = Convert.ToInt32(Math.Ceiling(Convert.ToDouble(fs.Length) / Convert.ToDouble(bufferSize)));

        int totalLength = (int) fs.Length;
        for (int i = 0; i < noOfChunks; i++)
        {
            int currentChunkLength;
            if (totalLength > bufferSize)
            {
                currentChunkLength = bufferSize;
                totalLength -= currentChunkLength;
            }
            else currentChunkLength = totalLength;

            var sendingBuffer = new byte[currentChunkLength];
            fs.Read(sendingBuffer, 0, currentChunkLength);
            
            ByteBuffer buffer = new ByteBuffer(sendingBuffer.Length);
            buffer.WriteInt32((int) ClientPackets.MAP_DATA);
            buffer.WriteInt32((int) type);
            buffer.WriteInt32(noOfChunks);
            buffer.WriteInt32(i + 1);
            buffer.WriteBytes(sendingBuffer);
            NetworkConfig_Client.socket.SendData(buffer.Data, buffer.Head);
        
            buffer.Dispose();
            
        }
    }
}
