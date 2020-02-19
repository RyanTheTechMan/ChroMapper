using System.Collections.Generic;

static class GameManager_Server
{
    public static Dictionary<int, Player_Server> playerList = new Dictionary<int, Player_Server>();
    
    public static byte[][] MapData_Info;
    public static byte[][] MapData_Difficulty;
    public static byte[][] MapData_Song;
    
    public static void JoinGame(int connectionID, Player_Server player)
    {
        NetworkSend_Server.InitNetworkPlayer(connectionID, player);
    }
    
    public static void CreatePlayer(int connectionID, string username, string avatar)
    {
        Player_Server player = new Player_Server
        {
            connectionID = connectionID,
            inGame = true,
            username = username,
            img = avatar
        };
        
        playerList.Add(connectionID, player);
        NetworkManager_Server.Log("Player {0} (id {1}) has been added to the game.", username, connectionID);
        JoinGame(connectionID, player);
    }
}