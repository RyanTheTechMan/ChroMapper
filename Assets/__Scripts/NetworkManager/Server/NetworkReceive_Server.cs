using System;
using System.Collections.Generic;
using System.Linq;
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
        byte[] chunkData = buffer.ReadBytes();

        buffer.Dispose();

        if (!GameManager_Server.ServerIsHost && GameManager_Client.instance.hostValidator != GameManager_Server.playerList[connectionID].hostValidator)
        {
            NetworkManager_Server.Log("{0}, Just tried to send data as a host even though they are not host!", GameManager_Server.playerList[connectionID].username);
            return;
        }
        
        NetworkSend_Server.SendMapDataChunk(clientIdToSendTo, chunkId, totalChunks, networkMapDataType, chunkData);
        
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
