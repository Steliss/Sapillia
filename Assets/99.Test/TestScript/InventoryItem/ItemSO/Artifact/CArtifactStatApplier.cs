using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CArtifactStatApplier : MonoBehaviour
{
    #region 인스펙터

    [SerializeField] private CDataManger _dataManager;
    #endregion

    #region 내부변수

    private CInventoryData _subscribedInventory;


    #endregion



    public void SetDataManager(CDataManger dataManager)
    {
        _dataManager = dataManager;
    }


    public void Initialize(CDataManger dataManager)
    {
        if (dataManager == null)
        {
            Debug.LogWarning("DataManager가 존재하지 않습니다.");
            return;
        }

        SetDataManager(dataManager);
        BindInventory();
        RefreshArtifactStats();
    }


    /// <summary>
    /// 인벤토리 변경 이벤트 연결
    /// </summary>
    public void BindInventory()
    {
        if (_dataManager == null)
        {
            return;
        }

        CInventoryData inventory = _dataManager.InventoryData?.InventoryData;

        if (inventory == null)
        {
            return;
        }

        // 기존 인벤토리 이벤트 구독 해제
        if (_subscribedInventory != null)
        {
            _subscribedInventory.OnInventoryChanged -= RefreshArtifactStats;
        }

        // 새로운 인벤토리 연결
        _subscribedInventory = inventory;

        // 인벤토리 변경 시 아티팩트 능력치 갱신
        _subscribedInventory.OnInventoryChanged += RefreshArtifactStats;
    }


    private void OnEnable()
    {
        CManagerDontDestroy manager = CManagerDontDestroy.Instance;

        if (manager == null || manager.DataManger == null)
        {
            Debug.LogWarning("CArtifactStatApplier: DataManager를 찾을 수 없습니다.");
            return;
        }

        Initialize(manager.DataManger);
    }


    private void OnDisable()
    {
        if (_subscribedInventory != null)
        {
            _subscribedInventory.OnInventoryChanged -= RefreshArtifactStats;
            _subscribedInventory = null;
        }
    }



    /// <summary>
    /// 오브젝트 제거 시 인벤토리 이벤트 구독 해제
    /// </summary>
    private void OnDestroy()
    {
        if (_subscribedInventory != null)
        {
            _subscribedInventory.OnInventoryChanged -= RefreshArtifactStats;
            _subscribedInventory = null;
        }
    }

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




    [Header("획득 테스트용")]
    [SerializeField] private CArtifactSO _testArtifact;

    [ContextMenu("Test Add Artifact")]
    private void TestAddArtifact()
    {
        CInventoryData inventory = _dataManager.InventoryData?.InventoryData;

        if (inventory == null || _testArtifact == null)
        {
            Debug.LogWarning("인벤토리 또는 테스트 아티팩트가 없습니다.");
            return;
        }

        bool success = inventory.AddArtifact(_testArtifact);

        Debug.Log(success
            ? $"아티팩트 획득 성공: {_testArtifact.ItemID}"
            : "아티팩트 획득 실패");
    }

}