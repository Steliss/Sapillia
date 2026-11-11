using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/UpgradeOption", fileName = "WeaponUpgradeOption")]
public class CWeaponUpgradeOptionSO : ScriptableObject
{
    [Header("기본 정보")]
    [SerializeField] private EWeaponType _weaponType;
    [SerializeField] private int _fromTier = 1;
    [SerializeField] private int _toTier = 2;

    [Header("UI 표시")]
    [SerializeField] private string _displayName;
    [TextArea(2, 5)]
    [SerializeField] private string _description;
    [SerializeField] private Sprite _icon;

    [Header("실제 장착 데이터")]
    [SerializeField] private CSwordShieldSetSO _swordShieldSet;
    [SerializeField] private CCrossbowDataSO _crossbowData;

    public EWeaponType WeaponType => _weaponType;
    public int FromTier => _fromTier;
    public int ToTier => _toTier;
    public string DisplayName => _displayName;
    public string Description => _description;
    public Sprite Icon => _icon;

    public CSwordShieldSetSO SwordShieldSet => _swordShieldSet;
    public CCrossbowDataSO CrossbowData => _crossbowData;

    public bool IsMatch(EWeaponType currentType, int currentTier)
    {
        return _weaponType == currentType && _fromTier == currentTier;
    }
}