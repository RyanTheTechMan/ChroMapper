using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SimpleJSON;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DifficultyPanel : SongEditMaster
{

    private void Start()
    {

    }

    public void SaveDifficulty()
    {
        if (SelectedDifficultyData.customData == null)
            SelectedDifficultyData.customData = new JSONObject();

        string oldPath = map?.directoryAndFile;
        switch (difficultyList.value)
        {
            case 0:
                SelectedDifficultyData.difficulty = "Easy";
                SelectedDifficultyData.difficultyRank = 1;
                break;
            case 1:
                SelectedDifficultyData.difficulty = "Normal";
                SelectedDifficultyData.difficultyRank = 3;
                break;
            case 2:
                SelectedDifficultyData.difficulty = "Hard";
                SelectedDifficultyData.difficultyRank = 5;
                break;
            case 3:
                SelectedDifficultyData.difficulty = "Expert";
                SelectedDifficultyData.difficultyRank = 7;
                break;
            case 4:
                SelectedDifficultyData.difficulty = "ExpertPlus";
                SelectedDifficultyData.difficultyRank = 9;
                break;
            default:
                Debug.Log("Difficulty doesnt seem to exist! Default to Easy...");
                SelectedDifficultyData.difficulty = "Easy";
                SelectedDifficultyData.difficultyRank = 1;
                break;
        }

        SelectedDifficultyData.UpdateName();

        if (map is null) map = new BeatSaberMap {mainNode = new JSONObject()};
        
        map.directoryAndFile = $"{Song.directory}\\{SelectedDifficultyData.beatmapFilename}";
        if (File.Exists(oldPath) && oldPath != map.directoryAndFile && !File.Exists(map.directoryAndFile))
            File.Move(oldPath, map.directoryAndFile); //This should properly "convert" difficulties just fine
        else map.Save();
        SelectedDifficultyData.noteJumpMovementSpeed = float.Parse(noteJumpSpeed.text);
        SelectedDifficultyData.noteJumpStartBeatOffset = float.Parse(startBeatOffset.text);
        if (difficultyLabel.text != "") SelectedDifficultyData.customData["_difficultyLabel"] = difficultyLabel.text;
        else SelectedDifficultyData.customData.Remove("_difficultyLabel");

        JSONArray requiredArray = new JSONArray();
        JSONArray suggestedArray = new JSONArray();
        if (WillChromaBeRequired.isOn && HasChromaEvents()) requiredArray.Add(new JSONString("Chroma Lighting Events"));
        else if (HasChromaEvents()) suggestedArray.Add(new JSONString("Chroma Lighting Events"));
        if (HasMappingExtensionsObjects()) requiredArray.Add(new JSONString("Mapping Extensions"));
        //if () requiredArray.Add(new JSONString("ChromaToggle")); //TODO: ChromaToggle

        if (suggestedArray.Linq.Any()) SelectedDifficultyData.customData["_suggestions"] = suggestedArray;
        if (requiredArray.Linq.Any()) SelectedDifficultyData.customData["_requirements"] = requiredArray;

        SelectedSet.difficultyBeatmaps = songDifficultyData;//todo this may need to have SelectedDifficultyData some where in here
        songDifficultySets.Add(SelectedSet);
        Song.difficultyBeatmapSets = songDifficultySets.Distinct().Where(x => x.difficultyBeatmaps.Any()).ToList();
        Song.SaveSong();
        InitializeDifficultyPanel(selectedDifficultyIndex);
    }

    private bool HasChromaEvents()
    {
        return !(map is null) && map._events.Any(mapEvent => mapEvent._value > ColourManager.RGB_INT_OFFSET);
    }
    
    private bool HasMappingExtensionsObjects()
    {
        if (SelectedDifficultyData == null) return false;
        return map != null && (map._notes.Any(note => note._lineIndex < 0 || note._lineIndex > 3) ||
                               map._obstacles.Any(ob =>
                                   ob._lineIndex < 0 || ob._lineIndex > 3 || ob._type >= 2 || ob._width >= 1000));
    }
}