using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SongEditMaster : MonoBehaviour
{
    
    
    
    private static readonly List<string> VanillaEnvironments = new List<string>()
    {
        "DefaultEnvironment",
        "BigMirrorEnvironment",
        "TriangleEnvironment",
        "NiceEnvironment",
        "KDAEnvironment",
        "MonstercatEnvironment",
        "DragonsEnvironment",
        "Origins",
        "CrabRaveEnvironment",
        "PanicEnvironment",
        "RocketEnvironment"
    };

    private static readonly List<string> VanillaDirectionalEnvironments = new List<string>()
    {
        "GlassDesertEnvironment"
    };

    public static readonly List<string> CharacteristicDropdownToBeatmapName = new List<string>()
    {
        "Standard",
        "NoArrows",
        "OneSaber",
        "360Degree",
        "90Degree",
        "Lightshow",
        "Lawless"
    };




    #region Unity Objects
    public TMP_InputField nameField;
    public TMP_InputField subNameField;
    public TMP_InputField songAuthorField;
    public TMP_InputField authorField;
    public TMP_InputField coverImageField;

    public TMP_InputField bpmField;
    public TMP_InputField prevStartField;
    public TMP_InputField prevDurField;
    
    public SelectItemFromList customPlatformsList;
    public SelectItemFromList environmentList;
    
    public SelectItemFromList characteristicList;

    public List<BeatSaberSong.DifficultyBeatmap> songDifficultyData = new List<BeatSaberSong.DifficultyBeatmap>();
    public List<BeatSaberSong.DifficultyBeatmapSet> songDifficultySets = new List<BeatSaberSong.DifficultyBeatmapSet>();
    public int selectedDifficultyIndex = -1;
    public string selectedBeatmapSet = "Standard";
    public Toggle WillChromaBeRequired;
    public TextMeshProUGUI HalfJumpDurationText;
    public TextMeshProUGUI JumpDistanceText;

    public GameObject difficultyExistsPanel;
    public GameObject difficultyNoExistPanel;
    public SelectItemFromList difficultyList;

    public TMP_InputField audioPath;
    public TMP_InputField offset;
    
    public TMP_InputField difficultyLabel;
    public TMP_InputField noteJumpSpeed;
    public TMP_InputField startBeatOffset;
    
    [SerializeField] private Image revertInfoButtonImage;
    #endregion
    
    
    //public SongEditMaster songEditInstance => this;

    #region Shared Variables
    protected BeatSaberSong Song => BeatSaberSongContainer.Instance.song;
    protected BeatSaberSong.DifficultyBeatmapSet SelectedSet => songDifficultySets.FirstOrDefault(x => x.beatmapCharacteristicName == selectedBeatmapSet);
    protected BeatSaberMap map => Song.GetMapFromDifficultyBeatmap(SelectedDifficultyData);
    protected BeatSaberSong.DifficultyBeatmap SelectedDifficultyData => songDifficultyData[selectedDifficultyIndex];
    #endregion
    
    private void Start() {
        if (BeatSaberSongContainer.Instance == null) {SceneManager.LoadScene(0); return;}
        LoadFromSong(true);
    }
    
    #region Load Data From Song
    public void LoadFromSong(bool initial)
    {
        if(!initial) _reloadSongDataCoroutine = StartCoroutine(SpinReloadSongDataButton());
        
        nameField.text = Song.songName;
        subNameField.text = Song.songSubName;
        songAuthorField.text = Song.songAuthorName;
        authorField.text = Song.levelAuthorName;
        coverImageField.text = Song.coverImageFilename;
        audioPath.text = Song.songFilename;
        offset.text = Song.songTimeOffset.ToString(CultureInfo.InvariantCulture);
        if (Song.songTimeOffset > 0)
        {
            PersistentUI.Instance.ShowDialogBox("Using Song Time Offset can result in desynced cut noises in game.\n\n" +
                "It is recommended that you apply your offsets using a audio manipulator such as Audacity.", null,
                PersistentUI.DialogBoxPresetType.Ok);
        }

        bpmField.text = Song.beatsPerMinute.ToString(CultureInfo.InvariantCulture);
        prevStartField.text = Song.previewStartTime.ToString(CultureInfo.InvariantCulture);
        prevDurField.text = Song.previewDuration.ToString(CultureInfo.InvariantCulture);

        
        
        /*environmentDropdown.ClearOptions();
        environmentDropdown.AddOptions(VanillaEnvironments);
        environmentDropdown.value = GetEnvironmentIDFromString(Song.environmentName);

        customPlatformsDropdown.ClearOptions();
        customPlatformsDropdown.AddOptions(new List<String> { "None" });
        customPlatformsDropdown.AddOptions(CustomPlatformsLoader.Instance.GetAllEnvironmentIds());

        if (Song.customData != null)
        {
            if (Song.customData["_customEnvironment"] != null && Song.customData["_customEnvironment"] != "")
                customPlatformsDropdown.value = CustomPlatformsLoader.Instance.GetAllEnvironmentIds().IndexOf(Song.customData["_customEnvironment"]) + 1;
            else
            { //For some reason the text defaults to "Dueling Dragons", not what we want.
                customPlatformsDropdown.value = 0;
                customPlatformsDropdown.captionText.text = "None";
            }
        }
        else
        {
            customPlatformsDropdown.value = 0;
            customPlatformsDropdown.captionText.text = "None";
        }
*/
        if (Song.difficultyBeatmapSets.Any())
        {
            songDifficultySets = Song.difficultyBeatmapSets;
            songDifficultyData = songDifficultySets.First().difficultyBeatmaps;
            selectedBeatmapSet = songDifficultySets.First().beatmapCharacteristicName;
            characteristicList.value = CharacteristicDropdownToBeatmapName.IndexOf(selectedBeatmapSet);
        }

        if (initial) InitializeDifficultyPanel();
    }
    #endregion
    
    #region Save Data From Song
    public void SaveToSong() {
        Song.songName = nameField.text;
        Song.songSubName = subNameField.text;
        Song.songAuthorName = songAuthorField.text;
        Song.levelAuthorName = authorField.text;
        Song.coverImageFilename = coverImageField.text;
        Song.songFilename = audioPath.text;

        Song.beatsPerMinute = float.Parse(bpmField.text);
        Song.previewStartTime = float.Parse(prevStartField.text);
        Song.previewDuration = float.Parse(prevDurField.text);
        Song.songTimeOffset = float.Parse(offset.text);

        if (Song.songTimeOffset > 0)
        {
            PersistentUI.Instance.ShowDialogBox("Using Song Time Offset can result in desynced cut noises in game.\n\n" +
                                                "It is recommended that you apply your offsets using a audio manipulator such as Audacity.", null,
                PersistentUI.DialogBoxPresetType.Ok);
        }

        /*Song.environmentName = GetEnvironmentNameFromID(environmentDropdown.value);

        if (Song.customData == null) Song.customData = new JSONObject();

        if (customPlatformsDropdown.value > 0)
        {
            string hash;
            Song.customData["_customEnvironment"] = customPlatformsDropdown.captionText.text;
            if (CustomPlatformsLoader.Instance.GetEnvironmentsWithHash().TryGetValue(customPlatformsDropdown.captionText.text, out hash))
                Song.customData["_customEnvironmentHash"] = hash;
        }
        else
        {
            Song.customData.Remove("_customEnvironment");
            Song.customData.Remove("_customEnvironmentHash");
        }*/

        Song.SaveSong();
        PersistentUI.Instance.DisplayMessage("Song Info Saved!", PersistentUI.DisplayMessageType.BOTTOM);
    }
    #endregion
    
    private Coroutine _reloadSongDataCoroutine;
    private IEnumerator SpinReloadSongDataButton()
    {
        if(_reloadSongDataCoroutine != null) StopCoroutine(_reloadSongDataCoroutine);
        
        float startTime = Time.time;
        var transform1 = revertInfoButtonImage.transform;
        Quaternion rotationQ = transform1.rotation;
        Vector3 rotation = rotationQ.eulerAngles;
        rotation.z = -110;
        transform1.rotation = Quaternion.Euler(rotation);
        
        revertInfoButtonImage.fillAmount = 0;
        float fillAmount = revertInfoButtonImage.fillAmount;
        
        while (true)
        {
            float rot = rotation.z;
            float timing = (Time.time / startTime) * 0.075f;
            rot = Mathf.Lerp(rot, 318f, timing);
            fillAmount = Mathf.Lerp(fillAmount, 1f, timing);
            revertInfoButtonImage.fillAmount = fillAmount;
            rotation.z = rot;
            transform1.rotation = Quaternion.Euler(rotation);
            
            if (rot >= 314f)
            {
                rotation.z = -45;
                transform1.rotation = Quaternion.Euler(rotation);
                revertInfoButtonImage.fillAmount = 1;
                yield break;
            }
            yield return new WaitForFixedUpdate();
        }
    }
}