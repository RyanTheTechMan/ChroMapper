using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using KaymakNetwork;
using UnityEngine;

internal static class NetworkReceive_Server
{
    internal static void PacketRouter()
    {
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.PING] = Packet_Ping;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.UPDATE_LOCATION] = Packet_UpdatePlayerLocation;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.UPDATE_ROTATION] = Packet_UpdatePlayerRotation;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.ACTION] = Packet_Action;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.MAP_DATA] = Packet_MapData;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.MAP_DATA_REQUEST] = Packet_MapDataRequest;
        //NetworkConfig_Server.Socket.TrafficReceived += MedioMapper;
    }

    private static void MedioMapper(int size, ref byte[] data)
    {
        string received = Encoding.UTF8.GetString(data);
        string bts = "";
        foreach (byte b in data)
        {
            bts += b + ", ";
        }
        Debug.Log("Received Bytes: " + bts);
        Debug.Log("Received: " + received);
        if (received.ToLower().StartsWith("chromapper"))
        {
            NetworkManager_Server.Log("User Connected With MedioMapper. Do something cool here.");

            
            ByteBuffer buffer = new ByteBuffer();
            buffer.WriteBytes(Encoding.UTF8.GetBytes("Epic"+"::"+2*100+ 1 +";;;"));
            buffer.WriteBytes(Encoding.UTF8.GetBytes("test"+";;;"));
            buffer.WriteBytes(Encoding.UTF8.GetBytes("aaaaa" + ";;;"));
            buffer.WriteBytes(Encoding.UTF8.GetBytes("Why" + ";;;"));
            buffer.WriteBytes(Encoding.UTF8.GetBytes("no point"+";;;"+2499+";;;"));
            buffer.WriteBytes(Encoding.UTF8.GetBytes( "https://files.catbox.moe/pf035x.ogg" + ";;;"));
            NetworkConfig_Server.Socket.SendDataTo(0, buffer.Data, buffer.Head);
            buffer.Dispose();
        }
    }

    private static void Packet_Ping(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        string version = buffer.ReadString();
        string username = buffer.ReadString();
        string avatar = buffer.ReadString();
        string hostValidator = buffer.ReadString();
        
        buffer.Dispose();

        if (username == "") username = "Editor " + connectionID;

        NetworkManager_Server.Log("User {0} (id {1}) connected with v{2}", username, connectionID, version);

        if (version != "0.6.0")
        {
            Player_Server ps = GameManager_Server.playerList[connectionID];
            NetworkManager_Server.Log("Kicked {0} (id {1}) because on v{2}", username, connectionID, version);
            ps.Kick("Incorrect Version: " + version + "\nServer Version: " + "0.6.0");
        }
        
        NetworkManager_Server.Log("Avatar: {0}", avatar);
        
        GameManager_Server.CreatePlayer(connectionID, username, avatar, hostValidator);
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
    
    private static void Packet_Action(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        string beatmapObject = buffer.ReadString();
        int beatmapActionType = buffer.ReadInt32();
        int beatmapObjectType = buffer.ReadInt32();

        buffer.Dispose();

        NetworkSend_Server.SendAction(connectionID, beatmapObject, beatmapActionType, beatmapObjectType);
    }

    private static void Packet_MapData(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        NetworkMapData_Type networkMapDataType = (NetworkMapData_Type) buffer.ReadInt32();
        int totalChunks = buffer.ReadInt32(); //Add way to see which client to send to.
        int chunkId = buffer.ReadInt32();
        int clientIdToSendTo = buffer.ReadInt32();
        string fileName = buffer.ReadString();
        byte[] chunkData = buffer.ReadBytes();
        short extra1 = buffer.ReadInt16();
        short extra2 = buffer.ReadInt16();

        buffer.Dispose();

        if (!GameManager_Server.ServerIsHost && GameManager_Client.instance.hostValidator != GameManager_Server.playerList[connectionID].hostValidator)
        {
            NetworkManager_Server.Log("{0}, Just tried to send data as a host even though they are not host!", GameManager_Server.playerList[connectionID].username);
            return;
        }
        
        NetworkSend_Server.SendMapDataChunk(clientIdToSendTo, chunkId, totalChunks, networkMapDataType, fileName, chunkData, extra1, extra2);
        
        //string byteChunk = chunkData.Aggregate("", (current, b) => current + (b + ", "));
        //NetworkManager_Server.Log("Receiving Map Data For {0} Chunk {1} Data: {2}", networkMapDataType.ToString(), chunkId + "/" + totalChunks, byteChunk);
    }

    private static void Packet_MapDataRequest(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        NetworkMapData_Type networkMapDataType = (NetworkMapData_Type) buffer.ReadInt32();
        buffer.Dispose();
        
        if (GameManager_Server.ServerIsHost)
        {
            NetworkManager_Server.Log("Server is host? This is not possible at the moment.");
            return;
        }
        
        NetworkSend_Server.SendMapDataRequestToHost(connectionID, networkMapDataType);
        
        //GameManager_Server.host.connectionID;
    }
}
