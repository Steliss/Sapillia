using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/WeaponData", fileName = "WeaponData")]
public class CWeaponDataSO : ScriptableObject
{
    #region 인스펙터
    [Header("기본 공격력")]
    [Min(0)]
    [SerializeField] private int _baseDamage = 10;

    [Header("기본 콤보 공격")]
    [SerializeField] private CWeaponAttackDataSO[] _normalAttacks = new CWeaponAttackDataSO[3];

    #endregion

    #region 프로퍼티
    public int BaseDamage => _baseDamage;
    public int AttackCount => _normalAttacks != null ? _normalAttacks.Length : 0;

    #endregion

    public CWeaponAttackDataSO GetAttack(int index)
    {
        if (index < 0 || index >= AttackCount)
        {
            return null;
        }           

        return _normalAttacks[index];
    }
}