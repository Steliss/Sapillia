using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CTabletReward : MonoBehaviour, IInputCommand
{
    #region 인스펙터

    [Header("석판 보상 선택 UI")]
    [SerializeField] private CTabletRewardUI _rewardUI;

    #endregion

    #region 내부변수

    private bool _isPlayerNearby;
    private bool _isRewardOpened = false;

    private CInputCommandHub _inputHub;
    private CancellationTokenSource _registerCts;
    #endregion

    private void OnEnable()
    {
        _registerCts = new CancellationTokenSource();
        RegisterInputAsync(_registerCts.Token).Forget();
    }

    private async UniTask RegisterInputAsync(CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            _inputHub = FindFirstObjectByType<CInputCommandHub>();

            if (_inputHub != null)
            {
                _inputHub.Register(EKeyAction.Interaction, this);
                return;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, token);
        }
    }

    public void Execute()
    {
        if (!_isPlayerNearby || _isRewardOpened)
        {
            return;
        }

        if (_rewardUI == null)
        {
            _rewardUI = FindAnyObjectByType<CTabletRewardUI>(
                FindObjectsInactive.Include
            );
        }

        if (_rewardUI == null)
        {
            Debug.LogError("[CTabletReward] 석판 보상 UI를 찾을 수 없습니다.", this);
            return;
        }

        _isRewardOpened = true;
        _rewardUI.ShowRewards(this);
    }

    private void OnDisable()
    {
        _registerCts?.Cancel();
        _registerCts?.Dispose();
        _registerCts = null;

        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Interaction, this);
        }

        _isPlayerNearby = false;
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