using UnityEngine;


[CreateAssetMenu(menuName = "SO/Player/PlayerData", fileName = "PlayerStats")]
public class CPlayerDataSO : ScriptableObject
{
    #region 인스펙터
    [Header("기본 플레이어 스탯")]
    [SerializeField] private SPlayerStats _baseStats = SPlayerStats.CreateDefault();

    [Header("시작 재화")]
    [Min(0)]
    [SerializeField] private int _startingMoney = 500;

    [Header("경험치")]
    [Min(1f)]
    [SerializeField] private float _expGrowthMultiplier = 1.2f;

    #endregion

    #region 프로퍼티
    public SPlayerStats BaseStats => _baseStats;
    public int StartMoney => _startingMoney;
    public float ExpGrowthMultiplier => _expGrowthMultiplier;

    #endregion

}