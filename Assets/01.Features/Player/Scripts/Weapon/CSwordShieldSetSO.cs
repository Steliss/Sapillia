
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Player/Weapon", fileName = "SwordShieldSet")]
public class CSwordShieldSetSO : ScriptableObject
{
    #region 인스펙터

    [Header("세트 정보")]
    [Min(1)]
    [SerializeField] private int _tier = 1;
    [SerializeField] private string _setName;

    [Header("검 공격 데이터")]
    [SerializeField] private CWeaponDataSO _swordData;
    [SerializeField] private CWeaponAttackDataSO _specialAttackData;

    [Header("특수공격")]
    [Min(1)]
    [SerializeField] private int _specialManaCost = 15;

    [Header("외형")]
    [SerializeField] private Sprite _swordSprite;
    [SerializeField] private Sprite _shieldSprite;

    [Header("무기 추가 능력치")]
    [SerializeField] private SWeaponStatBonus _statBonus;       

    [Header("방패 설정")]
    [Min(1)]
    [SerializeField] private int _guardManaCost = 10;

    [Min(0f)]
    [SerializeField] private float _perfectGuardWindow = 0.15f;

    [Min(0f)]
    [SerializeField] private float _guardBreakDuration = 10f;

    #endregion

    #region 프로퍼티

    public int Tier => _tier;
    public string SetName => _setName;
    public CWeaponDataSO SwordData => _swordData;
    public CWeaponAttackDataSO SpecialAttackData => _specialAttackData;
    public int SpecialManaCost => _specialManaCost;
    public Sprite SwordSprite => _swordSprite;
    public Sprite ShieldSprite => _shieldSprite;
    public int GuardManaCost => _guardManaCost;
    public float PerfectGuardWindow => _perfectGuardWindow;
    public float GuardBreakDuration => _guardBreakDuration;
    public SWeaponStatBonus StatBonus => _statBonus;
    #endregion
}
