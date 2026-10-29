using System;
using UnityEngine;

[Serializable]
public struct SWeaponStatBonus
{
    [Header("기본 능력치")]
    public int Str;
    public int Def;

    [Header("속성 능력치")]
    public int Attribute_Physical;
    public int Attribute_Fire;
    public int Attribute_Ice;
    public int Attribute_Lightning;

    [Header("공격 능력치")]
    public float Damage_Weapon;    
    public float Attack_Speed;

    [Header("특수 공격 관련")]
    public float Special_Range;
    public float Damage_Special;

    // 기존 SPlayerStats 복사본에 보너스 적용
    public void Apply(ref SPlayerStats stats)
    {
        stats.Str += Str;
        stats.Def += Def;

        stats.Attribute_Physical += Attribute_Physical;
        stats.Attribute_Fire += Attribute_Fire;
        stats.Attribute_Ice += Attribute_Ice;
        stats.Attribute_Lightning += Attribute_Lightning;

        stats.Damage_Weapon += Damage_Weapon;
        stats.Damage_Special += Damage_Special;
        stats.Attack_Speed += Attack_Speed;
    }
}
