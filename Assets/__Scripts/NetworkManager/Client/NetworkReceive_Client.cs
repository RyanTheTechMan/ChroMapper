using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using KaymakNetwork;
using SimpleJSON;
using UnityEngine;
using Object = UnityEngine.Object;

internal abstract class NetworkReceive_Client
{
    internal static void PacketRouter()
    {
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.WELCOME_MSG] = Packet_WelcomeMsg;
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.Instantiate_Player] = Packet_InitNetworkPlayer;
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.PLAYER_MOVE] = Packet_PlayerMove;
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.PLAYER_ROTATE] = Packet_PlayerRotate;
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.ACTION] = Packet_Action;
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.USER_KICK] = Packet_Kick;
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.MAP_DATA] = Packet_MapData;
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.MAP_DATA_REQUEST_TO_HOST] = Packet_MapDataRequestToHost;
        NetworkConfig_Client.socket.PacketId[(int) ServerPackets.PLAYER_LEAVE] = Packet_PlayerLeft;
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

        NetworkManager_Client.instance.InitNetworkPlayer(connectionID,
            connectionID == NetworkManager_Client.instance.connectionID, username, avatar);

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

        if (!GameManager_Client.instance.playerList.ContainsKey(connectionID)) return;

        GameManager_Client.instance.playerList[connectionID].transform.position = new Vector3(x, y, z);
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

        if (!GameManager_Client.instance.playerList.ContainsKey(connectionID)) return;

        GameManager_Client.instance.playerList[connectionID].transform.rotation = new Quaternion(x, y, z, w);
    }

    private static void Packet_Action(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        string beatmapObject = buffer.ReadString();
        BeatmapActionType beatmapActionType = (BeatmapActionType) buffer.ReadInt32();
        BeatmapObject.Type beatmapObjectType = (BeatmapObject.Type) buffer.ReadInt32();
        buffer.Dispose();

        NetworkManager_Client.Log("Received a {2} action with data: {0} and object type {1}", beatmapObject,
            beatmapObjectType.ToString(), beatmapActionType.ToString());

        List<BeatmapObjectContainerCollection> beatmapActionContainers = GameManager_Client.instance
            .BeatmapActionContainer.GetComponents<BeatmapObjectContainerCollection>().ToList(); //This will be changed

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
                throw new ArgumentOutOfRangeException(nameof(beatmapObjectType),
                    "Invalid Beatmap Object Type Received!!!");
        }

        beatmapActionContainers.FirstOrDefault(x => x.ContainerType == beatmapObjectType)
            ?.SpawnObject(bo, out _, false);
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
    
    private static void Packet_PlayerLeft(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        int connectionID = buffer.ReadInt32();
        PlayerLeave_Reason reason = (PlayerLeave_Reason) buffer.ReadInt32();
        buffer.Dispose();

        GameObject pl = GameManager_Client.instance.playerList[connectionID];
        
        NetworkManager_Client.Log("{0} Has left because {1}", pl.name, reason.ToString());
        
        Object.Destroy(pl);
        
    }

    private static void Packet_MapData(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        NetworkMapData_Type networkMapDataType = (NetworkMapData_Type) buffer.ReadInt32();
        int chunkID = buffer.ReadInt32();
        int totalChunks = buffer.ReadInt32();
        byte[] chunk = buffer.ReadBytes();
        buffer.Dispose();

        if (GameManager_Client.instance.mapDataRequest != networkMapDataType)
        {
            NetworkManager_Client.Log("Received {0} packet when expecting {1}",
                networkMapDataType.ToString(),
                GameManager_Client.instance.mapDataRequest.ToString());
            return;
        }

        string folderLocation = GameManager_Client.TemporaryDirectory.FullName;
        FileStream fs = null;
        switch (networkMapDataType)
        {
            case NetworkMapData_Type.INFO:
                fs = new FileStream(folderLocation + "/info.json", FileMode.OpenOrCreate, FileAccess.Write);
                break;
            case NetworkMapData_Type.DIFFICULTY:
                fs = new FileStream(folderLocation + "/.json", FileMode.OpenOrCreate, FileAccess.Write);//todo also send correctName
                break;
            case NetworkMapData_Type.SONG:
                fs = new FileStream(folderLocation + "/.ogg", FileMode.OpenOrCreate, FileAccess.Write);//todo also send correctName
                break;
            default:
                return;
        }
        
        fs.Write(chunk, 0, chunk.Length);
        fs.Close(); //todo make it so the client asks for the next packet so it wont FREEZE!!

        if (chunkID == 1) NetworkManager_Client.Log("Writing {0} to {1}", networkMapDataType.ToString(), folderLocation);
        
        if (chunkID == totalChunks)
        {
            NetworkManager_Client.Log("Finished Writing {0} to {1}", networkMapDataType.ToString(), folderLocation);
            GameManager_Client.instance.mapDataRequest = NetworkMapData_Type.NONE;
        }
        
    }

    private static void Packet_MapDataRequestToHost(ref byte[] data)
    {
        ByteBuffer buffer = new ByteBuffer(data);
        NetworkMapData_Type networkMapDataType = (NetworkMapData_Type) buffer.ReadInt32();
        int requestedClient = buffer.ReadInt32();
        buffer.Dispose();

        string fileLoc = "";
        
        switch (networkMapDataType)
        {
            case NetworkMapData_Type.INFO:
                fileLoc = BeatSaberSongContainer.Instance.song.directory + "/info.dat";
                break;
            case NetworkMapData_Type.DIFFICULTY:
                fileLoc = BeatSaberSongContainer.Instance.map.directoryAndFile;
                break;
            case NetworkMapData_Type.SONG:
                fileLoc = BeatSaberSongContainer.Instance.song.directory + "/" + BeatSaberSongContainer.Instance.song.songFilename;
                break;
        }

        if (fileLoc == "")
        {
            NetworkManager_Client.Log("Something went wrong with retrieving data from the client.");
            return;
        }
        
        NetworkSend_Client.SendMapData(networkMapDataType, requestedClient, fileLoc);
    }

}
