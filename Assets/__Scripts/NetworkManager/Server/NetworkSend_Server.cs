﻿using System;
using System.Collections.Generic;
using KaymakNetwork;
using UnityEngine;

internal class NetworkSend_Server
{ 
    public static void WelcomeMsg(int connectionID, string msg)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ServerPackets.WELCOME_MSG);
        buffer.WriteInt32(connectionID);
        buffer.WriteString(msg);
        NetworkConfig_Server.Socket.SendDataTo(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
    }


    private static ByteBuffer PlayerData(int connectionID, Player_Server player)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ServerPackets.Instantiate_Player);
        buffer.WriteInt32(connectionID);
        buffer.WriteString(player.username);
        buffer.WriteString(player.img);

        return buffer;
    }
    
    public static void InitNetworkPlayer(int connectionID, Player_Server player)
    {
        for (int i = 1; i < GameManager_Server.playerList.Count; i++)
        {
            if (GameManager_Server.playerList[i] != null && GameManager_Server.playerList[i].inGame && i != connectionID)
            {
                ByteBuffer buffer = PlayerData(i, player);
                NetworkConfig_Server.Socket.SendDataTo(connectionID, buffer.Data, buffer.Head);
                buffer.Dispose();
            }
        }
        
        ByteBuffer buf = PlayerData(connectionID, player);
        NetworkConfig_Server.Socket.SendDataToAll(buf.Data, buf.Head);
        buf.Dispose();
    }
    
    public static void SendPlayerMove(int connectionID, float x, float y, float z)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ServerPackets.PLAYER_MOVE);
        buffer.WriteInt32(connectionID);
        buffer.WriteSingle(x);
        buffer.WriteSingle(y);
        buffer.WriteSingle(z);
        NetworkConfig_Server.Socket.SendDataToAllBut(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
    }
    
    [Obsolete("Use 'x y z' instead.", false)]
    public static void SendPlayerMove(int connectionID, Vector3 loc)
    {
        SendPlayerMove(connectionID, loc.x, loc.y, loc.z);
    }

    public static void SendPlayerRotate(int connectionID, float x, float y, float z, float w)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ServerPackets.PLAYER_ROTATE);
        buffer.WriteInt32(connectionID);
        buffer.WriteSingle(x);
        buffer.WriteSingle(y);
        buffer.WriteSingle(z);
        buffer.WriteSingle(w);
        NetworkConfig_Server.Socket.SendDataToAllBut(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
    }

    public static void SendAction(int connectionID, string beatmapObject, int beatmapActionType, int beatmapObjectType)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ServerPackets.ACTION);
        buffer.WriteInt32(connectionID);
        buffer.WriteString(beatmapObject);
        buffer.WriteInt32(beatmapActionType);
        buffer.WriteInt32(beatmapObjectType);
        NetworkConfig_Server.Socket.SendDataToAllBut(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
    }

    public static void SendKickUser(int connectionID, string reason)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int)ServerPackets.USER_KICK);
        buffer.WriteInt32(connectionID);
        buffer.WriteString(reason);
        NetworkConfig_Server.Socket.SendDataTo(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
    }

    public static void SendMapDataRequestToHost(int connectionID, NetworkMapData_Type networkMapDataType)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int) ServerPackets.MAP_DATA_REQUEST_TO_HOST);
        buffer.WriteInt32((int) networkMapDataType);
        buffer.WriteInt32(connectionID);
        NetworkConfig_Server.Socket.SendDataTo(GameManager_Server.host.connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
    }
    
    public static void SendMapDataChunk(int connectionID, int chunkID, int totalChunks, NetworkMapData_Type networkMapDataType, byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int) ServerPackets.MAP_DATA);
        buffer.WriteInt32((int) networkMapDataType);
        buffer.WriteInt32(chunkID);
        buffer.WriteInt32(totalChunks);
        buffer.WriteBytes(data);
        NetworkConfig_Server.Socket.SendDataTo(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
    }

    public static void SendPlayerLeft(int connectionID, PlayerLeave_Reason reason)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int) ServerPackets.PLAYER_LEAVE);
        buffer.WriteInt32((int) reason);
        NetworkConfig_Server.Socket.SendDataToAllBut(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
        GameManager_Server.playerList[connectionID].isConnected = false;
    }
}
