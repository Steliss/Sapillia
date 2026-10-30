using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CFixedDungeonLayout
{
    #region 내부 변수

    private List<SRoomPlacement> _roomPlacements = new List<SRoomPlacement>();

    private CRoomPlacer _roomPlacer;
    private CRoomGrid _roomGrid;

    private readonly ERoomDirection[] _directions =
        {
        ERoomDirection.Up,
        ERoomDirection.Down,
        ERoomDirection.Left,
        ERoomDirection.Right
        };
    #endregion

    #region 프로퍼티

    public IReadOnlyList<SRoomPlacement> RoomPlacements => _roomPlacements;

    #endregion

    public CFixedDungeonLayout(CRoomPlacer roomPlacer)
    {
        _roomPlacer = roomPlacer;
        _roomGrid = new CRoomGrid();
    }

    public bool GenerateLayout(CRoomData startRoom, CRoomData middleRoom, CRoomData exitRoom)
    {
        ResetLayout();

        Vector2Int startGridPos = Vector2Int.zero;
        Vector3 startWorldPos = Vector3.zero;

        if (!_roomGrid.RoomAvailable(startGridPos, startRoom))
        {
            Debug.LogWarning("시작 방을 배치할 수 없습니다.");
            return false;
        }

        _roomGrid.RegisterRoom(startGridPos, startRoom);

        _roomPlacements.Add(new SRoomPlacement(startRoom, startGridPos, startWorldPos, 0));

        if (!TryFindRoomPosition(startRoom, startWorldPos, middleRoom, out Vector3 middleWorldPos))
        {
            Debug.LogWarning("중간 방을 배치할 수 없습니다.");
            ResetLayout();
            return false;
        }

        Vector2Int middleGridPos = _roomPlacer.WorldToGridPosition(middleWorldPos);

        _roomGrid.RegisterRoom(middleGridPos, middleRoom);

        _roomPlacements.Add(new SRoomPlacement(middleRoom, middleGridPos, middleWorldPos, 1));


        if (!TryFindRoomPosition(middleRoom, middleWorldPos, exitRoom, out Vector3 exitWorldPos))
        {
            Debug.LogWarning("Exit 방을 배치할 수 없습니다.");
            ResetLayout();
            return false;
        }

        Vector2Int exitGridPos = _roomPlacer.WorldToGridPosition(exitWorldPos);

        _roomGrid.RegisterRoom(exitGridPos, exitRoom);

        _roomPlacements.Add(new SRoomPlacement(exitRoom, exitGridPos, exitWorldPos, 2));

        if (_roomPlacements.Count != 3)
        {
            Debug.LogWarning("고정 던전의 방 개수가 올바르지 않습니다.");
            ResetLayout();
            return false;
        }

        return true;
    }


    private bool HasPassage(CRoomData room, ERoomDirection direction, int number)
    {
        switch (direction)
        {
            case ERoomDirection.Up:
                return number == 1 ? room.Up1 : number == 2 && room.Up2;

            case ERoomDirection.Down:
                return number == 1 ? room.Down1 : number == 2 && room.Down2;

            case ERoomDirection.Left:
                return number == 1 ? room.Left1 : number == 2 && room.Left2;

            case ERoomDirection.Right:
                return number == 1 ? room.Right1 : number == 2 && room.Right2;

            default:
                return false;
        }
    }


    private ERoomDirection GetOppositeDirection(ERoomDirection direction)
    {
        switch (direction)
        {
            case ERoomDirection.Up:
                return ERoomDirection.Down;

            case ERoomDirection.Down:
                return ERoomDirection.Up;

            case ERoomDirection.Left:
                return ERoomDirection.Right;

            case ERoomDirection.Right:
                return ERoomDirection.Left;

            default:
                return direction;
        }
    }


    private Vector3 GetNextRoomPosition(CRoomPassage passage, CRoomData nextRoom, int nextNumber)
    {
        switch (passage.Direction)
        {
            case ERoomDirection.Up:
                return _roomPlacer.GetUpRoomPosition(
                    passage.RoomPosition, passage.Room, nextRoom,
                    passage.Number, nextNumber);

            case ERoomDirection.Down:
                return _roomPlacer.GetDownRoomPosition(
                    passage.RoomPosition, passage.Room, nextRoom,
                    passage.Number, nextNumber);

            case ERoomDirection.Left:
                return _roomPlacer.GetLeftRoomPosition(
                    passage.RoomPosition, passage.Room, nextRoom,
                    passage.Number, nextNumber);

            case ERoomDirection.Right:
                return _roomPlacer.GetRightRoomPosition(
                    passage.RoomPosition, passage.Room, nextRoom,
                    passage.Number, nextNumber);

            default:
                return Vector3.zero;
        }
    }


    private bool TryFindRoomPosition(
    CRoomData currentRoom,
    Vector3 currentPosition,
    CRoomData nextRoom,
    out Vector3 nextPosition)
    {
        foreach (ERoomDirection direction in _directions)
        {
            ERoomDirection oppositeDirection = GetOppositeDirection(direction);

            for (int currentNumber = 1; currentNumber <= 2; currentNumber++)
            {
                if (!HasPassage(currentRoom, direction, currentNumber))
                {
                    continue;
                }

                CRoomPassage passage = new CRoomPassage(
                    currentRoom,
                    currentPosition,
                    direction,
                    currentNumber
                );

                for (int nextNumber = 1; nextNumber <= 2; nextNumber++)
                {
                    if (!HasPassage(nextRoom, oppositeDirection, nextNumber))
                    {
                        continue;
                    }

                    Vector3 position = GetNextRoomPosition(
                        passage,
                        nextRoom,
                        nextNumber
                    );

                    Vector2Int gridPosition =
                        _roomPlacer.WorldToGridPosition(position);

                    if (!_roomGrid.RoomAvailable(gridPosition, nextRoom))
                    {
                        continue;
                    }

                    nextPosition = position;
                    return true;
                }
            }
        }

        nextPosition = Vector3.zero;
        return false;
    }


    private void ResetLayout()
    {
        _roomPlacements.Clear();
        _roomGrid = new CRoomGrid();
    }
}