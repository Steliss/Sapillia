using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CPlayerStatsCalculator
{
    public static SPlayerStats Multiply(SPlayerStats stats, float multiplier)
    {
        SPlayerStats result = stats;

        #region SpaceDash

        result.DashSpeed = stats.DashSpeed * multiplier;
        result.DashDuration = stats.DashDuration * multiplier;
        result.DashCooldown = stats.DashCooldown * multiplier;
        result.DashCount = Mathf.RoundToInt(stats.DashCount * multiplier);
        result.DashRegainSpeed = stats.DashRegainSpeed * multiplier;

        #endregion

        #region 체력 / 마나 / 마나 회복

        result.MaxHP = Mathf.RoundToInt(stats.MaxHP * multiplier);
        result.MaxMP = Mathf.RoundToInt(stats.MaxMP * multiplier);
        result.MP_Regin = stats.MP_Regin * multiplier;

        #endregion

        #region 공격 속성

        result.Attribute_Physical = Mathf.RoundToInt(stats.Attribute_Physical * multiplier);
        result.Attribute_Fire = Mathf.RoundToInt(stats.Attribute_Fire * multiplier);
        result.Attribute_Ice = Mathf.RoundToInt(stats.Attribute_Ice * multiplier);
        result.Attribute_Lightning = Mathf.RoundToInt(stats.Attribute_Lightning * multiplier);

        #endregion

        #region 능력치

        result.Def = Mathf.RoundToInt(stats.Def * multiplier);
        result.Str = Mathf.RoundToInt(stats.Str * multiplier);
        result.Evasion = Mathf.RoundToInt(stats.Evasion * multiplier);

        #endregion

        #region 치명타

        result.Critical_Rate = stats.Critical_Rate * multiplier;
        result.Critical_Damage = stats.Critical_Damage * multiplier;

        #endregion

        #region 속도 관련

        result.Attack_Speed = stats.Attack_Speed * multiplier;
        result.Movement_Speed = stats.Movement_Speed * multiplier;
        result.Run_SpeedMulitplier = stats.Run_SpeedMulitplier * multiplier;

        #endregion

        #region 상점 및 운

        result.Bargain = Mathf.RoundToInt(stats.Bargain * multiplier);
        result.Luck = Mathf.RoundToInt(stats.Luck * multiplier);

        #endregion

        #region 경험치 및 성장

        result.RequiredEXP = Mathf.RoundToInt(stats.RequiredEXP * multiplier);
        result.PickUpMoney_Rate = stats.PickUpMoney_Rate * multiplier;
        result.Levelup_Heal = stats.Levelup_Heal * multiplier;

        #endregion

        #region 데미지 관련

        result.Damage_All = stats.Damage_All * multiplier;
        result.Damage_Fixed = Mathf.RoundToInt(stats.Damage_Fixed * multiplier);
        result.Damage_Break = stats.Damage_Break * multiplier;
        result.Damage_Reflect = stats.Damage_Reflect * multiplier;

        #endregion

        #region 공격별 데미지

        result.Damage_Weapon = stats.Damage_Weapon * multiplier;
        result.Damage_Normal = stats.Damage_Normal * multiplier;
        result.Damage_Dash = stats.Damage_Dash * multiplier;
        result.Damage_Special = stats.Damage_Special * multiplier;

        #endregion

        #region 흡수

        result.Drain_HP = Mathf.RoundToInt(stats.Drain_HP * multiplier);
        result.Drain_MP = Mathf.RoundToInt(stats.Drain_MP * multiplier);

        #endregion

        #region 총기인 경우만 추가

        result.Ammo_Cap = Mathf.RoundToInt(stats.Ammo_Cap * multiplier);
        result.Ammo_Count = Mathf.RoundToInt(stats.Ammo_Count * multiplier);
        result.Ammo_Speed = stats.Ammo_Speed * multiplier;
        result.Ammo_ReloadSpeed = stats.Ammo_ReloadSpeed * multiplier;

        #endregion

        return result;
    }




    public static SPlayerStats Add(SPlayerStats a, SPlayerStats b)
    {
        SPlayerStats result = a;

        #region SpaceDash

        result.DashSpeed = a.DashSpeed + b.DashSpeed;
        result.DashDuration = a.DashDuration + b.DashDuration;
        result.DashCooldown = a.DashCooldown + b.DashCooldown;
        result.DashCount = a.DashCount + b.DashCount;
        result.DashRegainSpeed = a.DashRegainSpeed + b.DashRegainSpeed;

        #endregion

        #region 체력 / 마나 / 마나 회복

        result.MaxHP = a.MaxHP + b.MaxHP;
        result.MaxMP = a.MaxMP + b.MaxMP;
        result.MP_Regin = a.MP_Regin + b.MP_Regin;

        #endregion

        #region 공격 속성

        result.Attribute_Physical = a.Attribute_Physical + b.Attribute_Physical;
        result.Attribute_Fire = a.Attribute_Fire + b.Attribute_Fire;
        result.Attribute_Ice = a.Attribute_Ice + b.Attribute_Ice;
        result.Attribute_Lightning = a.Attribute_Lightning + b.Attribute_Lightning;

        #endregion

        #region 능력치

        result.Def = a.Def + b.Def;
        result.Str = a.Str + b.Str;
        result.Evasion = a.Evasion + b.Evasion;

        #endregion

        #region 치명타

        result.Critical_Rate = a.Critical_Rate + b.Critical_Rate;
        result.Critical_Damage = a.Critical_Damage + b.Critical_Damage;

        #endregion

        #region 속도 관련

        result.Attack_Speed = a.Attack_Speed + b.Attack_Speed;
        result.Movement_Speed = a.Movement_Speed + b.Movement_Speed;
        result.Run_SpeedMulitplier = a.Run_SpeedMulitplier + b.Run_SpeedMulitplier;

        #endregion

        #region 상점 및 운

        result.Bargain = a.Bargain + b.Bargain;
        result.Luck = a.Luck + b.Luck;

        #endregion

        #region 경험치 및 성장

        result.RequiredEXP = a.RequiredEXP + b.RequiredEXP;
        result.PickUpMoney_Rate = a.PickUpMoney_Rate + b.PickUpMoney_Rate;
        result.Levelup_Heal = a.Levelup_Heal + b.Levelup_Heal;

        #endregion

        #region 데미지 관련

        result.Damage_All = a.Damage_All + b.Damage_All;
        result.Damage_Fixed = a.Damage_Fixed + b.Damage_Fixed;
        result.Damage_Break = a.Damage_Break + b.Damage_Break;
        result.Damage_Reflect = a.Damage_Reflect + b.Damage_Reflect;

        #endregion

        #region 공격별 데미지

        result.Damage_Weapon = a.Damage_Weapon + b.Damage_Weapon;
        result.Damage_Normal = a.Damage_Normal + b.Damage_Normal;
        result.Damage_Dash = a.Damage_Dash + b.Damage_Dash;
        result.Damage_Special = a.Damage_Special + b.Damage_Special;

        #endregion

        #region 흡수

        result.Drain_HP = a.Drain_HP + b.Drain_HP;
        result.Drain_MP = a.Drain_MP + b.Drain_MP;

        #endregion

        #region 총기인 경우만 추가

        result.Ammo_Cap = a.Ammo_Cap + b.Ammo_Cap;
        result.Ammo_Count = a.Ammo_Count + b.Ammo_Count;
        result.Ammo_Speed = a.Ammo_Speed + b.Ammo_Speed;
        result.Ammo_ReloadSpeed = a.Ammo_ReloadSpeed + b.Ammo_ReloadSpeed;

        #endregion

        return result;
    }

}