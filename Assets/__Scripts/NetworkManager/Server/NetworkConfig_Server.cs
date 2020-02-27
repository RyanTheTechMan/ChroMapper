using System;
using KaymakNetwork.Network;
using UnityEngine;

internal static class NetworkConfig_Server
{

    private static Server _socket;

    internal static Server Socket
    {
        get => _socket;
        set
        {
            if (_socket != null)
            {
                _socket.ConnectionReceived -= Socket_ConnectionReceived;
                _socket.ConnectionLost -= Socket_ConnectionLost;
            }

            _socket = value;
            if (_socket != null)
            {
                _socket.ConnectionReceived += Socket_ConnectionReceived;
                _socket.ConnectionLost += Socket_ConnectionLost;
            }
        }
    }

    internal static void InitNetwork()
    {
        if(Socket != null) return;
        
        Socket = new Server(100)
        {
            BufferLimit = 2048000,
            PacketAcceptLimit = 100,
            PacketDisconnectCount = 150
        };
        
        NetworkReceive_Server.PacketRouter();
    }
    
    internal static void Socket_ConnectionReceived(int connectionID)
    {
        NetworkManager_Server.Log("Connection received on index [" + connectionID + "]");
        NetworkSend_Server.SendMedioConnect(connectionID);
        NetworkSend_Server.WelcomeMsg(connectionID, "Welcome to the server!");
    }
    
    internal static void Socket_ConnectionLost(int connectionID)
    {
        NetworkManager_Server.Log("Connection lost on index [" + connectionID + "]");
        NetworkSend_Server.SendPlayerLeft(connectionID, PlayerLeave_Reason.LOST_CONNECTION);
    }
}