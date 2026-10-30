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
    [SerializeField] private float _projectileLifeTime = 1.5f;

    [Min(0.1f)]
    [SerializeField] private float _projectileSpeed = 12f;

    [Header("스프라이트")]
    [SerializeField] private Sprite _crossbowSprite;

    [Header("무기 추가 능력치")]
    [SerializeField] private SWeaponStatBonus _statBonus;

    #endregion

    #region 프로퍼티

    public int Tier => _tier;
    public string WeaponName => _weaponName;

    public int BaseDamage => _baseDamage;
    public float AttackInterval => _attackInterval;

    public GameObject ProjectilePrefab => _projectilePrefab;
    public float ProjectileSpeed => _projectileSpeed;
    public float ProjectileLifetime => _projectileLifeTime;
    public Sprite CrossbowSprite => _crossbowSprite;
    public SWeaponStatBonus StatBonus => _statBonus;

    #endregion
}