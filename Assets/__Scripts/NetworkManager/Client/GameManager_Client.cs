using System;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_Client : MonoBehaviour
{
    public Dictionary<int, GameObject> playerList = new Dictionary<int, GameObject>();

    public static GameManager_Client instance;

    private void Awake()
    {
        instance = this;
    }
}