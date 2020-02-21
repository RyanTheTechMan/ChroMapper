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
        buffer.WriteString(Application.version);
        if (GameManager_Client.instance.discordUsername == "")
        {
            buffer.WriteString("");
            buffer.WriteString("");
        }
        else
        {
            buffer.WriteString(GameManager_Client.instance.discordUsername);
            buffer.WriteString(GameManager_Client.instance.discordAvatar);
        }
        
        buffer.WriteString(GameManager_Client.instance.hostValidator);

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
    public static void SendMapData(NetworkMapData_Type type, int clientIDToSendTo, string fileLocation)
    { //buffer is in size of bytes. So, 1000 = 1 byte //todo make it so saving is disabled while sending map
        int bufferSize = 500; //todo make it so the user can change this value. In Options, in new category, multiplayer.

        if (type == NetworkMapData_Type.SONG) bufferSize = 1000; //may be changed
        
        FileStream fs = new FileStream(fileLocation, FileMode.Open, FileAccess.Read);
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
            buffer.WriteInt32(noOfChunks); //Total number of chunks
            buffer.WriteInt32(i + 1); //What chunk we are on
            buffer.WriteInt32(clientIDToSendTo); //The ID to send the data to
            buffer.WriteBytes(sendingBuffer); //Chunk Data
            NetworkConfig_Client.socket.SendData(buffer.Data, buffer.Head);
        
            buffer.Dispose();
        }
    }

    public static void SendRequestForMapData(NetworkMapData_Type type)
    {
        
        NetworkManager_Client.Log("Requesting for Map Data Type: {0}", type.ToString());
        
        GameManager_Client.instance.mapDataRequest = type;
        
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int) ClientPackets.MAP_DATA_REQUEST);
        buffer.WriteInt32((int) type);
        NetworkConfig_Client.socket.SendData(buffer.Data, buffer.Head);
        
        buffer.Dispose();
    }
}
