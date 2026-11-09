using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CArtifactReward : MonoBehaviour
{
    #region 인스펙터

    [Header("아티팩트 보상 선택 UI")]
    [SerializeField] private CArtifactRewardUI _rewardUI;

    #endregion

    #region 내부변수

    private bool _isPlayerNearby;

    private bool _isRewardOpened = false;

    #endregion

    private void Update()
    {
        if (!_isPlayerNearby || _isRewardOpened)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_rewardUI == null)
            {
                Debug.LogError(
                    $"[CArtifactReward] Reward UI가 연결되지 않았습니다! 오브젝트: {gameObject.name}"
                );
                return;
            }

            _isRewardOpened = true;
            _rewardUI.ShowRewards(this);
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

}
