using System.Collections.Generic;

static class GameManager_Server
{
    public static Dictionary<int, Player_Server> playerList = new Dictionary<int, Player_Server>();

    public static Player_Server host = null;

    public static bool ServerIsHost = false; //Does not work at the moment

    private static void JoinGame(int connectionID, Player_Server player)
    {
        NetworkSend_Server.InitNetworkPlayer(connectionID, player);
    }
    
    public static void CreatePlayer(int connectionID, string username, string avatar, string hostValidator)
    {
        Player_Server player = new Player_Server
        {
            connectionID = connectionID,
            inEditor = false,
            username = username,
            img = avatar,
            hostValidator = hostValidator
        };

        if (!ServerIsHost && connectionID == 1) //todo test to be sure initial connection is 1
        {
            host = player;
        } 
        
        playerList.Add(connectionID, player);
        NetworkManager_Server.Log("Player {0} (id {1}) has been added to the game.", username, connectionID);

        JoinGame(connectionID, player);
    }
}