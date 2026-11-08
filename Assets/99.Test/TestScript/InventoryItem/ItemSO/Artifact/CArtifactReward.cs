using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CArtifactReward : MonoBehaviour
{
    #region 인스펙터

    [Header("획득할 아티팩트")]
    [SerializeField] private CArtifactSO _artifact;

    [Header("데이터 매니저")]
    [SerializeField] private CDataManger _dataManager;

    #endregion

    #region 내부변수

    private bool _isPlayerNearby;

    #endregion

    private void Update()
    {
        if (!_isPlayerNearby)
        {
            return;
        }

        // 테스트용 상호작용 키
        if (Input.GetKeyDown(KeyCode.E))
        {
            AcquireArtifact();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerNearby = false;
    }

    /// <summary>
    /// 아티팩트 획득
    /// </summary>
    private void AcquireArtifact()
    {
        if (_artifact == null || _dataManager == null)
        {
            return;
        }

        CInventoryData inventory = _dataManager.InventoryData?.InventoryData;

        if (inventory == null)
        {
            return;
        }

        bool success = inventory.AddArtifact(_artifact);

        if (!success)
        {
            Debug.Log("아티팩트 획득 실패");
            return;
        }

        Debug.Log($"아티팩트 획득 성공: {_artifact.ItemID}");

        // 획득 완료 시 보상 오브젝트 제거
        Destroy(gameObject);
    }
}
