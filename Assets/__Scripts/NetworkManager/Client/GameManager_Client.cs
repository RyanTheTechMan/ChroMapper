using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager_Client : MonoBehaviour
{
    public Dictionary<int, GameObject> playerList = new Dictionary<int, GameObject>();

    public static GameManager_Client instance;
    public GameObject currentCamera;
    public Slider loadingSlider;
    [Space][Space]
    public SendLocationOverNetwork LONScript;
    public Transform BeatmapActionContainer;
    
    public TextMeshProUGUI SliderText;
    
    public TracksManager TracksManager;

    public string discordUsername;
    public string discordAvatar;

    public NetworkMapData_Type mapDataRequest = NetworkMapData_Type.NONE;
    public static byte[][] MapDataBytes;
    public static string[] MapDataHelper = new string[3];
    private static Scene initialScene;

    public string hostValidator = Guid.NewGuid().ToString();
    
    [HideInInspector] public List<Player_Client> initQueue = new List<Player_Client>();

    public static bool inMapperScene = false;

    /// <summary>
    /// Use 'TemporaryDirectory.FullName' to get the location.
    /// </summary>
    public static readonly DirectoryInfo TemporaryDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())); //todo fix this so It returns the same directory until destroy

    private void Awake()
    {
        initialScene = gameObject.scene;
    }

    private void OnDestroy()
    {
        if(TemporaryDirectory.Exists) TemporaryDirectory.Delete(true);
        if(NetworkConfig_Client.socket.IsConnected) NetworkConfig_Client.socket.Disconnect();
    }

    private void OnEnable()
    {
        instance = this;

        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene == SceneManager.GetSceneByName("03_Mapper"))
        {
            currentCamera = FindObjectOfType<Camera>().gameObject;
            TracksManager = FindObjectOfType<TracksManager>();
            BeatmapActionContainer = TracksManager.gameObject.transform;
            LONScript = currentCamera.GetComponent<SendLocationOverNetwork>();
            GetComponent<SendBeatmapActionOverNetwork>().enabled = true;
            inMapperScene = true;
        }
        else if (activeScene == SceneManager.GetSceneByName("05_MultiplayerHelper"))
        {
            SliderText = loadingSlider.GetComponentInChildren<TextMeshProUGUI>();
            inMapperScene = false;
        }
        
        SetupDiscordInfo();
        DontDestroyOnLoad(this);
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
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
            instance.SliderText.text = "Saving " + fileLocation.Substring(fileLocation.IndexOf("/", StringComparison.Ordinal) + 1).ToLower() + " | " + (instance.loadingSlider.value*100.0) + "%";
            fs.Write(b, 0, b.Length);
            yield return new WaitForEndOfFrame();
        }

        fs.Close();
        
        NetworkManager_Client.Log("Finished Writing {0} to {1}", instance.mapDataRequest, fileLocation);
        MapDataBytes = null;
        instance.mapDataRequest = NetworkMapData_Type.NONE;
        yield break;
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
        
        BeatSaberSongContainer.Instance.song = BeatSaberSong.GetSongFromFolder(TemporaryDirectory.FullName);
        
        BeatSaberSongContainer.Instance.difficultyData = BeatSaberSongContainer.Instance.song
            .difficultyBeatmapSets[Convert.ToInt32(MapDataHelper[1])]
            .difficultyBeatmaps[Convert.ToInt32(MapDataHelper[2])];
        
        BeatSaberSongContainer.Instance.map = BeatSaberSongContainer.Instance.song.GetMapFromDifficultyBeatmap(BeatSaberSongContainer.Instance.difficultyData);

        if (File.Exists(TemporaryDirectory.FullName + "/" + MapDataHelper[0]))
        {
            if (MapDataHelper[0].ToLower().EndsWith("ogg") ||  MapDataHelper[0].ToLower().EndsWith("egg"))
            {
                UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip($"file:///{Uri.EscapeDataString($"{TemporaryDirectory.FullName}/{MapDataHelper[0]}")}", AudioType.OGGVORBIS);
                yield return www.SendWebRequest();
                Debug.Log("Song loaded!");
                AudioClip clip = DownloadHandlerAudioClip.GetContent(www);
                if (clip == null)
                {
                    Debug.Log("Error getting Audio data!");
                    SceneTransitionManager.Instance.CancelLoading("Error getting Audio data!");
                }
                clip.name = "Song";
                BeatSaberSongContainer.Instance.loadedSong = clip;
            }
            else
            {
                Debug.Log("Incompatible file type! WTF!?");
                SceneTransitionManager.Instance.CancelLoading("Incompatible audio type!");
                yield break;
            }
        }
        else
        {
            SceneTransitionManager.Instance.CancelLoading("Audio file does not exist!");
            Debug.Log("Song does not exist! WTF!?");
            Debug.Log(TemporaryDirectory.FullName + "/" + MapDataHelper[0]);
            yield break;
        }

        SceneTransitionManager.Instance.LoadScene(3);
        
        yield return this;
    }

    private IEnumerator LoadIntoMapperSceneFromMultiplayer()
    {
        instance.currentCamera = FindObjectOfType<Camera>().gameObject;
        instance.LONScript = instance.currentCamera.GetComponent<SendLocationOverNetwork>();

        instance.LONScript.enabled = true;
        GetComponent<SendBeatmapActionOverNetwork>().enabled = true;

        instance.TracksManager = FindObjectOfType<TracksManager>();
        instance.BeatmapActionContainer = instance.TracksManager.gameObject.transform;
        
        inMapperScene = true;

        if (instance.initQueue.Count > 0)
        {
            foreach (Player_Client pc in instance.initQueue)
            {
                NetworkManager_Client.instance.InitNetworkPlayer(pc.ConnectionID, pc.Setup_CurrentPlayer, pc.Username, pc.Avatar);
                Debug.Log("Now running Init Player for: " + pc.Username + " : " + pc.ConnectionID);
            }

            instance.initQueue = null;
        }
        yield break;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if (scene.name != "03_Mapper" && scene.name != "04_Options" && scene.name != "05_MultiplayerHelper")
        {
            foreach (NetworkManager_Client c in FindObjectsOfType<NetworkManager_Client>())
            {
                Destroy(c.gameObject);
            }

            foreach (NetworkManager_Server s in FindObjectsOfType<NetworkManager_Server>())
            {
                Destroy(s.gameObject);
            }
            return;
        }
        
        Debug.Log("Scene: " + scene.name + "\nInitScene: " + initialScene.name);
        if (initialScene.name == "DontDestroyOnLoad" && scene.name == "03_Mapper")
        {
            SceneTransitionManager.Instance.AddLoadRoutine(LoadIntoMapperSceneFromMultiplayer());
        }
    }
}