using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CPlayerCostumeApplier : MonoBehaviour
{
    #region 인스펙터

    [Header("코스튬 매니저")]
    [SerializeField] private CCostumeManager _costumeManager;

    [Header("플레이어")]
    [SerializeField] private CPlayerController _playerController;
    [SerializeField] private CPlayerCostume _playerCostume;

    [Header("플레이어 Animator")]
    [SerializeField] private Animator _playerAnimator;

    #endregion

    #region 내부 변수
    private CUniTaskOnes _initTask = new CUniTaskOnes();
    private CDataManger _data;
    #endregion

    #region Unity

    private void Awake()
    {
        if (_playerAnimator != null)
        {
        }
    }

    private void OnEnable()
    {
        if (_costumeManager == null)
        {
            CLog.Warn("[CostumeApplier] CostumeManager 참조 없음");
            return;
        }

        // 코스튬 변경 이벤트 구독
        _costumeManager.OnCostumeChanged += HandleCostumeChanged;

        _initTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            CLog.Log("[CPlayerCostumeApplier] 데이터매니저 작업 완료");

            try
            {
                // 플레이어 데이터 초기화 완료 대기
                await UniTask.WaitUntil
                (
                    () =>
                        _playerController != null &&
                        _playerController.PlayerData != null &&
                        _playerController.PlayerData.IsInitialized,
                        cancellationToken: token
                );

                token.ThrowIfCancellationRequested();

                if (!isActiveAndEnabled)
                {
                    return;
                }

                // 초기 코스튬 적용
                ApplyCostume(_costumeManager.CurrentCostume);
            }
            catch (OperationCanceledException)
            {
                CLog.Warn("[CostumeAppier] 예외발생");
                // 오브젝트 비활성화 시 대기 취소
            }
        });

        CLog.Log($"[CostumeAppier] OnEnable 종료");
    }

    private void OnDisable()
    {
        _initTask.Cancel();

        if (_costumeManager != null)
        {
            _costumeManager.OnCostumeChanged -= HandleCostumeChanged;
        }
    }

    #endregion

    #region Costume Apply

    private void HandleCostumeChanged(CCostumeDataSO costume)
    {
        ApplyCostume(costume);
    }

    private void ApplyCostume(CCostumeDataSO costume)
    {
        // 필수 참조
        if (costume == null || _playerController == null || _playerController.PlayerData == null || !_playerController.PlayerData.IsInitialized)
        {
            CLog.Warn("[CPlayerCostumeApplier] 필수 참조 비어있음");
            return;
        }

        if (costume.AnimatorOverride == null)
        {
            CLog.Warn($"[CostumeApplier] {costume.CostumeName} " + $"AnimatorOverride 없음");
            return;
        }

        // 플레이어 외형 변경
        if (_playerAnimator != null)
        {
            _playerAnimator.runtimeAnimatorController = costume.AnimatorOverride;
            CLog.Log("[CPlayerCostumeApplier] 코스튬 외형 변경 완료");
        }

        CLog.Log($"[CostumeApplier] 외형 적용 : {costume.CostumeName} / " +
        $"Animator : {_playerAnimator.gameObject.name} / " +
        $"Controller : {_playerAnimator.runtimeAnimatorController.name}");

        // 코스튬 추가 능력치 반영
        _playerController.PlayerData.SetCostumeBonus(costume.Bonus);
        _data.SaveGame(_data.CurrentFlow.SaveSlot);
        CLog.Log($"[CostumeApplier] 코스튬 적용 완료 : " + $"{costume.CostumeName}");
    }

    #endregion
}
