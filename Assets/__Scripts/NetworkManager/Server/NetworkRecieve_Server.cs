using System;
using System.Collections.Generic;
using System.Linq;
using KaymakNetwork;
using UnityEngine;

internal static class NetworkRecieve_Server
{
    internal static void PacketRouter()
    {
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.PING] = Packet_Ping;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.UPDATE_LOCATION] = Packet_UpdatePlayerLocation;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.UPDATE_ROTATION] = Packet_UpdatePlayerRotation;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.ACTION] = Packet_Action;
        NetworkConfig_Server.Socket.PacketId[(int) ClientPackets.MAP_DATA] = Packet_MapData;
    }

    private static void Packet_Ping(int connectionID, ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        string version = buffer.ReadString();
        string username = buffer.ReadString();
        string avatar = buffer.ReadString();
        
        buffer.Dispose();

        if (username == "") username = "Editor " + connectionID;

        NetworkManager_Server.Log("User {0} (id {1}) connected with v{2}", username, connectionID, version);

        if (version != "0.6.0")
        {
            Player_Server ps = GameManager_Server.playerList[connectionID];
            ps.Kick("Incorrect Version: " + version + "\nServer Version: " + "0.6.0");
        }
        
        NetworkManager_Server.Log("Avatar: {0}", avatar);
        
        GameManager_Server.CreatePlayer(connectionID, username, avatar);
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
        int totalChunks = buffer.ReadInt32();
        int chunkId = buffer.ReadInt32();
        byte[] chunkData = buffer.ReadBytes();

        buffer.Dispose();

        
        
        switch (networkMapDataType)
        {
            case NetworkMapData_Type.INFO:
                //if (chunkId == 1) GameManager_Server.MapData_Info = totalChunks <= 1000 ? new byte[totalChunks][] : new byte[1000][];

                //if(chunkId >= 1000)//Check chunks, then send to clients. then get next 1000
                //GameManager_Server.MapData_Info[chunkId] = chunkData;
                break;
            case NetworkMapData_Type.DIFFICULTY:
                break;
            case NetworkMapData_Type.SONG:
                if (chunkId == 1) GameManager_Server.MapData_Info = totalChunks <= 1000 ? new byte[totalChunks][] : new byte[1000][];
                if (chunkId % 1000 == 1)
                {
                    NetworkSend_Server.SendMapDataChunk(connectionID, networkMapDataType, GameManager_Server.MapData_Song);
                }
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        string byteChunk = chunkData.Aggregate("", (current, b) => current + (b + ", "));
        NetworkManager_Server.Log("Receiving Map Data For {0} Chunk {1} Data: {2}", networkMapDataType.ToString(), chunkId + "/" + totalChunks, byteChunk);

        //NetworkSend_Server.SendAction(connectionID, beatmapObject, beatmapActionType, beatmapObjectType);
    }
}
