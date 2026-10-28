using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct SRoomPlacement
{
    public CRoomData RoomPrefab;
    public Vector2Int GridPosition;
    public Vector3 WorldPosition;
    public int Depth;

    public SRoomPlacement(
        CRoomData roomPrefab,
        Vector2Int gridPosition,
        Vector3 worldPosition,
        int depth)
    {
        RoomPrefab = roomPrefab;
        GridPosition = gridPosition;
        WorldPosition = worldPosition;
        Depth = depth;
    }

}
