using System;
using System.Collections.Generic;
using UnityEngine;

public enum TabletRotation
{
    None = 0,     
    Rotation90, 
    Rotation180,
    Rotation270 
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
    public TabletRotation Rotation;
}

[Serializable]
public class CInventoryData
{
    // 인벤토리 크기
    [SerializeField] private int _inventorySize = 30; // 기본 인벤토리 크기

    [SerializeField] private List<CInventorySlotData> inventorySlots; // 인벤토리
    [SerializeField] private List<CInventorySlotData> potionSlots;    // 포션
    [SerializeField] private List<CInventorySlotData> extraInventorySlots; // 벨트

    public List<CInventorySlotData> InventorySlots => inventorySlots;
    public List<CInventorySlotData> PotionSlots => potionSlots;
    public List<CInventorySlotData> ExtraInventorySlots => extraInventorySlots;


    // SO를 통해 전해줄 인벤토리 내 아이템 위치 정보
    private int _width;
    private int _height;


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
    }


}
