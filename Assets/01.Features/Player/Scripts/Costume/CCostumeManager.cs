using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public enum Costume
{
    Pink,
    Armored,
    White
    
}


public class CCostumeManager : MonoBehaviour
{

    #region 기본 코스튬
    [Header("Costume Data")]
    [SerializeField] private CCostumeDataSO _defaultCostume;
    [SerializeField] private CCostumeDataSO _pinkCostume;
    [SerializeField] private CCostumeDataSO _armoredCostume;
    [SerializeField] private CCostumeDataSO _whiteCostume;
    #endregion

    #region 
    private CCostumeDataSO _currentCostume;
    private CUniTaskOnes _dataTask = new CUniTaskOnes();
    private CDataManger _dataManager;
    private CPlayerGamingDataSO _playerData;
    #endregion

    #region 현재코스튬
    public CCostumeDataSO CurrentCostume => _currentCostume;

    #endregion

    #region 이벤트
    public event Action<CCostumeDataSO> OnCostumeChanged;

    #endregion
    #region 코스튬 변경
    // 열거체로 코스튬 선택
    public bool SelectCostume(Costume type)
    {
        switch (type)
        {
            case Costume.White:
                return SelectCostume(_whiteCostume);

            case Costume.Pink:
                return SelectCostume(_pinkCostume);

            case Costume.Armored:
                return SelectCostume(_armoredCostume);

        }

        return SelectCostume(_pinkCostume);
    }

    // 코스튬 변경
    public bool SelectCostume(CCostumeDataSO costume)
    {
        if (costume == null)
        {
            CLog.Warn("[CostumeManager] 코스튬 데이터 없음");
            return false;
        }

        if (_playerData == null || !_playerData.IsInitialized || _dataManager == null)
        {
            CLog.Warn("[CostumeManager] 플레이어 데이터 준비 전");
            return false;
        }

        // 이미 선택한 코스튬
        if (_currentCostume == costume)
        {
            return true;
        }

        // 코스튬 변경
        _currentCostume = costume;

        Costume type = Costume.Pink;

        if (_currentCostume == _armoredCostume)
        {
            type = Costume.Armored;
        }
        else if (_currentCostume == _whiteCostume)
        {
            type = Costume.White;
        }

        _playerData.CostumeSO(_currentCostume, type);

        // 변경 이벤트 호출
        OnCostumeChanged?.Invoke(_currentCostume);

        // 실제 코스튬 변경 시에만 저장
        if (!_dataManager.SaveGame(_dataManager.CurrentFlow.SaveSlot))
        {
            CLog.Warn("[CostumeManager] 코스튬 저장 실패");
        }

        CLog.Log($"[CostumeManager] 코스튬 변경 : " + $"{_currentCostume.CostumeName}");

        return true;
    }

    /// <summary>
    /// UI 버튼에 연결할 함수
    /// </summary>

    // 버튼 연결 → Basic 코스튬으로 변경
    public void SelectBasic()
    {
        SelectCostume(Costume.Pink);
    }
    // 버튼 연결 → Armored 코스튬으로 변경
    public void SelectArmored()
    {
        SelectCostume(Costume.Armored);
    }
    // 버튼 연결 → White 코스튬으로 변경
    public void SelectWhite()
    {
        SelectCostume(Costume.White);
    }

    #endregion


    #region SO에 매핑
    private CCostumeDataSO GetCostumeData(Costume type)
    {
        switch (type)
        {
            case Costume.Pink:
                return _pinkCostume;

            case Costume.Armored:
                return _armoredCostume;

            case Costume.White:
                return _whiteCostume;
        }

        return _defaultCostume;
    }

    #endregion

    #region Unity
    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            CDataManger manager = await CManagerDontDestroy.DataMangerAsync(token);            

            if (token.IsCancellationRequested || !isActiveAndEnabled || manager == null)
            {
                CLog.Error("[CostumeManager] DataManager 연결 실패");
                return;
            }

            _dataManager = manager;

            try
            {
                // RunSession에 플레이어 데이터가 준비될 때까지 대기
                await UniTask.WaitUntil(
                    () => _dataManager.PlayerData != null &&
                          _dataManager.PlayerData.GetPlayerData() != null &&
                          _dataManager.PlayerData.GetPlayerData().IsInitialized,
                    cancellationToken: token
                );
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (token.IsCancellationRequested || !isActiveAndEnabled)
            {
                return;
            }

            _playerData = _dataManager.PlayerData.GetPlayerData();

            // 저장되어 있는 코스튬 우선
            _currentCostume = GetCostumeData(_playerData.CostumeType);

            // 신규 플레이어인 경우 기본 코스튬 적용
            if (_currentCostume == null)
            {
                _currentCostume = _defaultCostume;
            }

            if (_currentCostume == null)
            {
                CLog.Error("[CostumeManager] 기본 코스튬 참조 누락");
                return;
            }

            // 코스튬 참조와 보너스를 플레이어 데이터에 반영
            _playerData.CostumeSO(_currentCostume);

            CLog.Log($"[CostumeManager] 코스튬 준비 완료 : {_currentCostume.CostumeName}");

            // 준비가 완료된 시점에 변경 알림
            OnCostumeChanged?.Invoke(_currentCostume);
        });


    }

    private void OnDisable()
    {
        _dataTask.Cancel();

        _dataManager = null;
        _playerData = null;
        _currentCostume = null;
    }

    private void Awake()
    {
        
    }

    private void Start()
    {
        
    }


    #endregion



}
