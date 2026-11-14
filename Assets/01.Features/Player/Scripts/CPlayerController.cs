using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class CPlayerController : MonoBehaviour,IInputCommand
{
    #region 인스펙터
    [SerializeField] private CPlayerCostume _playerCostume;
    [SerializeField] private CESCUI _ui;
    [SerializeField] private CPotionList _potion;
    #endregion

    #region 내부변수
    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CDataManger _dataManager;
    private CInputCommandHub _inputHub;
    #endregion


    #region 프로퍼티
    public CPlayerGamingDataSO PlayerData { get; private set; }
    #endregion

    #region Unity

    private void Awake()
    {
        if(_playerCostume == null)
        {
            _playerCostume = GetComponentInChildren<CPlayerCostume>(true);
        }

        _dataManager = FindAnyObjectByType<CDataManger>();

        _inputTask.Run(async token =>
        {
            
            _dataManager = await CManagerDontDestroy.DataMangerAsync(token);
            CLog.Log("CPlayerAttack OnEnable 작업 완료");
            // await UniTask.Delay(1000);
            

            // 중복 연결 방지
            if (PlayerData != null)
            {
                return;
            }

            PlayerData = _dataManager.PlayerData.GetPlayerData();

            if (PlayerData == null)
            {
                CLog.Warn("[PlayerController] PlayerData 연결 실패");
            }

            

            CLog.Log("[PlayerController] PlayerData 연결 성공");
            CLog.Log($"HP : {PlayerData.CurrentHP}");
            CLog.Log($"MP : {PlayerData.CurrentMP}");

            _inputHub = await CManagerDontDestroy.InputCommandHubAsync(token);

            if (token.IsCancellationRequested || !isActiveAndEnabled)
            {
                return;
            }

            if (_inputHub == null)
            {
                CLog.Error("[CPlayerMovement] InputHub 참조 실패");
                return;
            }

            RegisterInput();
            CLog.Log("[CPlayerMovement] 모든 초기화 완료");
        });

    }

    private void OnDisable()
    {
        UnRegisterInput();
    }

  
    private void Update()
    {
        if (_ui.IsAnyUIOpen)
        {
            return;
        }

        PlayerData.ReChargeDashperTime(Time.deltaTime);
        PlayerData.RecoverMPperTime(Time.deltaTime);
    }

    #endregion


    private void RegisterInput()
    {
        _inputHub.Register(EKeyAction.UsePotion, (IInputCommand)this);
    }

    private void UnRegisterInput()
    {
        if (_inputHub == null)
        {
            return;
        }

        _inputHub.Unregister(EKeyAction.UsePotion, (IInputCommand)this);
    }

    public void Execute()
    {
        if (_ui.IsAnyUIOpen)
        {
            return;
        }

        _potion.UseCurrentPotion(PlayerData);
    }

    public void Bind(CPlayerGamingDataSO player, CCostumeDataSO costume)
    {
        if(player == null)
        {
            CLog.Warn("[PlayerController] 전달받은 player 값이 NULL");
            return;
        }

        PlayerData = player;

        // 외형 변경
        if(_playerCostume != null)
        {
            _playerCostume.ApplyCostume(costume);
        }
    }

    
}
