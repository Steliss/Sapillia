using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CMinimapUI : MonoBehaviour
{
    #region 인스펙터

    [Header("미니맵")]
    [SerializeField] private RectTransform _miniMap;

    [Header("방 UI 프리팹")]
    [SerializeField] private RectTransform _roomPrefab;

    [Header("방 크기 설정")]
    [SerializeField] private float _roomUnitSize = 200f;
    [SerializeField] private float _roomGap = 4f;
    [SerializeField] private float _minimpaGap = 30f;

    [Header("방 크기별 스프라이트")]
    [SerializeField] private Sprite _room1x1;
    [SerializeField] private Sprite _room1x2;
    [SerializeField] private Sprite _room2x1;
    [SerializeField] private Sprite _room2x2;

    [Header("스크롤 설정")]
    [SerializeField] private ScrollRect _scrollRect;
    [SerializeField] private float _dragSpace = 100f;

    #endregion

    #region 내부변수

    private CDungeonGenerator _dungeonGenerator;
    private List<RectTransform> _createdRooms = new List<RectTransform>();

    private CanvasGroup _canvasGroup;
    private bool _isMapOpen = false;

    private CPlayerGamingDataSO _playerData;

    #endregion

    private void Awake()
    {
        _dungeonGenerator = FindFirstObjectByType<CDungeonGenerator>();

        _canvasGroup = GetComponent<CanvasGroup>();

        if (_canvasGroup == null)
        {
            Debug.LogError("[미니맵] CanvasGroup이 없습니다.");
            return;
        }

        SetMinimapVisible(false);
    }

    private void Start()
    {
        CDataManger dataManager = CManagerDontDestroy.Instance.DataManger;

        if (dataManager == null)
        {
            Debug.LogError("미니맵DataManager가 없습니다.");
            return;
        }

        _playerData = dataManager.PlayerData.GetPlayerData();

        if (_playerData == null)
        {
            Debug.LogError("미니맵PlayerData가 없습니다.");
        }
    }


    private void Update()
    {
        if (_playerData == null)
            return;

        if (_playerData.GetIsBattle)
        {
            if (_isMapOpen)
            {
                SetMinimapVisible(false);
            }

            return;
        }

        if (Input.GetKeyDown(KeyCode.Tab))
        {
            SetMinimapVisible(!_isMapOpen);
        }
    }


    private void OnEnable()
    {
        if (_dungeonGenerator == null)
        {
            Debug.LogError("[미니맵] DungeonGenerator가 없습니다.");
            return;
        }

        _dungeonGenerator.OnDungeonGenerated += GenerateMinimap;

        if (_dungeonGenerator.IsReady)
        {
            GenerateMinimap();
        }
    }

    private void OnDisable()
    {
        if (_dungeonGenerator != null)
        {
            _dungeonGenerator.OnDungeonGenerated -= GenerateMinimap;
        }
    }

    private void GenerateMinimap()
    {
        if (_miniMap == null || _roomPrefab == null)
        {
            Debug.LogError("[미니맵] UI 참조가 설정되지 않았습니다.");
            return;
        }

        ClearMinimap();

        List<SRoomPlacement> rooms = _dungeonGenerator.RoomPlacements;

        if (rooms.Count == 0)
            return;

        // 던전이 차지하는 그리드 범위
        int minX = int.MaxValue;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        int maxY = int.MinValue;

        foreach (SRoomPlacement room in rooms)
        {
            Vector2Int pos = room.GridPosition;
            Vector2Int size = room.RoomPrefab.RoomSize;

            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);
            maxX = Mathf.Max(maxX, pos.x + size.x);
            maxY = Mathf.Max(maxY, pos.y + size.y);
        }

        float width = maxX - minX;
        float height = maxY - minY;

        Canvas.ForceUpdateCanvases();

        if (_scrollRect == null || _scrollRect.viewport == null)
        {
            Debug.LogError("[미니맵] ScrollRect 또는 Viewport가 없습니다.");
            return;
        }

        RectTransform viewport = _scrollRect.viewport;

        // 방 하나의 기본 크기
        float unitSize = _roomUnitSize;

        // 실제 던전의 전체 크기
        float dungeonWidth = width * unitSize;
        float dungeonHeight = height * unitSize;

        // Content 크기 계산
        float contentWidth = Mathf.Max(dungeonWidth + _minimpaGap * 2f, viewport.rect.width + _dragSpace * 2f);

        float contentHeight = Mathf.Max(dungeonHeight + _minimpaGap * 2f, viewport.rect.height + _dragSpace * 2f);

        // Content 기준점을 중앙으로 설정
        _miniMap.anchorMin = new Vector2(0.5f, 0.5f);
        _miniMap.anchorMax = new Vector2(0.5f, 0.5f);
        _miniMap.pivot = new Vector2(0.5f, 0.5f);

        // Content 크기 설정
        _miniMap.sizeDelta = new Vector2(contentWidth, contentHeight);

        // 처음에는 중앙에 배치
        _miniMap.anchoredPosition = Vector2.zero;

        float centerX = (minX + maxX) * 0.5f;
        float centerY = (minY + maxY) * 0.5f;

        for (int i = 0; i < rooms.Count; i++)
        {
            SRoomPlacement room = rooms[i];

            RectTransform roomUI = Instantiate(_roomPrefab, _miniMap);

            Vector2Int pos = room.GridPosition;
            Vector2Int size = room.RoomPrefab.RoomSize;

            Image roomImage = roomUI.GetComponent<Image>();

            if (roomImage != null)
            {
                if (size == new Vector2Int(1, 1))
                    roomImage.sprite = _room1x1;

                else if (size == new Vector2Int(1, 2))
                    roomImage.sprite = _room1x2;

                else if (size == new Vector2Int(2, 1))
                    roomImage.sprite = _room2x1;

                else if (size == new Vector2Int(2, 2))
                    roomImage.sprite = _room2x2;
            }

            roomUI.anchorMin = new Vector2(0.5f, 0.5f);
            roomUI.anchorMax = new Vector2(0.5f, 0.5f);
            roomUI.pivot = new Vector2(0.5f, 0.5f);
            roomUI.localScale = Vector3.one;

            roomUI.sizeDelta = new Vector2(
                Mathf.Max(1f, size.x * unitSize - _roomGap),
                Mathf.Max(1f, size.y * unitSize - _roomGap)
            );

            roomUI.anchoredPosition = new Vector2(
                (pos.x + size.x * 0.5f - centerX) * unitSize,
                (pos.y + size.y * 0.5f - centerY) * unitSize
            );

            roomUI.gameObject.SetActive(true);
            _createdRooms.Add(roomUI);

            Button roomButton = roomUI.GetComponent<Button>();

            if (roomButton != null)
            {
                int roomIndex = i;

                roomButton.onClick.AddListener(() => TeleportToRoom(roomIndex));
            }
        }

        Canvas.ForceUpdateCanvases();

        // 기존 스크롤 이동 중지
        _scrollRect.StopMovement();

        // 지도 중앙 정렬
        _scrollRect.normalizedPosition = new Vector2(0.5f, 0.5f);

        Debug.Log($"[미니맵] 방 {rooms.Count}개 생성 완료");
    }

    private void ClearMinimap()
    {
        foreach (RectTransform room in _createdRooms)
        {
            if (room != null)
            {
                room.gameObject.SetActive(false);
                Destroy(room.gameObject);
            }
        }

        _createdRooms.Clear();
    }

    private void SetMinimapVisible(bool visible)
    {
        if (visible && (_playerData == null || _playerData.GetIsBattle))
        {
            return;
        }

        _isMapOpen = visible;

        _canvasGroup.alpha = visible ? 1f : 0f;
        _canvasGroup.interactable = visible;
        _canvasGroup.blocksRaycasts = visible;
    }


    private void TeleportToRoom(int roomIndex)
    {
        CRoomData room = _dungeonGenerator.GetSpawnedRoom(roomIndex);

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        player.transform.position = room.PlayerSpawnPoint.position;

        Debug.Log($"[미니맵] {roomIndex}번 방으로 순간이동 완료");

        SetMinimapVisible(false);
    }
}
