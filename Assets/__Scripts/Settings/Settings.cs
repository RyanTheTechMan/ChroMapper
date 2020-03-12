using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using UnityEngine;
using System.Globalization;

public class Settings {

    private static Settings _instance;
    public static Settings Instance => _instance ?? (_instance = Load());

    [BetterInputField] public string BeatSaberInstallation = "";
    public string CustomSongsFolder => ConvertToDirectory(BeatSaberInstallation + "/Beat Saber_Data/CustomLevels");
    public string CustomWIPSongsFolder => ConvertToDirectory(BeatSaberInstallation + "/Beat Saber_Data/CustomWIPLevels");
    [BetterToggle] public bool DiscordRPCEnabled = true;
    [BetterToggle] public bool OSC_Enabled = false;
    [BetterInputField] public string OSC_IP = "127.0.0.1";
    [BetterInputField] public string OSC_Port = "8080";
    [BetterSlider] public int EditorScale = 4;
    [BetterSlider] public int ChunkDistance = 5;
    [BetterInputField] public int AutoSaveInterval = 5;
    [BetterSlider] public int InitialLoadBatchSize = 100;
    [BetterToggle] public bool InvertNoteControls = false;
    [BetterToggle] public bool WaveformGenerator = false;
    [BetterToggle] public bool CountersPlus = false;
    [BetterToggle] public bool PlaceChromaEvents = false;
    [BetterToggle] public bool PickColorFromChromaEvents = false;
    [BetterToggle] public bool PlaceOnlyChromaEvents = false;
    [BetterToggle] public bool BongoBoye = false;
    [BetterToggle] public bool AutoSave = true;
    [BetterVolumeSlider] public float Volume = 1;
    [BetterVolumeSlider] public float MetronomeVolume = 0;
    [BetterToggle] public bool NodeEditor_Enabled = false;
    [BetterToggle] public bool NodeEditor_UseKeybind = false;
    public float PostProcessingIntensity = 1;
    [BetterToggle] public bool Reminder_SavingCustomEvents = true;
    [BetterToggle] public bool DarkTheme = false;
    [BetterToggle] public bool BoxSelect = false;
    [BetterToggle] public bool DontPlacePerfectZeroDurationWalls = true;
    [BetterSlider] public float Camera_MovementSpeed = 15;
    [BetterSlider] public float Camera_MouseSensitivity = 2;
    [BetterToggle] public bool EmulateChromaLite = true; //To get Chroma RGB lights
    [BetterToggle] public bool EmulateChromaAdvanced = true; //Ring propagation and other advanced chroma features
    [BetterToggle] public bool RotateTrack = true; // 360/90 mode
    [BetterToggle] public bool HighlightLastPlacedNotes = false;
    [BetterToggle] public bool InvertPrecisionScroll = false;
    [BetterToggle] public bool Reminder_Loading360Levels = true;
    [BetterToggle] public bool Reminder_SettingsFailed = true;
    [BetterToggle] public bool AdvancedShit = false;
    [BetterToggle] public bool InstantEscapeMenuTransitions = false;
    [BetterToggle] public bool ChromaticAberration = true;
    [BetterSlider] public int Offset_Spawning = 4;
    [BetterSlider] public int Offset_Despawning = 1;
    [BetterDropdown] public int NoteHitSound = 0;
    [BetterVolumeSlider] public float NoteHitVolume = 0.5f;
    [BetterSlider] public float PastNotesGridScale = 0.5f;
    [BetterSlider] public float CameraFOV = 60f;
    [BetterToggle] public bool WaveformWorkflow = true;
    [BetterToggle] public bool Load_Events = true;
    [BetterToggle] public bool Load_Notes = true;
    [BetterToggle] public bool Load_Obstacles = true;
    [BetterToggle] public bool Load_Others = true;

    public static Dictionary<string, FieldInfo> AllFieldInfos = new Dictionary<string, FieldInfo>();
    
    private static Settings Load()
    {
        //Fixes weird shit regarding how people write numbers (20,35 VS 20.35), causing issues in JSON
        //This should be thread-wide, but I have this set throughout just in case it isnt.
        System.Threading.Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;
        System.Threading.Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;

        bool settingsFailed = false;

        Settings settings = new Settings();
        if (!File.Exists(Application.persistentDataPath + "/ChroMapperSettings.json")) return settings;
        using (StreamReader reader = new StreamReader(Application.persistentDataPath + "/ChroMapperSettings.json")) //todo: save as object
        {
            JSONNode mainNode = JSON.Parse(reader.ReadToEnd());
            Type type = settings.GetType();
            MemberInfo[] infos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance);
            foreach (MemberInfo info in infos)
            {
                try
                {
                    if (!(info is FieldInfo field)) continue;
                    AllFieldInfos.Add(field.Name, field);
                    if (mainNode[field.Name] != null)
                        field.SetValue(settings, Convert.ChangeType(mainNode[field.Name].Value, field.FieldType));
                }catch(Exception e)
                {
                    Debug.LogWarning($"Setting {info.Name} failed to load.\n{e}");
                    settingsFailed = true;
                }
            }
        }
        if (settingsFailed)
        {
            PersistentUI.Instance.ShowDialogBox("Some ChroMapper settings failed to load.\n\n" +
                "If this dialog box keeps showing up when launching ChroMapper, try deleting your Configuration file located in:\n" +
                $"{Application.persistentDataPath}/ChroMapperSettings.json",
                Instance.HandleFailedReminder, "Ok", "Don't Remind Me");
        }
        return settings;
    }

    private void HandleFailedReminder(int res)
    {
        Reminder_SettingsFailed = res == 0;
    }

    public void Save()
    {
        JSONObject mainNode = new JSONObject();
        Type type = GetType();
        FieldInfo[] infos = type.GetMembers(BindingFlags.Public | BindingFlags.Instance).Where(x => x is FieldInfo).OrderBy(x => x.Name).Cast<FieldInfo>().ToArray();
        foreach (FieldInfo info in infos) mainNode[info.Name] = info.GetValue(this).ToString();
        using (StreamWriter writer = new StreamWriter(Application.persistentDataPath + "/ChroMapperSettings.json", false))
            writer.Write(mainNode.ToString(2));
    }

    public static bool ValidateDirectory(Action<string> errorFeedback = null) {
        if (!Directory.Exists(Instance.BeatSaberInstallation)) {
            errorFeedback?.Invoke("That folder does not exist!");
            return false;
        }
        if (!Directory.Exists(Instance.CustomSongsFolder)) {
            errorFeedback?.Invoke("No \"Beat Saber_Data\" or \"CustomLevels\" folder was found at chosen location!");
            return false;
        }
        if (!Directory.Exists(Instance.CustomWIPSongsFolder))
        {
            errorFeedback?.Invoke("No \"CustomWIPLevels\" folder was found at chosen location!");
            return false;
        }
        return true;
    }

    public static void ApplyOptionByName(string name, object value)
    {
        try
        {
            AllFieldInfos.TryGetValue(name, out FieldInfo fieldInfo);
            fieldInfo?.SetValue(Instance, value);
        }
        catch (ArgumentException e)
        {
            PersistentUI.Instance.ShowDialogBox($"Someone fucked up and failed applying a setting:\n{e}", null, PersistentUI.DialogBoxPresetType.Ok);
        }
    }
    
    public static object GetOptionByName(string name)
    {
        try
        {
            AllFieldInfos.TryGetValue(name, out FieldInfo fieldInfo);
            return fieldInfo?.GetValue(Instance);
        }
        catch (ArgumentException e)
        {
            PersistentUI.Instance.ShowDialogBox($"Someone fucked up and failed applying a setting:\n{e}", null, PersistentUI.DialogBoxPresetType.Ok);
        }

        return null;
    }
    
    public static string ConvertToDirectory(string s) => s.Replace('\\', '/');
}

public class BetterDropdownAttribute : Attribute {}
public class BetterInputFieldAttribute : Attribute {}
public class BetterSliderAttribute : Attribute {}
public class BetterVolumeSliderAttribute : Attribute {}
public class BetterToggleAttribute : Attribute {}
