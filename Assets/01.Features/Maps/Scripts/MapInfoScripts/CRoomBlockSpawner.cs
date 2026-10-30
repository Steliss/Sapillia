using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRoomBlockSpawner
{

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


    private Vector2Int GetPassageGridPosition(SRoomPlacement placement, ERoomDirection direction, int number)
    {
        Vector2Int pos = placement.GridPosition;
        Vector2Int size = placement.RoomPrefab.RoomSize;

        switch (direction)
        {
            case ERoomDirection.Up:
                return pos + new Vector2Int(number - 1, size.y - 1);

            case ERoomDirection.Down:
                return pos + new Vector2Int(number - 1, 0);

            case ERoomDirection.Left:
                return pos + new Vector2Int(0, size.y - number);

            case ERoomDirection.Right:
                return pos + new Vector2Int(size.x - 1, size.y - number);
        }

        return pos;
    }


    private bool HasPassageAtGrid(SRoomPlacement placement, ERoomDirection direction, Vector2Int gridPosition)
    {
        for (int number = 1; number <= 2; number++)
        {
            if (!HasPassage(placement.RoomPrefab, direction, number))
            {
                continue;
            }

            Vector2Int doorPosition =
                GetPassageGridPosition(placement, direction, number);

            if (doorPosition == gridPosition)
            {
                return true;
            }
        }

        return false;
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
                Debug.LogWarning("잘못된 출입구 방향");
                return direction;
        }
    }

    private bool IsPassageConnected(
    SRoomPlacement placement,
    ERoomDirection direction,
    int number,
    IReadOnlyList<SRoomPlacement> roomPlacements)
    {
        Vector2Int doorGrid =
            GetPassageGridPosition(placement, direction, number);

        Vector2Int offset = Vector2Int.zero;

        switch (direction)
        {
            case ERoomDirection.Up:
                offset = Vector2Int.up;
                break;

            case ERoomDirection.Down:
                offset = Vector2Int.down;
                break;

            case ERoomDirection.Left:
                offset = Vector2Int.left;
                break;

            case ERoomDirection.Right:
                offset = Vector2Int.right;
                break;
        }

        Vector2Int nextGrid = doorGrid + offset;

        ERoomDirection oppositeDirection =
            GetOppositeDirection(direction);

        foreach (SRoomPlacement other in roomPlacements)
        {
            if (other.GridPosition == placement.GridPosition)
            {
                continue;
            }

            Vector2Int otherMin = other.GridPosition;
            Vector2Int otherMax = otherMin + other.RoomPrefab.RoomSize;

            if (nextGrid.x < otherMin.x || nextGrid.x >= otherMax.x ||
                nextGrid.y < otherMin.y || nextGrid.y >= otherMax.y)
            {
                continue;
            }

            return HasPassageAtGrid(other, oppositeDirection, nextGrid);
        }

        return false;
    }


    private Vector3 GetBlockWorldPosition(SRoomPlacement placement, ERoomDirection direction, int number, Vector2Int defaultSize)
    {
        Vector2Int gridPos =
            GetPassageGridPosition(placement, direction, number);

        return new Vector3(
            gridPos.x * defaultSize.x,
            gridPos.y * defaultSize.y,
            placement.WorldPosition.z
        );
    }

    private GameObject GetBlockPrefab(
    ERoomDirection direction,
    GameObject blockUp,
    GameObject blockDown,
    GameObject blockLeft,
    GameObject blockRight)
    {
        switch (direction)
        {
            case ERoomDirection.Up:
                return blockUp;

            case ERoomDirection.Down:
                return blockDown;

            case ERoomDirection.Left:
                return blockLeft;

            case ERoomDirection.Right:
                return blockRight;

            default:
                return null;
        }
    }


    public void SpawnBlock(
    SRoomPlacement placement,
    ERoomDirection direction,
    int number,
    IReadOnlyList<SRoomPlacement> roomPlacements,
    Vector2Int defaultSize,
    GameObject blockUp,
    GameObject blockDown,
    GameObject blockLeft,
    GameObject blockRight,
    Transform generatedDungeonRoot)
    {
        if (!HasPassage(placement.RoomPrefab, direction, number))
        {
            return;
        }

        if (IsPassageConnected(placement, direction, number, roomPlacements))
        {
            return;
        }

        GameObject blockPrefab = GetBlockPrefab(
            direction,
            blockUp,
            blockDown,
            blockLeft,
            blockRight
        );

        if (blockPrefab == null)
        {
            return;
        }

        Vector3 blockPosition = GetBlockWorldPosition(
            placement,
            direction,
            number,
            defaultSize
        );

        Object.Instantiate(
            blockPrefab,
            blockPosition,
            Quaternion.identity,
            generatedDungeonRoot
        );
    }

}
