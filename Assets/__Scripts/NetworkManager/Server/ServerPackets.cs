public enum ServerPackets
{
    WELCOME_MSG = 1,
    Instantiate_Player = 2,
    PLAYER_MOVE = 3,
    PLAYER_ROTATE = 4,
    ACTION = 5,
    USER_KICK = 6,
    MAP_DATA = 7,
    MAP_DATA_REQUEST_TO_HOST = 8,
    PLAYER_LEAVE
}

public enum PlayerLeave_Reason
{
    LOST_CONNECTION = 0,
    KICK = 1
}