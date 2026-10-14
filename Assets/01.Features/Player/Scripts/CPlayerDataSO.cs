using System.Collections;
using System.Collections.Generic;
using UnityEngine;




[CreateAssetMenu(menuName = "SO/Player/PlayerData", fileName = "PlayerStats")]
public class CPlayerDataSO : ScriptableObject
{
    #region ÀÎ½ºÆåÅÍ
    private SPlayerStats _baseStats = new SPlayerStats
    {
        DashSpeed = 12f,
        DashDuration = 0.2f,
        DashCooldown = 0.5f,
        DashCount = 2,

        MaxHP = 80,
        MaxMP = 40,
        MP_Regin = 6,

        Attribute_Physical = 20,
        Attribute_Fire = 20,
        Attribute_Ice = 20,
        Attribute_Lightning = 20,

        Def = 0,
        Str = 0,
        Evasion = 0,

        Critical_Rate = 0f,
        Critical_Damage = 50f,

        Attack_Speed = 100f,
        Movement_Speed = 100f,
        Dash_RegainSpeed = 100f,

        Bargain = 0,
        Luck = 0,

        EXP = 100f,
        Money = 100f,
        Levelup_Heal = 9f,

        Damage_All = 100f,
        Damage_Fixed = 0,
        Damage_Break = 0f,
        Damage_Reflect = 0f,

        Damage_Weapon = 100f,
        Damage_Normal = 100f,
        Damage_Dash = 100f,
        Damage_Special = 100f,

        Drain_HP = 0,
        Drain_MP = 0
    };

    #endregion

    public SPlayerStats BaseStats => _baseStats;
}
