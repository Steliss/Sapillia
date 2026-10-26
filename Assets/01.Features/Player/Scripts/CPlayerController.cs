using UnityEngine;

public class CPlayerController : MonoBehaviour
{
    #region 인스펙터
    //[SerializeField] private CPlayerGamingDataSO _playerData;
    [SerializeField] private CPlayerCostume _playerCostume;
    #endregion

    #region 프로퍼티
    public CPlayerGamingDataSO PlayerData { get; private set; }
    #endregion

    private void Awake()
    {
        if(_playerCostume == null)
        {
            _playerCostume = GetComponentInChildren<CPlayerCostume>(true);
        }


        
    }

    void Start()
    {
        
        // 중복 연결 방지
        if(PlayerData != null)
        {
            return;
        }

        CDataManger _dataManager = FindFirstObjectByType<CDataManger>();

        // 테스트용으로 1번 슬롯 불러오기
        if (!_dataManager.LoadGame(1))
        {
            CLog.Warn("[CPlayerController] 게임 불러오기 실패");
            return;
        }

        // 런 세션에서 플레이어 데이터 가져오기
        PlayerData = _dataManager.PlayerData.GetPlayerData();

        
        if(PlayerData == null)
        {
            CLog.Warn("[PlayerController] PlayerData 연결 실패");
        }

        CLog.Log("[PlayerController] PlayerData 연결 성공");
        CLog.Log($"HP : {PlayerData.CurrentHP}");
        CLog.Log($"MP : {PlayerData.CurrentMP}");
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
