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
                _rewardUI = FindAnyObjectByType<CArtifactRewardUI>(
                    FindObjectsInactive.Include
                );
            }

            if (_rewardUI == null)
            {
                Debug.LogError("[CArtifactReward] 보상 선택 UI를 찾을 수 없습니다.");
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

    public void CancelReward()
    {
        _isRewardOpened = false;
    }

}
