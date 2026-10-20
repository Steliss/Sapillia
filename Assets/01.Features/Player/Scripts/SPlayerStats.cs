using System;
using UnityEngine;

public enum EAttribute
{
    Physical,
    Fire,
    Ice,
    Lightning
}

[Serializable]
public struct SPlayerStats
{
    #region SpaceDash
    [Header("Dash")]
    [Min(0f)] public float DashSpeed;
    [Min(0f)] public float DashDuration;
    [Min(0f)] public float DashCooldown;
    [Min(1f)] public int DashCount;
    [Min(0f)] public float DashRegainSpeed;

    #endregion

    #region 체력 / 마나 / 마나 회복
    [Min(1)] public int MaxHP;
    [Min(1)] public int MaxMP;
    [Min(0f)] public float MP_Regin;
    #endregion

    #region 공격 속성
    // 속성 스탯의 자료구조를 결정해야 함.
    // 딕셔너리로 관리할지, 현재처럼 int 필드로 각각 관리할지 검토.

    [Min(0)] public int Attribute_Physical;
    [Min(0)] public int Attribute_Fire;
    [Min(0)] public int Attribute_Ice;
    [Min(0)] public int Attribute_Lightning;
    #endregion

    #region 능력치
    [Min(0)] public int Def;
    [Min(0)] public int Str;
    public int Evasion;

    #endregion

    #region 치명타
    [Min(0f)] public float Critical_Rate;
    [Min(0f)] public float Critical_Damage;
    #endregion

    #region 속도 관련
    public float Attack_Speed;
    public float Movement_Speed;
    [Min(1f)] public float Run_SpeedMulitplier;
    #endregion

    #region 상점 및 운
    [Min(0)] public int Bargain;
    [Min(0f)] public int Luck;
    #endregion

    #region 경험치 및 성장
    [Min(0f)] public int RequiredEXP;
    [Min(0f)] public float PickUpMoney_Rate;
    [Min(0f)] public float Levelup_Heal;
    #endregion

    #region 데미지 관련
    [Min(0f)] public float Damage_All;
    [Min(0)] public int Damage_Fixed;
    [Range(0f, 100f)] public float Damage_Break;
    [Range(0f, 100f)] public float Damage_Reflect;
    #endregion

    #region 공격별 데미지
    [Min(0f)] public float Damage_Weapon;
    [Min(0f)] public float Damage_Normal;
    [Min(0f)] public float Damage_Dash;
    [Min(0f)] public float Damage_Special;
    #endregion

    #region 흡수
    [Min(0)] public int Drain_HP;
    [Min(0)] public int Drain_MP;
    #endregion

    #region 총기인 경우만 추가
    [Min(1)] public int Ammo_Cap;
    [Min(1)] public int Ammo_Count;
    [Min(1)] public float Ammo_Speed;
    [Min(1)] public float Ammo_ReloadSpeed;
    #endregion

    // 속성 공격력 가져오기
    public int GetAttributePower(EAttribute attribute)
    {
        switch (attribute)
        {
            case EAttribute.Physical:
                return Attribute_Physical;

            case EAttribute.Fire:
                return Attribute_Fire;

            case EAttribute.Ice:
                return Attribute_Ice;

            case EAttribute.Lightning:
                return Attribute_Lightning;

            default:
                return 0;
        }
    }

    // 속성치 추가
    public void AddAttributePower(EAttribute attribute, int amount)
    {
        switch (attribute)
        {
            case EAttribute.Physical:
                Attribute_Physical = Mathf.Max(0, Attribute_Physical + amount);
                break;

            case EAttribute.Fire:
                Attribute_Fire = Mathf.Max(0, Attribute_Fire + amount);
                break;

            case EAttribute.Ice:
                Attribute_Ice = Mathf.Max(0, Attribute_Ice + amount);
                break;

            case EAttribute.Lightning:
                Attribute_Lightning = Mathf.Max(0, Attribute_Lightning + amount);
                break;
        }
    }

    // 초기화 및 최소값 보정
    public void Validate()
    {
        DashSpeed = Mathf.Max(0f, DashSpeed);
        DashDuration = Mathf.Max(0f, DashDuration);
        DashCooldown = Mathf.Max(0f, DashCooldown);
        DashCount = Mathf.Max(1, DashCount);
        DashRegainSpeed = Mathf.Max(0f, DashRegainSpeed);

        MaxHP = Mathf.Max(1, MaxHP);
        MaxMP = Mathf.Max(1, MaxMP);
        MP_Regin = Mathf.Max(0f, MP_Regin);

        Attribute_Physical = Mathf.Max(0, Attribute_Physical);
        Attribute_Fire = Mathf.Max(0, Attribute_Fire);
        Attribute_Ice = Mathf.Max(0, Attribute_Ice);
        Attribute_Lightning = Mathf.Max(0, Attribute_Lightning);

        Def = Mathf.Max(0, Def);
        Str = Mathf.Max(0, Str);
        Evasion = Mathf.Clamp(Evasion, 0, 100);

        Critical_Rate = Mathf.Clamp(Critical_Rate, 0f, 100f);
        Critical_Damage = Mathf.Max(0f, Critical_Damage);

        Attack_Speed = Mathf.Max(0.01f, Attack_Speed);
        Movement_Speed = Mathf.Max(0f, Movement_Speed);
        Run_SpeedMulitplier = Mathf.Max(1f, Run_SpeedMulitplier);

        Bargain = Mathf.Max(0, Bargain);
        Luck = Mathf.Max(0, Luck);

        RequiredEXP = Mathf.Max(1, RequiredEXP);
        Levelup_Heal = Mathf.Max(0f, Levelup_Heal);

        Damage_All = Mathf.Max(0f, Damage_All);
        Damage_Fixed = Mathf.Max(0, Damage_Fixed);
        Damage_Break = Mathf.Clamp(Damage_Break, 0f, 100f);
        Damage_Reflect = Mathf.Clamp(Damage_Reflect, 0f, 100f);

        Damage_Weapon = Mathf.Max(0f, Damage_Weapon);
        Damage_Normal = Mathf.Max(0f, Damage_Normal);
        Damage_Dash = Mathf.Max(0f, Damage_Dash);
        Damage_Special = Mathf.Max(0f, Damage_Special);

        Drain_HP = Mathf.Max(0, Drain_HP);
        Drain_MP = Mathf.Max(0, Drain_MP);

        Ammo_Cap = Mathf.Max(0, Ammo_Cap);
        Ammo_Speed = Mathf.Max(0f, Ammo_Speed);
        Ammo_ReloadSpeed = Mathf.Max(0f, Ammo_ReloadSpeed);
    }


    // 별도로 기본값을 설정하지 않은 플레이어에게 적용하는 기본 스탯
    public static SPlayerStats CreateDefault()
    {
        return new SPlayerStats
        {
            DashSpeed = 12f,
            DashDuration = 0.2f,
            DashCooldown = 0.5f,
            DashCount = 2,
            DashRegainSpeed = 0.5f,

            MaxHP = 80,
            MaxMP = 40,
            MP_Regin = 6f,

            Attribute_Physical = 20,
            Attribute_Fire = 20,
            Attribute_Ice = 20,
            Attribute_Lightning = 20,

            Def = 0,
            Str = 0,
            Evasion = 0,

            Critical_Rate = 0f,
            Critical_Damage = 50f,

            Attack_Speed = 1f,
            Movement_Speed = 3f,
            Run_SpeedMulitplier = 1.6f,

            Bargain = 0,
            Luck = 0,

            RequiredEXP = 100,
            Levelup_Heal = 9f,

            Damage_All = 1f,
            Damage_Fixed = 0,
            Damage_Break = 0f,
            Damage_Reflect = 0f,

            Damage_Weapon = 1f,
            Damage_Normal = 1f,
            Damage_Dash = 1f,
            Damage_Special = 1f,

            Drain_HP = 0,
            Drain_MP = 0,

            Ammo_Cap = 0,
            Ammo_Speed = 0f,
            Ammo_ReloadSpeed = 0f
        };
    }

}
