using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CArtifactStatApplier : MonoBehaviour
{
    #region 인스펙터

    [SerializeField] private CDataManger _dataManager;
    #endregion

    /// <summary>
    /// 인벤토리의 아티팩트 능력치를 플레이어에게 적용
    /// </summary>
    [ContextMenu("Refresh Artifact Stats")]
    public void RefreshArtifactStats()
    {
        if (_dataManager == null)
        {
            Debug.LogWarning("DataManager가 연결되지 않았습니다.");
            return;
        }

        CInventoryData inventory = _dataManager.InventoryData?.InventoryData;
        CPlayerGamingDataSO player = _dataManager.PlayerData?.PlayerData;

        if (inventory == null || player == null || !player.IsInitialized)
        {
            Debug.LogWarning("플레이어 또는 인벤토리 데이터가 준비되지 않았습니다.");
            return;
        }

        SPlayerStats artifactStats = inventory.GetTotalArtifactStats();

        player.SetArtifactBonus(artifactStats);

        Debug.Log(
            $"아티팩트 적용 완료 | 물리속성: {player.Stats.Attribute_Physical} " +
            $"| 특수공격: {player.Stats.Damage_Special}"
        );
    }
}