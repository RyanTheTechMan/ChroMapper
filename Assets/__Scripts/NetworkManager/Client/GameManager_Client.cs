using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Discord;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager_Client : MonoBehaviour
{
    public Dictionary<int, GameObject> playerList = new Dictionary<int, GameObject>();

    public static GameManager_Client instance;
    public GameObject currentCamera;
    public SendLocationOverNetwork LONScript;
    public Transform BeatmapActionContainer;
    public Slider loadingSlider;
    [HideInInspector] public TextMeshProUGUI SliderText;
    
    [HideInInspector] public TracksManager TracksManager;

    [HideInInspector] public string discordUsername;
    [HideInInspector] public string discordAvatar;

    [HideInInspector] public NetworkMapData_Type mapDataRequest = NetworkMapData_Type.NONE;
    
    [HideInInspector] public static byte[][] MapDataBytes = null;

    public string hostValidator = Guid.NewGuid().ToString();
    
    /// <summary>
    /// Use 'TemporaryDirectory.FullName' to get the location.
    /// </summary>
    public static readonly DirectoryInfo TemporaryDirectory = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString()));

    private void OnDestroy()
    {
        if(TemporaryDirectory.Exists) TemporaryDirectory.Delete(true);
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
    
}