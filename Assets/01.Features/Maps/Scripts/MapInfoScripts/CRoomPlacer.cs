using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRoomPlacer 
{
    private Vector2Int _defaultSize;

    public CRoomPlacer(Vector2Int defaultSize)
    {
        _defaultSize = defaultSize;
    }


    
    // 오른쪽 방 좌표계산
    public Vector3 GetRightRoomPosition(
        Vector3 currentPos,
        CRoomData currentRoom,
        CRoomData nextRoom,
        int currentNumber,
        int nextNumber)
    {

        float x = currentPos.x + currentRoom.RoomSize.x * _defaultSize.x;

        float currentY = (currentRoom.RoomSize.y - currentNumber) * _defaultSize.y;
        float nextY = (nextRoom.RoomSize.y - nextNumber) * _defaultSize.y;


        float y = currentPos.y + currentY - nextY;

        return new Vector3(x, y, currentPos.z);

    }


    // 왼쪽 방 좌표계산
    public Vector3 GetLeftRoomPosition(
        Vector3 currentPos,
        CRoomData currentRoom,
        CRoomData nextRoom,
        int currentNumber,
        int nextNumber)
    {
        float x = currentPos.x - nextRoom.RoomSize.x * _defaultSize.x;

        float currentY = (currentRoom.RoomSize.y - currentNumber) * _defaultSize.y;
        float nextY = (nextRoom.RoomSize.y - nextNumber) * _defaultSize.y;

        float y = currentPos.y + currentY - nextY;

        return new Vector3(x, y, currentPos.z);

    }


    // 위쪽 방 좌표 계산
    public Vector3 GetUpRoomPosition(
        Vector3 currentPos,
        CRoomData currentRoom,
        CRoomData nextRoom,
        int currentNumber,
        int nextNumber)
    {
        float currentX = (currentNumber - 1) * _defaultSize.x;
        float nextX = (nextNumber - 1) * _defaultSize.x;

        float x = currentPos.x + currentX - nextX;

        float y = currentPos.y + currentRoom.RoomSize.y * _defaultSize.y;

        return new Vector3(x, y, currentPos.z);
    }


    // 아래 방 좌표 계산
    public Vector3 GetDownRoomPosition(
        Vector3 currentPos,
        CRoomData currentRoom,
        CRoomData nextRoom,
        int currentNumber,
        int nextNumber)
    {
        float currentX = (currentNumber - 1) * _defaultSize.x;
        float nextX = (nextNumber - 1) * _defaultSize.x;

        float x = currentPos.x + currentX - nextX;

        float y = currentPos.y - nextRoom.RoomSize.y * _defaultSize.y;

        return new Vector3(x, y, currentPos.z);


    }


    // 월드 좌표를 26과 18기준 1로 변경
    public Vector2Int WorldToGridPosition(Vector3 worldPos)
    {
        int x = Mathf.RoundToInt(worldPos.x / _defaultSize.x);
        int y = Mathf.RoundToInt(worldPos.y / _defaultSize.y);

        return new Vector2Int(x, y);
    }

}
