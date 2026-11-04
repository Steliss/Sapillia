using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/Crossbow", fileName = "CrossbowData" )]
public class CCrossbowDataSO : ScriptableObject
{
    #region 인스펙터

    [Header("석궁 정보")]
    [Min(1)]
    [SerializeField] private int _tier = 1;
    [SerializeField] private string _weaponName;

    [Header("공격 데이터")]
    [Min(0)]
    [SerializeField] private int _baseDamage = 10;

    [Min(0.01f)]
    [SerializeField] private float _attackInterval = 0.5f;

    [Header("Ammo")]
    [SerializeField] private GameObject _projectilePrefab;
    [SerializeField] private float _projectileRange = 5.0f;

    [Header("재장전")]
    [Min(0.01f)]
    [SerializeField] private float _reloadDuration = 2f;

    [Min(0.1f)]
    [SerializeField] private float _projectileSpeed = 12f;

    [Header("스프라이트")]
    [SerializeField] private Sprite _crossbowSprite;

    [Header("무기 추가 능력치")]
    [SerializeField] private SWeaponStatBonus _statBonus;

    [Header("석궁 특수공격")]
    [SerializeField] private GameObject _specialProjectilePrefab;

    [Min(0)]
    [SerializeField] private int _specialManaCost = 10;

    [Min(0.1f)]
    [SerializeField] private float _specialProjectileSpeed = 15f;

    [Min(0.1f)]
    [SerializeField] private float _specialProjectileRange = 10f;

    [Min(0f)]
    [SerializeField] private float _specialCooldown = 0.5f;

    [Header("특수공격 반동")]
    [Min(0f)]
    [SerializeField] private float _specialRecoilSpeed = 8f;

    [Min(0.01f)]
    [SerializeField] private float _specialRecoilDeceleration = 30f;

    #endregion

    #region 프로퍼티

    public int Tier => _tier;
    public string WeaponName => _weaponName;

    public int BaseDamage => _baseDamage;
    public float AttackInterval => _attackInterval;
    public float ReloadDuration => _reloadDuration;
    public GameObject ProjectilePrefab => _projectilePrefab;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileRange => _projectileRange;
    public Sprite CrossbowSprite => _crossbowSprite;
    public SWeaponStatBonus StatBonus => _statBonus;
    public GameObject SpecialProjectilePrefab => _specialProjectilePrefab;
    public int SpecialManaCost => _specialManaCost;
    public float SpecialProjectileSpeed => _specialProjectileSpeed;
    public float SpecialProjectileRange => _specialProjectileRange;
    public float SpecialCooldown => _specialCooldown;
    public float SpecialRecoilSpeed => _specialRecoilSpeed;
    public float SpecialRecoilDeceleration => _specialRecoilDeceleration;
    #endregion
}