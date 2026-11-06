using UnityEngine;

public class ArtifactTestScript : MonoBehaviour
{
    #region 인스펙터

    [SerializeField] private CArtifactSO _artifactA;
    [SerializeField] private CArtifactSO _artifactB;

    [SerializeField] private int _starsA = 2;
    [SerializeField] private int _starsB = 1;

    #endregion

    private void Start()
    {
        if (_artifactA == null || _artifactB == null)
        {
            Debug.LogError("테스트 아티팩트가 설정되지 않았습니다.");
            return;
        }

        // 테스트용 인벤토리 생성
        CInventoryData inventory = new CInventoryData();
        inventory.Initialize();

        // 첫 번째 슬롯에 아티팩트 A 배치
        CInventorySlotData slotA = inventory.InventorySlots[0];

        slotA.ItemID = _artifactA.ItemID;
        slotA.ItemType = _artifactA.Type;
        slotA.ItemSO = _artifactA;
        slotA.EnhanceLevel = _starsA;

        // 두 번째 슬롯에 아티팩트 B 배치
        CInventorySlotData slotB = inventory.InventorySlots[1];

        slotB.ItemID = _artifactB.ItemID;
        slotB.ItemType = _artifactB.Type;
        slotB.ItemSO = _artifactB;
        slotB.EnhanceLevel = _starsB;

        // 슬롯별 능력치 계산
        SPlayerStats statsA = slotA.GetArtifactStats();
        SPlayerStats statsB = slotB.GetArtifactStats();

        // 인벤토리 전체 능력치 합산
        SPlayerStats totalStats = inventory.GetTotalArtifactStats();

        Debug.Log($"슬롯 0 ({_starsA}성) | 힘: {statsA.Attribute_Physical} | 체력: {statsA.MaxHP}");
        Debug.Log($"슬롯 1 ({_starsB}성) | 특수: {statsB.Damage_Special} | 체력: {statsB.MaxHP}");

        Debug.Log($"인벤토리 전체 합계 | 물리속성: {totalStats.Attribute_Physical} | 특수공격: {totalStats.Damage_Special}");

        // 테스트용 플레이어 데이터 생성
        CPlayerGamingDataSO playerData = new CPlayerGamingDataSO();
        playerData.Initalize();

        // 아티팩트 적용 전
        Debug.Log($"적용 전 | 물리속성: {playerData.Stats.Attribute_Physical} | 특수공격: {playerData.Stats.Damage_Special}");

        // 인벤토리에서 합산한 아티팩트 능력치 적용
        playerData.SetArtifactBonus(totalStats);

        // 아티팩트 적용 후
        Debug.Log($"적용 후 | 물리속성: {playerData.Stats.Attribute_Physical} | 특수공격: {playerData.Stats.Damage_Special}");

        // 아티팩트 보너스 제거
        playerData.SetArtifactBonus(default);

        // 제거 후
        Debug.Log($"제거 후 | 물리속성: {playerData.Stats.Attribute_Physical} | 특수공격: {playerData.Stats.Damage_Special}");
    }
}