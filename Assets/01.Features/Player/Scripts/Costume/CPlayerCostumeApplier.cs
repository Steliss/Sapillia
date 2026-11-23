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
    private CCostumeDataSO _dataSO;
    #endregion

    #region 프로퍼티
    public CCostumeDataSO costumeDataSO {  get; private set; }
    #endregion

    #region Unity

    private void Awake()
    {        
        
    }

    private void Start()
    {
        if (_costumeManager.CurrentCostume == null)
        {

        }
        // 초기 코스튬 적용
        ApplyCostume(_costumeManager.CurrentCostume);
    }

    private void OnEnable()
    {
        if (_costumeManager == null || _playerController == null || _playerAnimator == null)
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
                        _playerController.PlayerData != null &&
                        _playerController.PlayerData.IsInitialized &&
                        _costumeManager.CurrentCostume != null,
                        cancellationToken: token
                );                
            }
            catch (OperationCanceledException)
            {
                CLog.Warn("[CostumeAppier] 예외발생");
                // 오브젝트 비활성화 시 대기 취소
            }

            if (token.IsCancellationRequested || !isActiveAndEnabled)
            {
                return;
            }

            // 초기 외형 적용
            ApplyCostume(_costumeManager.CurrentCostume);
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

        costumeDataSO = null;
    }

    #endregion

    #region Costume Apply

    private void HandleCostumeChanged(CCostumeDataSO costume)
    {
        ApplyCostume(costume);
    }

    private void ApplyCostume(CCostumeDataSO costume)
    {
        /*
        // 필수 참조
        if (costume == null || _playerController == null || _playerController.PlayerData == null)
        {
            CLog.Warn("[CPlayerCostumeApplier] 필수 참조 비어있음");
            return;
        }
        */
        if (costume == null)
        {
            CLog.Warn("[CPlayerCostumeApplier] costume 필수 참조 비어있음");
            return;
        }

        // 초기화가 늦었다면 OnEnable의 대기 작업에서 준비 완료 후 다시 적용함
        if (_playerController == null || _playerController.PlayerData == null || !_playerController.PlayerData.IsInitialized)
        {
            return;
        }

        if (_playerAnimator == null)
        {
            CLog.Warn("[CostumeApplier] Animator 참조 없음");
            return;
        }

        if (costume.AnimatorOverride == null)
        {
            CLog.Warn($"[CostumeApplier] {costume.CostumeName} " + $"AnimatorOverride 없음");
            return;
        }

        // 이미 동일한 외형이 적용됐다면 생략
        if (costumeDataSO == costume && _playerAnimator.runtimeAnimatorController == costume.AnimatorOverride)
        {
            return;
        }

        // 플레이어 외형 변경
        _playerAnimator.runtimeAnimatorController = costume.AnimatorOverride;
        costumeDataSO = costume;

        CLog.Log($"[CostumeApplier] 외형 적용 완료 : {costume.CostumeName}");        
    }

    #endregion
}
