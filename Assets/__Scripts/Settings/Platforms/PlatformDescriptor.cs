﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlatformDescriptor : MonoBehaviour {

    [Header("Small Rings")]
    [Tooltip("Useful if you have no small rings (Eg. Big Mirror Environment)")]
    public bool UseSmallRings = true;
    public GameObject SmallRingObject;
    public int SmallRingsToSpawn = 64;
    public float SmallRingsExpandedDistance = 2f;
    public float SmallRingsZoomedDistance = 0.5f;
    public float SmallRingsTimeBetweenSpins = 0.1f;
    public float SmallRingsSpinSpeed = 0.5f;
    [Header("Big Rings")]
    [Tooltip("You have no big rings for your platform? Blasphemy!")]
    public bool UseBigRings = true;
    public GameObject BigRingObject;
    public int BigRingsToSpawn = 16;
    public float BigRingsDistance = 5;
    public float BigRingsTimeBetweenSpins = 0.25f;
    public float BigRingsSpinSpeed = 1;
    [Header("Lighting Groups")]
    [Tooltip("Manually map an Event ID (Index) to a group of lights (Parent GameObject)")]
    public GameObject[] LightingGroups = new GameObject[] { };
    public Color RedColor = Color.red;
    public Color BlueColor = new Color(0, 0.282353f, 1, 1);

    [HideInInspector] public int SmallRingsSpawned = 0;
    [HideInInspector] public int BigRingsSpawned = 0;

    private BeatmapObjectCallbackController callbackController;
    private LargeRing largeRing;
    private SmallRing smallRing;

    private Dictionary<GameObject, Color[]> ChromaCustomColors = new Dictionary<GameObject, Color[]>();
    private Dictionary<GameObject, LightingEvent[]> GroupToEvents = new Dictionary<GameObject, LightingEvent[]>();

    void Start()
    {
        if (UseBigRings) largeRing = BigRingObject.GetComponent<LargeRing>();
        if (UseSmallRings) smallRing = SmallRingObject.GetComponent<SmallRing>();
        if (SceneManager.GetActiveScene().name != "999_PrefabBuilding") StartCoroutine(FindEventCallback());
    }

    void OnDestroy()
    {
        callbackController.EventPassedThreshold -= EventPassed;
    }

    IEnumerator FindEventCallback()
    {
        yield return new WaitUntil(() => GameObject.Find("Vertical Grid Callback"));
        callbackController = GameObject.Find("Vertical Grid Callback").GetComponent<BeatmapObjectCallbackController>();
        callbackController.EventPassedThreshold += EventPassed;
        yield return new WaitForSeconds(0.1f); //Give time for back rings to spawn. This is way too much time to give LUL
        foreach (GameObject group in LightingGroups)
            if (group != null) GroupToEvents.Add(group, group.GetComponentsInChildren<LightingEvent>());
    }

    void EventPassed(bool initial, int index, BeatmapObject obj)
    {
        MapEvent e = obj as MapEvent;
        switch (e._type) { //FUN PART BOIS
            case 8:
                bool RotateRight = Random.Range((int)0, 2) == 1;
                if (UseBigRings)
                {
                    largeRing.RotationOffset = Random.Range(-5f, 5);
                    largeRing.StartCoroutine(largeRing.Rotate(RotateRight));
                }
                if (!UseSmallRings) break;
                smallRing.RotationOffset = Random.Range(-10f, 10);
                smallRing.Rotate(RotateRight, true, smallRing.distance == SmallRingsExpandedDistance);
                break;
            case 9:
                if (!UseSmallRings) break;
                bool expand = smallRing.distance == SmallRingsExpandedDistance;
                smallRing.distance = expand ? SmallRingsZoomedDistance : SmallRingsExpandedDistance;
                smallRing.Rotate(false, false, !expand);
                break;
            case 12:
                foreach (RotatingLights l in LightingGroups[2].GetComponentsInChildren<RotatingLights>())
                {
                    l.Offset = Random.Range(-10f, 10);
                    l.Speed = e._value;
                }
                break;
            case 13:
                foreach (RotatingLights r in LightingGroups[3].GetComponentsInChildren<RotatingLights>())
                {
                    r.Offset = Random.Range(-10f, 10);
                    r.Speed = e._value;
                }
                break;
            default:
                if (e._type <= LightingGroups.Length - 1 && LightingGroups[e._type] != null)
                    StartCoroutine(HandleLights(LightingGroups[e._type], e._value));
                break;
        }
    }

    IEnumerator HandleLights(GameObject group, int value)
    {
        if (group == null) yield break;
        Color color = Color.white;

        //Check if its a legacy Chroma RGB event
        if (value >= ColourManager.RGB_INT_OFFSET)
        {
            if (ChromaCustomColors.ContainsKey(group)) ChromaCustomColors[group] = new Color[] { ColourManager.ColourFromInt(value) };
            else ChromaCustomColors.Add(group, new Color[] { ColourManager.ColourFromInt(value) });
            yield break;
        }

        //Set initial light values
        if (value <= 3) color = BlueColor;
        else if (value <= 7 && value >= 5)
        {
            color = RedColor;
            value -= 5;
        }
        if (ChromaCustomColors.ContainsKey(group))
        {
            if (ChromaCustomColors[group].Length == 0) color = Random.ColorHSV(0, 1, 1, 1);
            else color = ChromaCustomColors[group][Random.Range(0, ChromaCustomColors[group].Length)];
        }

        switch (value) {
            case 0:
                foreach(LightingEvent e in GroupToEvents[group]) e.ChangeAlpha(0);
                break;
            case 1:
                foreach (LightingEvent e in GroupToEvents[group]) e.ChangeColor(color);
                break;
            case 2:
                foreach (LightingEvent e in GroupToEvents[group]) e.ChangeColor(color, LightingEvent.FlashTime);
                break;
            case 3:
                foreach (LightingEvent e in GroupToEvents[group]) e.StartCoroutine(e.Fade(color));
                break;
            case ColourManager.RGB_RESET:
                if (ChromaCustomColors.ContainsKey(group)) ChromaCustomColors.Remove(group);
                break;
            case ColourManager.RGB_ALT:
                if (ChromaCustomColors.ContainsKey(group)) ChromaCustomColors[group] = new Color[] { ColourManager.DefaultLightAltA, ColourManager.DefaultLightAltB };
                else ChromaCustomColors.Add(group, new Color[] { ColourManager.DefaultLightAltA, ColourManager.DefaultLightAltB });
                break;
            case ColourManager.RGB_WHITE:
                if (ChromaCustomColors.ContainsKey(group)) ChromaCustomColors[group] = new Color[] { Color.white };
                else ChromaCustomColors.Add(group, new Color[] { Color.white });
                break;
            case ColourManager.RGB_RANDOM:
                color = Random.ColorHSV(0, 1, 1, 1);
                if (ChromaCustomColors.ContainsKey(group)) ChromaCustomColors[group] = new Color[] { };
                else ChromaCustomColors.Add(group, new Color[] { });
                break;
        }
    }
}
