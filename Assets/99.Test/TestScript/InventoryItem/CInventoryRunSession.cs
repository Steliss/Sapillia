using System.Collections.Generic;
using UnityEngine;

// 런타임중 데이터 생성 관리
public class CInventoryRunSession : MonoBehaviour
{
    [SerializeField] private CItemDatabaseSO _itemDatabase;

    public CInventoryData InventoryData { get; private set; }

    /// <summary>
    /// CDataManger 저장용 함수
    /// </summary>
    public CInventoryData GetInventoryData()
    {
        return InventoryData;
    }

    /// <summary>
    /// 새 인벤토리 생성 또는 불러온 데이터 연결
    /// </summary>
    public void SetInventoryData(CInventoryData inventoryData = null)
    {

        // 인벤토리 정보가 없을시 새 인벤토리 생성
        if (inventoryData == null)
        {
            InventoryData = new CInventoryData();
            InventoryData.Initialize();
            return;
        }
        else
        {
            InventoryData = inventoryData;

            // json 인덱스와 SO연결
            BindItemSO(InventoryData.InventorySlots);
            BindItemSO(InventoryData.PotionSlots);
            BindItemSO(InventoryData.ExtraInventorySlots);
        }
    }



    /// <summary>
    /// 슬롯에 저장된 ID로 아이템 SO 연결
    /// </summary>
    private void BindItemSO(List<CInventorySlotData> slots)
    {
        foreach (CInventorySlotData slot in slots)
        {
            // 슬롯이 0번 비어있을경우
            if (slot.IsEmpty)
            {
                continue;
            }

            slot.ItemSO = _itemDatabase.SetItem(slot.ItemID);

            if (slot.ItemSO == null)
            {
                CLog.Warn($"아이템 SO를 찾을 수 없습니다. ID: {slot.ItemID}");
            }
        }
    }
}