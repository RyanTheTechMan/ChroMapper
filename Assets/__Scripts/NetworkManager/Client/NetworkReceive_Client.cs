using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaymakNetwork;
using SimpleJSON;
using UnityEngine;

internal abstract class NetworkReceive_Client
{
    internal static void PacketRouter()
    {
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.WELCOME_MSG] = Packet_WelcomeMsg;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.Instantiate_Player] = Packet_InitNetworkPlayer;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.PLAYER_MOVE] = Packet_PlayerMove;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.PLAYER_ROTATE] = Packet_PlayerRotate;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.ACTION] = Packet_Action;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.USER_KICK] = Packet_Kick;
        NetworkConfig_Client.socket.PacketId[(int)ServerPackets.MAP_DATA] = Packet_MapData;
    }

    private static void Packet_WelcomeMsg(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        string msg = buffer.ReadString();
        buffer.Dispose();

        NetworkManager_Client.instance.connectionID = connectionID;
        
        NetworkSend_Client.SendPing();

    }

    private static void Packet_InitNetworkPlayer(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        string username = buffer.ReadString();
        string avatar = buffer.ReadString();

        NetworkManager_Client.instance.
            InitNetworkPlayer(connectionID, 
                connectionID == NetworkManager_Client.instance.connectionID,
                username, avatar);

        buffer.Dispose();
    }

    private static void Packet_PlayerMove(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        float x = buffer.ReadSingle();
        float y = buffer.ReadSingle();
        float z = buffer.ReadSingle();
        buffer.Dispose();

        if(!GameManager_Client.instance.playerList.ContainsKey(connectionID)) return;
        
        GameManager_Client.instance.playerList[connectionID].transform.position = new Vector3(x,y,z);
    }
    
    private static void Packet_PlayerRotate(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        float x = buffer.ReadSingle();
        float y = buffer.ReadSingle();
        float z = buffer.ReadSingle();
        float w = buffer.ReadSingle();
        buffer.Dispose();

        if(!GameManager_Client.instance.playerList.ContainsKey(connectionID)) return;
        
        GameManager_Client.instance.playerList[connectionID].transform.rotation = new Quaternion(x,y,z, w);
    }

    private static void Packet_Action(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        string beatmapObject = buffer.ReadString();
        BeatmapActionType beatmapActionType = (BeatmapActionType) buffer.ReadInt32();
        BeatmapObject.Type beatmapObjectType = (BeatmapObject.Type) buffer.ReadInt32();
        buffer.Dispose();

        NetworkManager_Client.Log("Received a {2} action with data: {0} and object type {1}", beatmapObject, beatmapObjectType.ToString(), beatmapActionType.ToString());
        
        List<BeatmapObjectContainerCollection> beatmapActionContainers = GameManager_Client.instance.BeatmapActionContainer.GetComponents<BeatmapObjectContainerCollection>().ToList(); //This will be changed
        
        JSONNode node = JSON.Parse(beatmapObject);

        BeatmapObject bo;
        
        switch (beatmapObjectType)
        {
            case BeatmapObject.Type.NOTE:
                bo = new BeatmapNote(node) {beatmapType = beatmapObjectType};
                break;
            case BeatmapObject.Type.OBSTACLE:
                bo = new BeatmapObstacle(node) {beatmapType = beatmapObjectType};
                break;
            case BeatmapObject.Type.EVENT:
                bo = new MapEvent(node) {beatmapType = beatmapObjectType};
                break;
            case BeatmapObject.Type.CUSTOM_EVENT:
                bo = new BeatmapCustomEvent(node) {beatmapType = beatmapObjectType};
                break;
            case BeatmapObject.Type.BPM_CHANGE:
                bo = new BeatmapBPMChange(node) {beatmapType = beatmapObjectType};
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(beatmapObjectType), "Invalid Beatmap Object Type Received!!!");
        }
        beatmapActionContainers.FirstOrDefault(x => x.ContainerType == beatmapObjectType)?.SpawnObject(bo, out _, false);
        GameManager_Client.instance.TracksManager.RefreshTracks();
        
    }
    
    private static void Packet_Kick(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        string reason = buffer.ReadString();
        buffer.Dispose();
        NetworkConfig_Client.socket.Disconnect();
        
        NetworkManager_Client.Log("You Have Been Kicked For {0}", reason);
        //todo display reason here. KICK USER OUT OF MAPPER SCENE
    }
    
    private static void Packet_MapData(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        //NetworkMapData_Type networkMapDataType = (NetworkMapData_Type) buffer.ReadInt32();
        byte[] chunk = buffer.ReadBytes();
        buffer.Dispose();
        NetworkConfig_Client.socket.Disconnect();
    }

}
