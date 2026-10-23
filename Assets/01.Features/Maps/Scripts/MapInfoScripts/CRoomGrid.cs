using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRoomGrid
{
    private Dictionary<Vector2Int, CRoomData> _rooms;

    public CRoomGrid()
    {
        _rooms = new Dictionary<Vector2Int, CRoomData>();
    }


    public bool RoomAvailable(Vector2Int position, CRoomData room)
    {
        for (int x = 0; x < room.RoomSize.x; x++)
        {
            for(int y = 0; y < room.RoomSize.y; y++)
            {
                Vector2Int checkPos = position + new Vector2Int(x, y);

                if(_rooms.ContainsKey(checkPos))
                {
                    return false;
                }
            }
        }

        return true;
    }

}
