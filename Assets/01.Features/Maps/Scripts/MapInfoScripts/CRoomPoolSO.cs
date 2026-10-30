using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RoomPool_", menuName = "SO/Dungeon/RoomPool")]
public class CRoomPoolSO : ScriptableObject
{
    #region 인스펙터

    [Header("시작 방")]
    [SerializeField] private CRoomData _startRoom;

    [Header("중간 방 목록")]
    [SerializeField] private CRoomData[] _middleRoomPrefabs;

    [Header("마지막 방 목록")]
    [SerializeField] private CRoomData[] _lastRoomPrefabs;


    [Header("랜덤 시작 방 목록")]
    [SerializeField] private CRoomData[] _startRoomPrefabs;

    [Header("일반 전투 방 목록")]
    [SerializeField] private CRoomData[] _combatRoomPrefabs;

    [Header("층 이동 방 목록")]
    [SerializeField] private CRoomData[] _exitRoomPrefabs;

    [Header("중간보스 방 목록")]
    [SerializeField] private CRoomData[] _miniBossRoomPrefabs;

    #endregion

    #region 프로퍼티

    public CRoomData StartRoom => _startRoom;
    public CRoomData[] MiddleRoomPrefabs => _middleRoomPrefabs;
    public CRoomData[] LastRoomPrefabs => _lastRoomPrefabs;

    public CRoomData[] StartRoomPrefabs => _startRoomPrefabs;
    public CRoomData[] CombatRoomPrefabs => _combatRoomPrefabs;
    public CRoomData[] ExitRoomPrefabs => _exitRoomPrefabs;

    public CRoomData[] MiniBossRoomPrefabs => _miniBossRoomPrefabs;
    #endregion
}