using UnityEngine;

public class CPlayerController : MonoBehaviour,IInputCommand
{
    #region 인스펙터
    [SerializeField] private CPlayerCostume _playerCostume;
    [SerializeField] private CESCUI _ui;
    [SerializeField] private CPotionList _potion;
    [SerializeField] private CPlayerSound _playerSound;
    #endregion

    #region 내부변수
    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CDataManger _dataManager;
    private CInputCommandHub _inputHub;
    private bool _isInputRegistered;
    #endregion


    #region 프로퍼티
    public CPlayerGamingDataSO PlayerData { get; private set; }
    #endregion

    #region Unity

    private void Awake()
    {
                                 

    }

    private void OnEnable()
    {
        if (_playerCostume == null)
        {
            _playerCostume = GetComponentInChildren<CPlayerCostume>(true);
        }                      
        
        _inputTask.Run(async token =>
        {

            CDataManger manager = await CManagerDontDestroy.DataMangerAsync(token);
            CLog.Log("CPlayerAttack OnEnable 작업 완료");

            if (token.IsCancellationRequested || !isActiveAndEnabled)
            {
                return;
            }

            if (manager == null)
            {
                CLog.Error("[PlayerController] DataManager 연결 실패");
                return;
            }

            _dataManager = manager;

            // 기존 런타임 데이터 연결 시도
            TryBindPlayerData();

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
        // 비동기 초기화 중단
        _inputTask.Cancel();

        UnRegisterInput();

        _inputHub = null;
        _dataManager = null;
    }

  
    private void Update()
    {
        // 런 세션 데이터가 늦게 준비되거나 교체된 경우 연결
        // 다만 계속 호출이 될 수도 있기 때문에 주의해야할 사항
        if (_dataManager != null)
        {            
            TryBindPlayerData();
        }

        if (PlayerData == null || !PlayerData.IsInitialized)
        {
            return;
        }

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
        if (_inputHub == null || _isInputRegistered)
        {
            return;
        }


        _inputHub.Register(EKeyAction.UsePotion, (IInputCommand)this);
        _isInputRegistered = true;
    }

    private void UnRegisterInput()
    {
        if (_inputHub == null)
        {
            return;
        }

        _inputHub.Unregister(EKeyAction.UsePotion, (IInputCommand)this);
        _isInputRegistered = false;
    }

    public void Execute()
    {
        if (_ui.IsAnyUIOpen)
        {
            return;
        }

        _potion.UseCurrentPotion(PlayerData);
    }

    private void TryBindPlayerData()
    {
        if (_dataManager == null || _dataManager.PlayerData == null)
        {
            return;
        }
        
        CPlayerGamingDataSO currentData = _dataManager.PlayerData.GetPlayerData();

        if (currentData == null || !currentData.IsInitialized)
        {
            return;
        }

        // 이미 같은 런타임 객체를 사용 중이라면 아무것도 하지 않음
        if (ReferenceEquals(PlayerData, currentData))
        {
            // 데이터가 같아도 사운드 연결 상태 확인
            _playerSound?.BindPlayerData(currentData);
            return;
        }

        // 기존 런 세션의 객체 참조만 연결
        PlayerData = currentData;

        // 피격 / 사망 이벤트 연결
        _playerSound?.BindPlayerData(PlayerData);
    }

    public void Bind(CPlayerGamingDataSO player, CCostumeDataSO costume)
    {
        if(player == null)
        {
            CLog.Warn("[PlayerController] 전달받은 player 값이 NULL");
            return;
        }

        PlayerData = player;

        // 사운드 이벤트 연결
        _playerSound?.BindPlayerData(PlayerData);

        // 외형 변경
        if (_playerCostume != null)
        {
            _playerCostume.ApplyCostume(costume);
        }
    }

    
}
