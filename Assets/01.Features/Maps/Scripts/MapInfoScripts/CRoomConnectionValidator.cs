using System.Collections.Generic;
using UnityEngine;


public class CRoomConnectionValidator
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


    private bool IsRightAdjacent(SRoomPlacement a, SRoomPlacement b)
    {
        Vector2Int aPos = a.GridPosition;
        Vector2Int bPos = b.GridPosition;

        Vector2Int aSize = a.RoomPrefab.RoomSize;
        Vector2Int bSize = b.RoomPrefab.RoomSize;

        bool touching = aPos.x + aSize.x == bPos.x;

        bool overlappingY =
            aPos.y < bPos.y + bSize.y &&
            bPos.y < aPos.y + aSize.y;

        return touching && overlappingY;
    }

    private bool IsUpAdjacent(SRoomPlacement a, SRoomPlacement b)
    {
        Vector2Int aPos = a.GridPosition;
        Vector2Int bPos = b.GridPosition;

        Vector2Int aSize = a.RoomPrefab.RoomSize;
        Vector2Int bSize = b.RoomPrefab.RoomSize;

        bool touching = aPos.y + aSize.y == bPos.y;

        bool overlappingX =
            aPos.x < bPos.x + bSize.x &&
            bPos.x < aPos.x + aSize.x;

        return touching && overlappingX;
    }

    private bool ValidateRightBoundary(SRoomPlacement a, SRoomPlacement b)
    {
        int startY = Mathf.Max(
            a.GridPosition.y,
            b.GridPosition.y
        );

        int endY = Mathf.Min(
            a.GridPosition.y + a.RoomPrefab.RoomSize.y,
            b.GridPosition.y + b.RoomPrefab.RoomSize.y
        );

        for (int y = startY; y < endY; y++)
        {
            Vector2Int aGrid = new Vector2Int(
                a.GridPosition.x + a.RoomPrefab.RoomSize.x - 1,
                y
            );

            Vector2Int bGrid = aGrid + Vector2Int.right;

            bool aHasDoor = HasPassageAtGrid(a, ERoomDirection.Right, aGrid);
            bool bHasDoor = HasPassageAtGrid(b, ERoomDirection.Left, bGrid);

            if (aHasDoor != bHasDoor)
            {
                return false;
            }
        }

        return true;
    }

    private bool ValidateUpBoundary(SRoomPlacement a, SRoomPlacement b)
    {
        int startX = Mathf.Max(
            a.GridPosition.x,
            b.GridPosition.x
        );

        int endX = Mathf.Min(
            a.GridPosition.x + a.RoomPrefab.RoomSize.x,
            b.GridPosition.x + b.RoomPrefab.RoomSize.x
        );

        for (int x = startX; x < endX; x++)
        {
            Vector2Int aGrid = new Vector2Int(
                x,
                a.GridPosition.y + a.RoomPrefab.RoomSize.y - 1
            );

            Vector2Int bGrid = aGrid + Vector2Int.up;

            bool aHasDoor = HasPassageAtGrid(a, ERoomDirection.Up, aGrid);
            bool bHasDoor = HasPassageAtGrid(b, ERoomDirection.Down, bGrid);

            if (aHasDoor != bHasDoor)
            {
                return false;
            }
        }

        return true;
    }


    public bool ValidateRoomConnections(
    IReadOnlyList<SRoomPlacement> roomPlacements)
    {
        for (int i = 0; i < roomPlacements.Count; i++)
        {
            SRoomPlacement a = roomPlacements[i];

            for (int j = i + 1; j < roomPlacements.Count; j++)
            {
                SRoomPlacement b = roomPlacements[j];

                if (IsRightAdjacent(a, b))
                {
                    if (!ValidateRightBoundary(a, b))
                    {
                        Debug.LogWarning(
                            $"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 오른쪽 경계 출입구 불일치");
                        return false;
                    }
                }

                if (IsRightAdjacent(b, a))
                {
                    if (!ValidateRightBoundary(b, a))
                    {
                        Debug.LogWarning(
                            $"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 왼쪽 경계 출입구 불일치");
                        return false;
                    }
                }

                if (IsUpAdjacent(a, b))
                {
                    if (!ValidateUpBoundary(a, b))
                    {
                        Debug.LogWarning(
                            $"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 위쪽 경계 출입구 불일치");
                        return false;
                    }
                }

                if (IsUpAdjacent(b, a))
                {
                    if (!ValidateUpBoundary(b, a))
                    {
                        Debug.LogWarning(
                            $"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 아래쪽 경계 출입구 불일치");
                        return false;
                    }
                }
            }
        }

        return true;
    }
}