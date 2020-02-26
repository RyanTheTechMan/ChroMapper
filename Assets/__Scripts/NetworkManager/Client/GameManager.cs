using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public static Dictionary<int, PlayerManager> players = new Dictionary<int, PlayerManager>();

    public GameObject localPlayerPrefab;
    public GameObject playerPrefab;
    
    public GameObject currentCamera;
    [HideInInspector] public SendLocationOverNetwork LONScript;
    [HideInInspector] public Transform BeatmapActionContainer;
    public Slider loadingSlider;
    [HideInInspector] public TextMeshProUGUI SliderText;
    
    [HideInInspector] public TracksManager TracksManager;

    [HideInInspector] public string discordUsername;
    [HideInInspector] public string discordAvatar;

    [HideInInspector] public NetworkMapData_Type mapDataRequest = NetworkMapData_Type.NONE;
    public static byte[][] MapDataBytes;

    public string hostValidator = Guid.NewGuid().ToString();
    
    //[HideInInspector] public List<Client> initQueue = new List<Client>();

    public static bool inMapperScene = false;

    /// <summary>
    /// Use 'TemporaryDirectory.FullName' to get the location.
    /// </summary>
    public static readonly DirectoryInfo TemporaryDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Debug.Log("Instance already exists, destroying object!");
            Destroy(this);
        }
    }
    
    private void OnDestroy()
    {
        if(TemporaryDirectory.Exists) TemporaryDirectory.Delete(true);
    }

    private void OnEnable()
    {
        instance = this;

        if (gameObject.scene.name == "03_Mapper")
        {
            TracksManager = FindObjectOfType<TracksManager>();
            BeatmapActionContainer = TracksManager.gameObject.transform;
            LONScript = currentCamera.GetComponent<SendLocationOverNetwork>();
            inMapperScene = true;
        }
        else if (gameObject.scene.name == "05_MultiplayerHelper")
        {
            SliderText = loadingSlider.GetComponentInChildren<TextMeshProUGUI>();
            inMapperScene = false;
        }
        
        SetupDiscordInfo();
        DontDestroyOnLoad(this);
    }

    /// <summary>Spawns a player.</summary>
    /// <param name="_id">The player's ID.</param>
    /// <param name="_name">The player's name.</param>
    /// <param name="_position">The player's starting position.</param>
    /// <param name="_rotation">The player's starting rotation.</param>
    public void SpawnPlayer(int _id, string _username, Vector3 _position, Quaternion _rotation)
    {
        GameObject _player;
        if (_id == Client.instance.myId)
        {
            _player = Instantiate(localPlayerPrefab, _position, _rotation);
        }
        else
        {
            _player = Instantiate(playerPrefab, _position, _rotation);
        }

        _player.GetComponent<PlayerManager>().id = _id;
        _player.GetComponent<PlayerManager>().username = _username;
        players.Add(_id, _player.GetComponent<PlayerManager>());
    }

    private void SetupDiscordInfo()
    {
        NetworkManager_Client.Log("Starting discord info");

        if (DiscordController.IsActive)
        {
            DiscordController discordController = (DiscordController) FindObjectOfType(typeof(DiscordController));
            Discord.Discord discord = discordController.discord;
            UserManager userManager = discord.GetUserManager();
            userManager.OnCurrentUserUpdate += () =>
            {
                User user = userManager.GetCurrentUser();
                discordUsername = user.Username;
                discordAvatar = "https://cdn.discordapp.com/avatars/" + user.Id + "/" + user.Avatar + ".png";
                NetworkManager_Client.instance.SetupNetwork();
            };
        }
        else
        {
            discordUsername = "";
            discordAvatar = "";
            NetworkManager_Client.instance.SetupNetwork();
        }
    }

    public void SaveMapData(string fileLocation)
    {
        StartCoroutine(saveMapData(fileLocation));
    }
    
    private IEnumerator saveMapData(string fileLocation)
    {
        FileStream fs = new FileStream(fileLocation, FileMode.OpenOrCreate, FileAccess.Write);
        NetworkManager_Client.Log("Writing {0} to {1}", instance.mapDataRequest, fileLocation);
        instance.SliderText.text = "Saving " + fileLocation.Substring(fileLocation.IndexOf("/", StringComparison.Ordinal) + 1).ToLower();
        for (int i = 0; i < MapDataBytes.Length; i++)
        {
            byte[] b = MapDataBytes[i];
            instance.loadingSlider.value = Mathf.Abs(i / (float) MapDataBytes.Length);
            fs.Write(b, 0, b.Length);
            yield return new WaitForEndOfFrame();
        }

        fs.Close();
        
        NetworkManager_Client.Log("Finished Writing {0} to {1}", instance.mapDataRequest, fileLocation);
        MapDataBytes = null;
        instance.mapDataRequest = NetworkMapData_Type.NONE;
        yield return true;
    }

    public void RequestForMapData()
    {
        StartCoroutine(RequestMapData());
    }
    
    private IEnumerator RequestMapData()
    {
        while (instance.mapDataRequest != NetworkMapData_Type.NONE) {yield return new WaitForEndOfFrame();}
        NetworkSend_Client.SendRequestForMapData(NetworkMapData_Type.INFO);
        
        while (instance.mapDataRequest != NetworkMapData_Type.NONE) {yield return new WaitForEndOfFrame();}
        NetworkSend_Client.SendRequestForMapData(NetworkMapData_Type.DIFFICULTY);
        
        while (instance.mapDataRequest != NetworkMapData_Type.NONE) {yield return new WaitForEndOfFrame();}
        NetworkSend_Client.SendRequestForMapData(NetworkMapData_Type.SONG);
        
        while (instance.mapDataRequest != NetworkMapData_Type.NONE) {yield return new WaitForEndOfFrame();}
        
        NetworkManager_Client.Log("Received all Map Data! {0}", 
            (TemporaryDirectory.GetFiles("*", 
                SearchOption.AllDirectories).Sum(fi => fi.Length) / 1000.0 / 1000.0).ToString("F2") + " Mega Bytes");
        
        yield return new WaitForSeconds(3);
        
        //BeatSaberSongContainer.Instance.song = BeatSaberSong.GetSongFromFolder(TemporaryDirectory.FullName);
        
        JSONNode mainNode = BeatSaberSong.GetNodeFromFile(TemporaryDirectory.FullName + "/" + "info.dat");
        if (mainNode == null)
        {
            Debug.LogWarning("Failed to get difficulty json file");
            yield break;
        }
        
        BeatSaberSongContainer.Instance.map = BeatSaberMap.GetBeatSaberMapFromJSON(mainNode, TemporaryDirectory.FullName + "/" + "info.dat");
        FindObjectOfType<SongInfoEditUI>().GetSongFromDifficultyData(BeatSaberSongContainer.Instance.map);
        
        SceneManager.LoadSceneAsync(3);

        currentCamera = FindObjectOfType<CameraController>().gameObject;
        LONScript = currentCamera.GetComponent<SendLocationOverNetwork>();

        TracksManager = FindObjectOfType<TracksManager>();
        BeatmapActionContainer = TracksManager.gameObject.transform;
        
        inMapperScene = true;

        if (initQueue.Count > 0)
        {
            foreach (Player_Client pc in initQueue)
            {
                NetworkManager_Client.instance.InitNetworkPlayer(pc.ConnectionID, pc.Setup_CurrentPlayer, pc.Username, pc.Avatar);
            }
        }
        
        yield return this;
    }
}
