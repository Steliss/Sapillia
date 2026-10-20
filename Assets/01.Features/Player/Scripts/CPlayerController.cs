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

        CPlayerRunSession session = FindFirstObjectByType<CPlayerRunSession>();

        if(session == null)
        {
            CLog.Warn("[CPlayerController] Runssesion 누락");
            return;
        }

        session.BindPlayer(this);

        if(PlayerData == null)
        {
            CLog.Warn("[PlayerController] PlayerData 연결 실패");
        }

    }

    void Update()
    {
        PlayerData.RecoverMPperTime(Time.fixedDeltaTime);
        PlayerData.ReChargeDashperTime(Time.fixedDeltaTime);
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
