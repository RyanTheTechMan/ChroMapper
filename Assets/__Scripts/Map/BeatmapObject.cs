﻿using SimpleJSON;
using System;

public abstract class BeatmapObject{

    public enum Type {
        NOTE,
        EVENT,
        OBSTACLE,
        CUSTOM_NOTE,
        CUSTOM_EVENT,
        BPM_CHANGE,
    }

    private float time;

    //To prevent floating point precision errors, round shit to the nearest thousandth.
    public virtual float _time { get => time; set => time = (float)Math.Round(value, 3); }
    public abstract Type beatmapType { get; set; }
    public virtual JSONNode _customData { get; set; }

    public abstract JSONNode ConvertToJSON();

    public static T GenerateCopy<T>(T originalData) where T : BeatmapObject
    {
        T objectData = Activator.CreateInstance(originalData.GetType(), new object[] { originalData.ConvertToJSON() }) as T;
        //The JSONObject somehow stays behind even after this, so we're going to have to parse a new one from the original
        if (originalData._customData != null) objectData._customData = JSON.Parse(originalData._customData.ToString());
        return objectData;
    }
}
