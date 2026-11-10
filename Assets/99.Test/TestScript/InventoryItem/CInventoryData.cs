using System;
using System.Collections.Generic;
using UnityEngine;

public enum ETabletRotation
{
    None = 0,     
    Rotation90, 
    Rotation180,
    Rotation270 
}

public enum EItemType
{
    None = 0,
    Artifact,
    Potion,
    Tablet
}

[Serializable]
public class CInventorySlotData
{
    /*
        인벤토리 아이템 관련
        Id = SO와 연결을 위해 필요
        숫자 = 포션용
        강화수치 = 아이템 용
        회전 = 석판용
    */
    public int ItemID;
    public int Count;
    public int EnhanceLevel;
    public EItemType ItemType;
    public ETabletRotation Rotation;

    // 실행 중 사용하며 JSON에는 저장하지 않음
    [NonSerialized] public CItemSO ItemSO;

    public bool IsEmpty => ItemID == 0; // 인덱스 확인용 불값

    

    /// <summary>
    /// 해당 슬롯의 아이템 비우기
    /// </summary>
    public void Clear()
    {
        ItemID = 0;
        Count = 0;
        ItemType = EItemType.None;
        Rotation = ETabletRotation.None;

        ItemSO = null;
    }

    /// <summary>
    /// 현재 레벨에 따른 아티팩트 능력치 반환
    /// </summary>
    public SPlayerStats GetArtifactStats()
    {
        if (IsEmpty || ItemSO is not CArtifactSO artifact || EnhanceLevel < 0)
        {
            return default;
        }

        return artifact.GetStats(EnhanceLevel);
    }

    /// <summary>
    /// 석판 회전함수
    /// </summary>
    /// <returns></returns>
    public List<CTabletEffectLange> RotateTablet()
    {
        if (IsEmpty || ItemSO is not CTabletSO tablet)
        {
            return new List<CTabletEffectLange>();
        }

        // 회전 가능한 경우 다음 방향으로 변경
        if (tablet.CanRotate)
        {
            Rotation = tablet.NextRotation(Rotation);
        }

        // 현재 방향의 전체 영향 범위 반환
        return tablet.RotatedEffects(Rotation);
    }
}

[Serializable]
public class CInventoryData
{
    [SerializeField] private int _inventorySize = 30; // 인벤토리 크기

    [SerializeField] private List<CInventorySlotData> inventorySlots = new List<CInventorySlotData>(); // 인벤토리
    [SerializeField] private List<CInventorySlotData> potionSlots = new List<CInventorySlotData>();    // 포션
    [SerializeField] private List<CInventorySlotData> extraInventorySlots = new List<CInventorySlotData>(); // 벨트

    public List<CInventorySlotData> InventorySlots => inventorySlots;
    public List<CInventorySlotData> PotionSlots => potionSlots;
    public List<CInventorySlotData> ExtraInventorySlots => extraInventorySlots;


    public const int _width = 6; // 인벤토리의 가로 칸 수

    public int InventorySize => _inventorySize;

    [field : NonSerialized] public event Action OnInventoryChanged;


    /// <summary>
    /// 인벤토리 초기 생성 관련
    /// </summary>
    public void Initialize()
    {
        // 리스트 생성
        inventorySlots = new List<CInventorySlotData>();
        potionSlots = new List<CInventorySlotData>();
        extraInventorySlots = new List<CInventorySlotData>();


        for (int i = 0; i < 30; i++)
        {
            inventorySlots.Add(new CInventorySlotData());
        }
        for (int i = 0; i < 2; i++)
        {
            potionSlots.Add(new CInventorySlotData());
        }
        for (int i = 0; i < 6; i++)
        {
            extraInventorySlots.Add(new CInventorySlotData());
        }

        _inventorySize = inventorySlots.Count;
    }

    /// <summary>
    /// 인벤토리 슬롯 확장
    /// </summary>
    public void ExpandInventory(int size)
    {
        if (size <= 0)
        {
            return;
        }

        for (int i = 0; i < size; i++)
        {
            inventorySlots.Add(new CInventorySlotData());
        }

        _inventorySize = inventorySlots.Count;
        
        NotifyInventoryChanged();
    }


public SPlayerStats GetTotalArtifactStats()
{
    SPlayerStats totalStats = default;

    foreach (CInventorySlotData slot in inventorySlots)
    {
        SPlayerStats slotStats = slot.GetArtifactStats();

        totalStats = CPlayerStatsCalculator.Add(totalStats, slotStats);
    }

    return totalStats;
}

    /// <summary>
    /// 아티팩트를 인벤토리의 첫 번째 빈 슬롯에 추가
    /// </summary>
    public bool AddArtifact(CArtifactSO artifact)
    {
        // 유효한 아티팩트인지 검사
        if (artifact == null || artifact.ItemID <= 0)
        {
            return false;
        }

        // 빈 슬롯 검색
        foreach (CInventorySlotData slot in inventorySlots)
        {
            if (!slot.IsEmpty)
            {
                continue;
            }

            // 아티팩트 정보 등록
            slot.ItemID = artifact.ItemID;
            slot.ItemType = artifact.Type;
            slot.ItemSO = artifact;
            slot.Count = 1;
            slot.Rotation = ETabletRotation.None;

            // 슬롯의 EnhanceLevel은 변경하지 않음

            // 인벤토리 변경 알림
            NotifyInventoryChanged();

            return true;
        }

        // 빈 슬롯이 없으면 획득 실패
        return false;
    }




    /// <summary>
    /// 인벤토리 변경됐을때
    /// </summary>
    public void NotifyInventoryChanged()
    {
        ApplyTabletLevels();

        OnInventoryChanged?.Invoke();
    }


    /// <summary>
    /// 메인 인벤토리의 각 칸에 적용되는 석판 레벨 증감 합계
    /// </summary>
    public int[] CalculateTabletLevels()
    {
        int[] levels = new int[inventorySlots.Count];

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            CInventorySlotData slot = inventorySlots[i];

            if (slot.IsEmpty || slot.ItemSO is not CTabletSO tablet)
            {
                continue;
            }

            // 왼쪽부터 X 증가, 위에서 아래로 Y 증가
            int tabletX = i % _width;
            int tabletY = i / _width;

            List<CTabletEffectLange> effects = tablet.RotatedEffects(slot.Rotation);

            foreach (CTabletEffectLange effect in effects)
            {
                int targetX = tabletX + effect.Offset.x;

                // 효과 좌표는 위쪽이 Y+이므로 빼기
                int targetY = tabletY - effect.Offset.y;

                // 좌우 경계와 위쪽 경계 검사
                if (targetX < 0 ||
                    targetX >= _width ||
                    targetY < 0)
                {
                    continue;
                }

                int targetIndex = targetY * _width + targetX;

                // 아래쪽 경계와 마지막 줄의 없는 칸 검사
                if (targetIndex >= inventorySlots.Count)
                {
                    continue;
                }

                levels[targetIndex] += effect.LevelChange;
            }
        }

        return levels;
    }


    /// <summary>
    /// 석판 계산 결과를 각 슬롯에 덮어쓰기
    /// </summary>
    public void ApplyTabletLevels()
    {
        int[] levels = CalculateTabletLevels();

        for (int i = 0; i < inventorySlots.Count; i++)
        {
            inventorySlots[i].EnhanceLevel = levels[i];
        }

        // 보관용 영역에는 석판 효과가 적용되지 않음
        foreach (CInventorySlotData slot in potionSlots)
        {
            slot.EnhanceLevel = 0;
        }

        foreach (CInventorySlotData slot in extraInventorySlots)
        {
            slot.EnhanceLevel = 0;
        }
    }
}
