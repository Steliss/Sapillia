        using System.Collections.Generic;
        using UnityEngine;

public class CDungeonGenerator : MonoBehaviour
{
    #region 인스펙터

    [Header("층 데이터")]
    [SerializeField] private CFloorDataSO[] _floorDatas;

    [Header("방 디폴트 사이즈")]
    [SerializeField] private Vector2Int _defaultSize = new Vector2Int(26, 18);

    [Header("출입구 막기")]
    [SerializeField] private GameObject _blockUp;
    [SerializeField] private GameObject _blockDown;
    [SerializeField] private GameObject _blockLeft;
    [SerializeField] private GameObject _blockRight;

    [Header("현재 층")]
    [SerializeField, Range(1, 5)]
    private int _currentFloor = 1;

    #endregion

    #region 내부 변수
    private CRoomPlacer _roomPlacer;
    private CRoomGrid _roomGrid;
    private List<CRoomPassage> _availablePassages = new List<CRoomPassage>();
    private Dictionary<Vector2Int, int> _roomDepths = new Dictionary<Vector2Int, int>();

    private List<SRoomPlacement> _roomPlacements = new List<SRoomPlacement>();

    private Transform _generatedDungeonRoot;

    private HashSet<CRoomData> _usedRoomPrefabs = new HashSet<CRoomData>();

    #endregion


    private void Awake()
    {
        _roomPlacer = new CRoomPlacer(_defaultSize);
        _roomGrid = new CRoomGrid();

        GameObject generatedDungeon = new GameObject("GeneratedDungeon");
        _generatedDungeonRoot = generatedDungeon.transform;

        _generatedDungeonRoot.SetParent(transform);
    }

    private void Start()
    {
        GenerateNewDungeon();

    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            GenerateNewDungeon();
        }
    }


    private void GenerateNewDungeon()
    {        
        CFloorDataSO floorData = GetCurrentFloorData();

        if (floorData == null)
        {
            Debug.LogError($"현재 층({_currentFloor})의 데이터가 없습니다.");
            return;
        }

        int targetRoomCount = Random.Range(
            floorData.MinRoomCount,
            floorData.MaxRoomCount + 1
        );

        ClearGeneratedDungeon();

        int maxAttempts = 50;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            ResetDungeonData();

            bool success;

            if (_currentFloor == 1)
            {
                success = GenerateFirstFloor(floorData);
            }
            else if (_currentFloor == 5)
            {
                success = GenerateBossFloor(floorData);
            }
            else
            {
                CRoomData startRoom = GetRandomRoomFromPool(floorData.RoomPool?.StartRoomPrefabs);

                if (startRoom == null)
                {
                    Debug.LogError("시작 방 프리팹이 없습니다.");
                    return;
                }

                success = GenerateDungeon(targetRoomCount, startRoom);
            }

            if (success)
            {
                Debug.Log($"던전 생성 성공! 시도 횟수: {attempt + 1}");
                SpawnRooms();
                return;
            }
        }

        Debug.LogError("던전 생성 실패: 최대 시도 횟수 초과");
    }

    private CFloorDataSO GetCurrentFloorData()
    {
        if (_floorDatas == null)
        {
            return null;
        }

        foreach (CFloorDataSO floorData in _floorDatas)
        {
            if (floorData == null)
            {
                continue;
            }

            if (floorData.FloorNumber == _currentFloor)
            {
                return floorData;
            }
        }

        return null;
    }


    private void ClearGeneratedDungeon()
    {
        for (int i = _generatedDungeonRoot.childCount - 1; i >= 0; i--)
        {
            Destroy(_generatedDungeonRoot.GetChild(i).gameObject);
        }
    }



    private void AddRoomPassages(CRoomData room, Vector3 roomPos)
    {
        if (room.Up1)
            _availablePassages.Add(new CRoomPassage(room, roomPos, ERoomDirection.Up, 1));

        if (room.Up2)
            _availablePassages.Add(new CRoomPassage(room, roomPos, ERoomDirection.Up, 2));

        if (room.Down1)
            _availablePassages.Add(new CRoomPassage(room, roomPos, ERoomDirection.Down, 1));

        if (room.Down2)
            _availablePassages.Add(new CRoomPassage(room, roomPos, ERoomDirection.Down, 2));

        if (room.Left1)
            _availablePassages.Add(new CRoomPassage(room, roomPos, ERoomDirection.Left, 1));

        if (room.Left2)
            _availablePassages.Add(new CRoomPassage(room, roomPos, ERoomDirection.Left, 2));

        if (room.Right1)
            _availablePassages.Add(new CRoomPassage(room, roomPos, ERoomDirection.Right, 1));

        if (room.Right2)
            _availablePassages.Add(new CRoomPassage(room, roomPos, ERoomDirection.Right, 2));
    }



    private void RemoveRoomPassage(CRoomData room, Vector3 roomPos, ERoomDirection direction, int number)
    {
        for (int i = _availablePassages.Count - 1; i >= 0; i--)
        {
            CRoomPassage passage = _availablePassages[i];

            if (passage.Room == room &&
                passage.RoomPosition == roomPos &&
                passage.Direction == direction &&
                passage.Number == number)
            {
                _availablePassages.RemoveAt(i);
                return;
            }

        }
    }



    private Vector3 GetNextRoomPosition(CRoomPassage passage, CRoomData nextRoom, int nextNumber)
    {
        switch (passage.Direction)
        {
            case ERoomDirection.Up:
                return _roomPlacer.GetUpRoomPosition(passage.RoomPosition, passage.Room, nextRoom, passage.Number, nextNumber);
            case ERoomDirection.Down:
                return _roomPlacer.GetDownRoomPosition(passage.RoomPosition, passage.Room, nextRoom, passage.Number, nextNumber);
            case ERoomDirection.Left:
                return _roomPlacer.GetLeftRoomPosition(passage.RoomPosition, passage.Room, nextRoom, passage.Number, nextNumber);
            case ERoomDirection.Right:
                return _roomPlacer.GetRightRoomPosition(passage.RoomPosition, passage.Room, nextRoom, passage.Number, nextNumber);
            default:
                return Vector3.zero;
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
                Debug.LogWarning("잘못된 출입구 방향");
                return direction;
        }
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



    private bool TryFindConnectablePassage(
        CRoomData nextRoom,
        out CRoomPassage selectedPassage,
        out int selectedNumber,
        out Vector3 selectedPosition)

    {
        List<CRoomPassage> candidates = new List<CRoomPassage>(_availablePassages);


        while (candidates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            CRoomPassage passage = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);

            if (nextRoom.RoomType == ERoomType.Exit)
            {
                Vector2Int parentGridPos =
                    _roomPlacer.WorldToGridPosition(passage.RoomPosition);

                if (_roomDepths[parentGridPos] < 1)
                {
                    continue;
                }
            }

            ERoomDirection oppositeDirection = GetOppositeDirection(passage.Direction);

            for (int number = 1; number <= 2; number++)
            {
                if (!HasPassage(nextRoom, oppositeDirection, number))
                {
                    continue;
                }

                Vector3 nextRoomPos = GetNextRoomPosition(passage, nextRoom, number);

                Vector2Int nextGridPos = _roomPlacer.WorldToGridPosition(nextRoomPos);

                if (!_roomGrid.RoomAvailable(nextGridPos, nextRoom))
                {
                    continue;
                }

                selectedPassage = passage;
                selectedNumber = number;
                selectedPosition = nextRoomPos;

                return true;
            }
        }

        selectedPassage = default;
        selectedNumber = 0;
        selectedPosition = Vector3.zero;

        return false;
    }



    private bool TryAddRoom(CRoomData nextRoom)
    {
        if (!TryFindConnectablePassage(
            nextRoom,
            out CRoomPassage selectedPassage,
            out int selectedNumber,
            out Vector3 selectedPosition))
        {
            return false;
        }


        Vector2Int gridPos = _roomPlacer.WorldToGridPosition(selectedPosition);

        _roomGrid.RegisterRoom(gridPos, nextRoom);

        Vector2Int parentGridPos = _roomPlacer.WorldToGridPosition(selectedPassage.RoomPosition);

        int parentDepth = _roomDepths[parentGridPos];

        _roomDepths.Add(gridPos, parentDepth + 1);

        _roomPlacements.Add(new SRoomPlacement(nextRoom, gridPos, selectedPosition, parentDepth + 1));

        Debug.Log($"방 생성 위치 : {gridPos}, 깊이 : {parentDepth + 1}");

        AddRoomPassages(nextRoom, selectedPosition);

        RemoveRoomPassage(
            selectedPassage.Room,
            selectedPassage.RoomPosition,
            selectedPassage.Direction,
            selectedPassage.Number
        );

        ERoomDirection oppositeDirection =
            GetOppositeDirection(selectedPassage.Direction);

        RemoveRoomPassage(
            nextRoom,
            selectedPosition,
            oppositeDirection,
            selectedNumber
        );

        return true;
    }



    private CRoomData GetRandomRoomFromPool(CRoomData[] roomPrefabs)
    {
        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            return null;
        }

        int randomIndex = Random.Range(0, roomPrefabs.Length);

        return roomPrefabs[randomIndex];
    }

    private bool GenerateFirstFloor(CFloorDataSO floorData)
    {
        if (floorData == null || floorData.RoomPool == null)
        {
            Debug.LogError("1층 방 프리팹 데이터가 없습니다.");
            return false;
        }

        CRoomPoolSO roomPool = floorData.RoomPool;

        CRoomData startRoom = roomPool.StartRoom;

        CRoomData middleRoom =
            GetRandomRoomFromPool(roomPool.MiddleRoomPrefabs);

        CRoomData lastRoom =
            GetRandomRoomFromPool(roomPool.LastRoomPrefabs);

        if (startRoom == null || middleRoom == null || lastRoom == null)
        {
            Debug.LogError("1층 방 프리팹이 정상적으로 등록되지 않았습니다.");
            return false;
        }

        CFixedDungeonLayout fixedLayout = new CFixedDungeonLayout(_roomPlacer);

        if (!fixedLayout.GenerateLayout(startRoom, middleRoom, lastRoom))
        {
            Debug.LogWarning("1층 방 배치 실패");
            return false;
        }

        _roomPlacements.Clear();
        _roomPlacements.AddRange(fixedLayout.RoomPlacements);

        return ValidateRoomConnections();
    }

    private bool GenerateBossFloor(CFloorDataSO floorData)
    {
        if (floorData == null || floorData.RoomPool == null)
        {
            Debug.LogError("5층 방 데이터가 없습니다.");
            return false;
        }

        CRoomData bossRoom = floorData.RoomPool.BossRoom;
        CRoomData extraRoom = floorData.RoomPool.BossExtraRoom;

        if (bossRoom == null || extraRoom == null)
        {
            Debug.LogError("5층 보스 방 또는 추가 방이 없습니다.");
            return false;
        }

        // 보스 방과 추가 방의 크기 검사
        if (bossRoom.RoomSize != new Vector2Int(2, 2) ||
            extraRoom.RoomSize != new Vector2Int(1, 1))
        {
            Debug.LogError("5층 방 크기가 올바르지 않습니다.");
            return false;
        }

        // 출입구 검사
        if (!bossRoom.Right2 || !extraRoom.Left1)
        {
            Debug.LogError("5층 방 출입구 설정이 올바르지 않습니다.");
            return false;
        }

        // 1. 보스 방을 원점에 배치
        Vector3 bossPosition = Vector3.zero;
        Vector2Int bossGridPos = Vector2Int.zero;

        if (!_roomGrid.RoomAvailable(bossGridPos, bossRoom))
        {
            return false;
        }

        _roomGrid.RegisterRoom(bossGridPos, bossRoom);

        _roomPlacements.Add(
            new SRoomPlacement(bossRoom, bossGridPos, bossPosition, 0)
        );

        Vector3 extraPosition = _roomPlacer.GetRightRoomPosition(
            bossPosition,
            bossRoom,
            extraRoom,
            2,
            1
        );

        Vector2Int extraGridPos = _roomPlacer.WorldToGridPosition(extraPosition);

        if (!_roomGrid.RoomAvailable(extraGridPos, extraRoom))
        {
            Debug.LogWarning("5층 추가 방 배치 실패");
            return false;
        }

        _roomGrid.RegisterRoom(extraGridPos, extraRoom);

        _roomPlacements.Add(new SRoomPlacement(extraRoom, extraGridPos, extraPosition, 1));

        return ValidateRoomConnections();
    }


    private bool TryAddRandomRoom()
    {
        CFloorDataSO floorData = GetCurrentFloorData();

        CRoomData[] roomPrefabs = floorData?.RoomPool?.CombatRoomPrefabs;

        if (roomPrefabs == null || roomPrefabs.Length == 0)
        {
            return false;
        }

        List<CRoomData> candidates = new List<CRoomData>(roomPrefabs);

        while (candidates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            CRoomData randomRoom = candidates[randomIndex];

            candidates.RemoveAt(randomIndex);

            if (randomRoom == null)
            {
                continue;
            }
            if (randomRoom.RoomType != ERoomType.Combat)
            {
                continue;
            }

            // 이미 사용한 방이라면 건너뛰기
            if (_usedRoomPrefabs.Contains(randomRoom))
            {
                continue;
            }

            if (TryAddRoom(randomRoom))
            {
                // 배치에 성공한 경우에만 사용 기록
                _usedRoomPrefabs.Add(randomRoom);

                Debug.Log($"랜덤 방 생성 성공: {randomRoom.name}");
                return true;
            }
        }

        return false;
    }

    private bool TryAddMiniBossRoom()
    {
        CFloorDataSO floorData = GetCurrentFloorData();

        CRoomData[] miniBossPrefabs = floorData?.RoomPool?.MiniBossRoomPrefabs;

        if (miniBossPrefabs == null || miniBossPrefabs.Length == 0)
        {
            Debug.LogWarning("중간보스 방 프리팹이 없습니다.");
            return false;
        }

        List<CRoomData> candidates = new List<CRoomData>(miniBossPrefabs);

        while (candidates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            CRoomData miniBossRoom = candidates[randomIndex];
            candidates.RemoveAt(randomIndex);

            if (miniBossRoom == null || miniBossRoom.RoomType != ERoomType.Miniboss)
            {
                continue;
            }

            if (TryAddRoom(miniBossRoom))
            {
                Debug.Log($"중간보스 방 생성 성공: {miniBossRoom.name}");
                return true;
            }
        }

        return false;
    }




    private bool TryAddExitRoom()
    {
        CFloorDataSO floorData = GetCurrentFloorData();

        CRoomData[] exitRoomPrefabs = floorData?.RoomPool?.ExitRoomPrefabs;

        if (exitRoomPrefabs == null || exitRoomPrefabs.Length == 0)
        {
            return false;
        }

        List<CRoomData> candidates = new List<CRoomData>(exitRoomPrefabs);

        while (candidates.Count > 0)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            CRoomData exitRoom = candidates[randomIndex];

            candidates.RemoveAt(randomIndex);

            if (exitRoom == null)
            {
                continue;
            }

            if (exitRoom.RoomType != ERoomType.Exit)
            {
                continue;
            }

            if (TryAddRoom(exitRoom))
            {
                Debug.Log($"층 이동 방 생성 성공: {exitRoom.name}");
                return true;
            }
        }

        return false;
    }


    private bool CanPlaceExitRoom()
    {
        CFloorDataSO floorData = GetCurrentFloorData();

        CRoomData[] exitRoomPrefabs = floorData?.RoomPool?.ExitRoomPrefabs;

        if (exitRoomPrefabs == null || exitRoomPrefabs.Length == 0)
        {
            return false;
        }

        foreach (CRoomData exitRoom in exitRoomPrefabs)
        {
            if (exitRoom == null || exitRoom.RoomType != ERoomType.Exit)
            {
                continue;
            }

            if (TryFindConnectablePassage(exitRoom, out CRoomPassage selectedPassage, out int selectedNumber, out Vector3 selectedPosition))
            {
                return true;
            }
        }

        return false;
    }



    private void SpawnRooms()
    {
        foreach (SRoomPlacement placement in _roomPlacements)
        {
            Instantiate(
                placement.RoomPrefab,
                placement.WorldPosition,
                Quaternion.identity,
                _generatedDungeonRoot
            );

            for (int number = 1; number <= 2; number++)
            {
                SpawnBlock(placement, ERoomDirection.Up, number);
                SpawnBlock(placement, ERoomDirection.Down, number);
                SpawnBlock(placement, ERoomDirection.Left, number);
                SpawnBlock(placement, ERoomDirection.Right, number);
            }
        }
    }


    private bool ValidateDungeon(int targetRoomCount)
    {
        if (_roomPlacements.Count != targetRoomCount)
        {
            return false;
        }

        foreach (SRoomPlacement placement in _roomPlacements)
        {
            if (placement.RoomPrefab.RoomType == ERoomType.Exit)
            {
                return ValidateRoomConnections();
            }
        }

        return false;
    }


    private void ResetDungeonData()
    {
        _roomGrid = new CRoomGrid();

        _availablePassages.Clear();
        _roomDepths.Clear();
        _roomPlacements.Clear();

        _usedRoomPrefabs.Clear();
    }

    private bool GenerateDungeon(int targetRoomCount, CRoomData startRoom)
    {
        Vector2Int firstGridPos = Vector2Int.zero;

        if (!_roomGrid.RoomAvailable(firstGridPos, startRoom))
        {
            return false;
        }

        _roomPlacements.Add(new SRoomPlacement(startRoom, firstGridPos, Vector3.zero, 0));

        _roomGrid.RegisterRoom(firstGridPos, startRoom);
        AddRoomPassages(startRoom, Vector3.zero);
        _roomDepths.Add(firstGridPos, 0);

        int combatTargetCount = targetRoomCount - 1;
        int currentRoomCount = 1;

        Debug.Log($"목표 방 개수{targetRoomCount}");

        while (currentRoomCount < combatTargetCount)
        {
            if (TryAddRandomRoom())
            {
                currentRoomCount++;
                Debug.Log($"현재 방 개수: {currentRoomCount}");

                if (!CanPlaceExitRoom())
                {
                    Debug.LogWarning("Exit 방을 배치할 공간이 없습니다.");
                    return false;
                }
            }
            else
            {
                Debug.Log("더 이상 방을 배치할 수 없습니다.");
                break;
            }
        }

        if (_currentFloor == 4)
        {
            if (!TryAddMiniBossRoom())
            {
                Debug.LogWarning("4층 중간보스 방 배치 실패");
                return false;
            }
        }

        if (TryAddExitRoom())
        {
            Debug.Log("층 이동 방 배치 완료");
        }
        else
        {
            Debug.LogWarning("층 이동 방 배치 실패");
        }

        int finalRoomCount = targetRoomCount + (_currentFloor == 4 ? 1 : 0);

        return ValidateDungeon(finalRoomCount);
    }


    private bool ValidateRoomConnections()
    {
        for (int i = 0; i < _roomPlacements.Count; i++)
        {
            SRoomPlacement a = _roomPlacements[i];

            for (int j = i + 1; j < _roomPlacements.Count; j++)
            {
                SRoomPlacement b = _roomPlacements[j];

                if (IsRightAdjacent(a, b))
                {
                    if (ValidateRightBoundary(a, b))
                    {
                        Debug.Log($"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 오른쪽 경계 정상");
                    }
                    else
                    {
                        Debug.LogWarning($"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 오른쪽 경계 출입구 불일치");
                        return false;
                    }
                }

                if (IsRightAdjacent(b, a))
                {
                    if (ValidateRightBoundary(b, a))
                    {
                        Debug.Log($"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 왼쪽 경계 정상");
                    }
                    else
                    {
                        Debug.LogWarning($"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 왼쪽 경계 출입구 불일치");
                        return false;
                    }
                }

                if (IsUpAdjacent(a, b))
                {
                    if (ValidateUpBoundary(a, b))
                    {
                        Debug.Log($"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 위쪽 경계 정상");
                    }
                    else
                    {
                        Debug.LogWarning($"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 위쪽 경계 출입구 불일치");
                        return false;
                    }
                }

                if (IsUpAdjacent(b, a))
                {
                    if (ValidateUpBoundary(b, a))
                    {
                        Debug.Log($"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 아래쪽 경계 정상");
                    }
                    else
                    {
                        Debug.LogWarning($"{a.RoomPrefab.name} ↔ {b.RoomPrefab.name} 아래쪽 경계 출입구 불일치");
                        return false;
                    }
                }
            }
        }

        return true;

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


    private Vector2Int GetPassageGridPosition(
    SRoomPlacement placement,
    ERoomDirection direction,
    int number)
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

    private bool IsPassageConnected(
    SRoomPlacement placement,
    ERoomDirection direction,
    int number)
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

        foreach (SRoomPlacement other in _roomPlacements)
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


    private GameObject GetBlockPrefab(ERoomDirection direction)
    {
        switch (direction)
        {
            case ERoomDirection.Up:
                return _blockUp;

            case ERoomDirection.Down:
                return _blockDown;

            case ERoomDirection.Left:
                return _blockLeft;

            case ERoomDirection.Right:
                return _blockRight;

            default:
                return null;
        }
    }

    private Vector3 GetBlockWorldPosition(SRoomPlacement placement, ERoomDirection direction, int number)
    {
        Vector2Int gridPos = GetPassageGridPosition(placement, direction, number);

        return new Vector3(
            gridPos.x * _defaultSize.x,
            gridPos.y * _defaultSize.y,
            placement.WorldPosition.z
        );
    }

    private void SpawnBlock(SRoomPlacement placement, ERoomDirection direction, int number)
    {
        // 존재하지 않는 출입구는 검사하지 않음
        if (!HasPassage(placement.RoomPrefab, direction, number))
        {
            return;
        }

        // 다른 방과 연결되어 있으면 막지 않음
        if (IsPassageConnected(placement, direction, number))
        {
            return;
        }

        // 방향에 맞는 벽 프리팹 선택
        GameObject blockPrefab = GetBlockPrefab(direction);

        if (blockPrefab == null)
        {
            return;
        }

        // 출입구가 속한 26×18 칸의 월드 좌표
        Vector3 blockPosition = GetBlockWorldPosition(placement, direction, number);

        // 벽 프리팹 생성
        Instantiate(blockPrefab, blockPosition, Quaternion.identity, _generatedDungeonRoot);
    }
}
