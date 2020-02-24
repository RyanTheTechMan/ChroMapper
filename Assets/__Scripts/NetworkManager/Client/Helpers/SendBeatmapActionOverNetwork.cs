using System;
using System.Collections.Generic;
using UnityEngine;

public class SendBeatmapActionOverNetwork : MonoBehaviour
{
    private void OnEnable()
    {
        BeatmapActionContainer.OnBeatmapAction += OnBeatmapAction;
    }

    private void OnDestroy()
    {
        BeatmapActionContainer.OnBeatmapAction -= OnBeatmapAction;
    }

    private void OnBeatmapAction(BeatmapObject beatmapObject, BeatmapActionType actionType)
    {
        NetworkSend_Client.SendBeatmapAction(beatmapObject.ConvertToJSON().ToString(), actionType, beatmapObject.beatmapType);
    }
}