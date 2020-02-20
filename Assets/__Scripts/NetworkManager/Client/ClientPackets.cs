public enum ClientPackets
{ 
    PING = 1,
    UPDATE_LOCATION = 2,
    UPDATE_ROTATION = 3,
    ACTION = 4,
    MAP_DATA = 5,
    MAP_DATA_REQUEST = 6
}

public enum NetworkMapData_Type
{
    NONE = 0,
    DIFFICULTY = 1,
    SONG = 2,
    INFO = 3
}