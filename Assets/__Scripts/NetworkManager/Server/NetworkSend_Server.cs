using System;
using System.Collections.Generic;
using System.Text;
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
            if (GameManager_Server.playerList[i] != null && GameManager_Server.playerList[i].inEditor && i != connectionID)
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
    
    public static void SendMapDataChunk(int connectionID, int chunkID, int totalChunks, NetworkMapData_Type networkMapDataType, string fileName, byte[] data, short extra1, short extra2)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteInt32((int) ServerPackets.MAP_DATA);
        buffer.WriteInt32((int) networkMapDataType);
        buffer.WriteInt32(chunkID);
        buffer.WriteInt32(totalChunks);
        buffer.WriteString(fileName);
        buffer.WriteBytes(data);
        buffer.WriteInt16(extra1);
        buffer.WriteInt16(extra2);
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

    public static void SendMedioConnect(int connectionID)
    {
        ByteBuffer buffer = new ByteBuffer(4);
        buffer.WriteBytes(Encoding.UTF8.GetBytes("dc:duplicate name"));
        NetworkConfig_Server.Socket.SendDataTo(connectionID, buffer.Data, buffer.Head);
        buffer.Dispose();
        //buffer.WriteString(config.folder.split('/').pop()+"::"+diffIndex*100+ charIndex+";;;"); //0
        //buffer.WriteString(JSON.stringify(info)+";;;"); //1
        //buffer.WriteString(info._difficultyBeatmapSets[charIndex]._difficultyBeatmaps[diffIndex]._beatmapFilename+";;;"); //2
        //buffer.WriteString(JSON.stringify(diff)+";;;");//3
        //buffer.WriteString(info._songFilename+";;;"+fileSize+";;;");//4 & 5
        //buffer.WriteString(downloadURL +";;;" );//6
    }
}
