using System.Collections;
using System.Collections.Generic;
using KaymakNetwork.Network;
using UnityEditor;
using UnityEngine;

internal static class NetworkConfig_Client
{
    internal static Client socket;

    internal static void InitNetwork()
    {
        if(!ReferenceEquals(socket, null)) return;
        socket = new Client(100);
        NetworkReceive_Client.PacketRouter();
    }

    internal static void ConnectToServer()
    {
        socket.Connect("localhost", 5555);
    }

    internal static void DisconnectFromServer()
    {
        socket.Dispose();
    }
}
