using UnityEngine;

public class CPlayerController : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private CPlayerGamingDataSO _player;
    [SerializeField] private CPlayerCostume _playerCostume;
    #endregion

    #region 프로퍼티
    public CPlayerGamingDataSO PlayerData { get; private set; }
    #endregion

    private void Awake()
    {
        _player.Initalize();
    }

    void Start()
    {
        
    }

    void Update()
    {
        _player.RecoverMPperTime(Time.fixedDeltaTime);
        _player.ReChargeDashperTime(Time.fixedDeltaTime);
    }

    public void Bind(CPlayerGamingDataSO player, CCostumeDataSO costume)
    {
        _player = player;

        // 외형 변경
        if(_playerCostume != null)
        {
            _playerCostume.ApplyCostume(costume);
        }
    }
}
