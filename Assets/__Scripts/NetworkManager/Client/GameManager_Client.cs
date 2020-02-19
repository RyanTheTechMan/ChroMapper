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
    }

    private void Start()
    {
        TracksManager = BeatmapActionContainer.GetComponent<TracksManager>();
        SetupDiscordInfo();
    }

    private void SetupDiscordInfo()
    {
        Discord.Discord discord = ((DiscordController) FindObjectOfType(typeof(DiscordController))).discord;
        
        UserManager userManager = discord.GetUserManager();
        userManager.OnCurrentUserUpdate += () =>
        {
            User user = userManager.GetCurrentUser();
            discordUsername = user.Username;
            discordAvatar = "https://cdn.discordapp.com/avatars/" + user.Id + "/" + user.Avatar + ".png";
        };
    }
}