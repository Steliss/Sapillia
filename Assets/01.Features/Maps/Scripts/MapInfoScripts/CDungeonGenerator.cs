    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class CDungeonGenerator : MonoBehaviour
    {
	    #region 인스펙터

	    [Header("방 프리팹")]
	    [SerializeField] private CRoomData _firstRoom;
	    [SerializeField] private CRoomData _secondRoom;

        [Header("방 디폴트 규격")]
        [SerializeField] private Vector2Int _defaultSize = new Vector2Int(26, 18);

        #endregion


        private void Start()
        {

            if(!_firstRoom.Right2 || !_secondRoom.Left1)
            {
                return;
            }

            Instantiate(_firstRoom, Vector3.zero, Quaternion.identity);

            Vector3 secondRoomPos = GetRightRoomPosition(Vector3.zero, _firstRoom, _secondRoom, 2, 1);

            Instantiate(_secondRoom, secondRoomPos, Quaternion.identity);

        }



        private Vector3 GetRightRoomPosition(
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


    }
