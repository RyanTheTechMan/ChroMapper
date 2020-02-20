using System;
using System.Collections.Generic;
using System.IO;
using Discord;
using UnityEngine;

public class GameManager_Client : MonoBehaviour
{
    public Dictionary<int, GameObject> playerList = new Dictionary<int, GameObject>();

    public static GameManager_Client instance;
    public GameObject currentCamera;
    public SendLocationOverNetwork LONScript;
    public Transform BeatmapActionContainer;
    
    
    [HideInInspector] public TracksManager TracksManager;

    [HideInInspector] public string discordUsername;
    [HideInInspector] public string discordAvatar;
    
    [HideInInspector] public bool downloadedInfo;
    [HideInInspector] public bool downloadedDifficulty;
    [HideInInspector] public bool downloadedSong;

    [HideInInspector] public NetworkMapData_Type mapDataRequest = NetworkMapData_Type.NONE;

    public string hostValidator = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Use 'TemporaryDirectory.FullName' to get the location.
    /// </summary>
    public DirectoryInfo TemporaryDirectory
    {
        get => TemporaryDirectory ?? (TemporaryDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())));
        private set => TemporaryDirectory = value;
    }

    private void OnDestroy()
    {
        if(TemporaryDirectory.Exists) TemporaryDirectory.Delete();
    }

    private void Awake()
    {
        instance = this;
        SetupDiscordInfo();
    }

    private void Start()
    {
        TracksManager = BeatmapActionContainer.GetComponent<TracksManager>();
    }

    private void SetupDiscordInfo()
    {
        NetworkManager_Client.Log("Starting discord info");
        DiscordController discordController = (DiscordController) FindObjectOfType(typeof(DiscordController));

        if (DiscordController.IsActive)
        {

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
}