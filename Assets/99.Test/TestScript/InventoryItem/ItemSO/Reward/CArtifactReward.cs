using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CArtifactReward : MonoBehaviour, IInputCommand
{
    #region 인스펙터

    [Header("아티팩트 보상 선택 UI")]
    [SerializeField] private CArtifactRewardUI _rewardUI;

    #endregion

    #region 내부변수

    private bool _isPlayerNearby;

    private bool _isRewardOpened = false;

    private CInputCommandHub _inputHub;

    #endregion


    private void OnEnable()
    {
        _inputHub = FindFirstObjectByType<CInputCommandHub>();

        if (_inputHub == null)
        {
            Debug.Log("CArtifactReward 인풋허브오류");
            return;
        }

        _inputHub.Register(EKeyAction.Interaction, this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerNearby = true;
    }

    private void OnDisable()
    {
        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Interaction, this);
        }

        _isPlayerNearby = false;
    }

    public void Execute()
    {
        if (!_isPlayerNearby || _isRewardOpened)
        {
            return;
        }

        if (_rewardUI == null)
        {
            _rewardUI = FindAnyObjectByType<CArtifactRewardUI>(
                FindObjectsInactive.Include
            );
        }

        if (_rewardUI == null)
        {
            Debug.LogError("CArtifactReward 보상 선택 UI 오류");
            return;
        }

        _isRewardOpened = true;
        _rewardUI.ShowRewards(this);
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
