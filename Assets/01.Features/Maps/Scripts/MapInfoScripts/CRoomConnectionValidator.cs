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



}