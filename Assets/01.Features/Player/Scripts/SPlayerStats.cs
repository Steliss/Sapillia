using System;
using UnityEngine;

[Serializable]
public struct SPlayerStats
{	
	#region SpaceDash
	[Header("Dash")]
	[Min(0f)] public float DashSpeed;
    [Min(0f)] public float DashDuration;
    [Min(0f)] public float DashCooldown;
	[Min(1f)] public float DashCount;
	#endregion

	#region 체력 / 마나 / 마나회복
	[Min(1)] public int MaxHP;
	[Min(1)] public int MaxMP;
	[Min(6)] public int MP_Regin;
	#endregion

	#region 공격 속성
	[Min(20)] public int Attribute_Physical;
	[Min(20)] public int Attribute_Fire;
	[Min(20)] public int Attribute_Ice;
	[Min(20)] public int Attribute_Lightning;
	#endregion

	#region 방어
	[Min(0)] public int Def;
	[Min(0)] public int Str;
	[Min(0)] public int Evasion;

	#endregion

	#region 치명타
	[Min(0f)] public float Critical_Rate;
	[Min(50f)] public float Critical_Damage;
	#endregion

	#region 속도관련
	[Min(1f)] public float Attack_Speed;
	[Min(1f)] public float Movement_Speed;
	[Min(1f)] public float Dash_RegainSpeed;
	#endregion

	#region 재화 및 행운
	[Min(0)] public int Bargain;
	[Min(0f)] public int Luck;
	#endregion

	#region 레벨업 및 재화
	[Min(100f)] public float EXP;
	[Min(100f)] public float Money;
	[Min(9f)] public float Levelup_Heal;
	#endregion

	#region 피해
	[Min(1f)] public float Damage_All;
	[Min(0)] public int Damage_Fixed;
	[Min(0f)] public float Damage_Break;
	[Min(0f)] public float Damage_Reflect;
	#endregion

	#region 무기
	[Min(0f)] public float Damage_Weapon;
	[Min(0f)] public float Damage_Normal;
	[Min(0f)] public float Damage_Dash;
	[Min(0f)] public float Damage_Special;
	#endregion

	#region 흡수
	[Min(0)] public int Drain_HP;
	[Min(0)] public int Drain_MP;
	#endregion

	#region 석궁일경우만 추가
	[Min(1)] public int Ammo_Cap;
	[Min(1)] public int Ammo_Count;
	[Min(1)] public int Ammo_Speed;
	[Min(1)] public float Ammo_ReloadSpeed;
	#endregion
}
