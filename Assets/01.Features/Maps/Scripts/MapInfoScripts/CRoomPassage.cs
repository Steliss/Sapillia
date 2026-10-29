using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ERoomDirection
{
    Up,
    Down,
    Left,
    Right
}


public struct CRoomPassage
{
    public CRoomData Room;
    public Vector3 RoomPosition;
    public ERoomDirection Direction;
    public int Number;


    public CRoomPassage(CRoomData room, Vector3 roomPosition, ERoomDirection direction, int number)
    {
        Room = room;
        RoomPosition = roomPosition;
        Direction = direction;
        Number = number;
    }

}
