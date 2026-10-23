using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDungeonGenerator : MonoBehaviour
{
    #region 인스펙터

    [Header("방 정보")]
    [SerializeField] private CRoomData _firstRoom;
    [SerializeField] private CRoomData _secondRoom;

    [Header("방 디폴트 사이즈")]
    [SerializeField] private Vector2Int _defaultSize = new Vector2Int(26, 18);

    #endregion

    #region 내부 변수
    private CRoomPlacer _roomPlacer;


    #endregion


    private void Awake()
    {
        _roomPlacer = new CRoomPlacer(_defaultSize);
    }

    private void Start()
    {
        if (_firstRoom == null || _secondRoom == null)
        {
            return;
        }

        if(!_firstRoom.Down1 || !_secondRoom.Up1)
        {
            return;
        }

        Instantiate(_firstRoom, Vector3.zero, Quaternion.identity);

        Vector3 secondRoomPos = _roomPlacer.GetDownRoomPosition(Vector3.zero, _firstRoom, _secondRoom, 1, 1);

        Instantiate(_secondRoom, secondRoomPos, Quaternion.identity);





    }


}
