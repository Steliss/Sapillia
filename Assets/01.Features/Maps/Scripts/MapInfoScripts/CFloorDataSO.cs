using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "FloorData_", menuName = "SO/Dungeon/FloorData")]
public class CFloorDataSO : ScriptableObject
{
    #region 인스펙터

    [Header("층 정보")]
    [SerializeField] private int _floorNumber;

    [Header("방 개수")]
    [SerializeField] private int _minRoomCount;
    [SerializeField] private int _maxRoomCount;

    #endregion

    #region 프로퍼티

    public int FloorNumber => _floorNumber;
    public int MinRoomCount => _minRoomCount;
    public int MaxRoomCount => _maxRoomCount;

    #endregion
}
