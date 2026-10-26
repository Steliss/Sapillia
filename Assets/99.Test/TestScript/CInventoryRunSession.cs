using UnityEngine;

// 플레이어 런 섹션과 같음
public class CInventoryRunSession : MonoBehaviour
{
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

        InventoryData = inventoryData;

    }

}