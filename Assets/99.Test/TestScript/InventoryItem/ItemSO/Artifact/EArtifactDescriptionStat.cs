
public enum EArtifactDescriptionStat
{
    None = 0,

    // 체력 / 마나
    MaxHP,
    MaxMP,
    MP_Regin,

    // 공격 속성
    Attribute_Physical,
    Attribute_Fire,
    Attribute_Ice,
    Attribute_Lightning,

    // 기본 능력치
    Def,
    Str,
    Evasion,

    // 치명타
    Critical_Rate,
    Critical_Damage,

    // 속도
    Attack_Speed,
    Movement_Speed,
    Run_SpeedMulitplier,

    // 상점 및 성장
    Bargain,
    Luck,
    RequiredEXP,
    EXP_multiplier,
    PickUpMoney_Rate,
    Levelup_Heal,

    // 피해
    Damage_All,
    Damage_Fixed,
    Damage_Break,
    Damage_Reflect,
    Damage_Weapon,
    Damage_Normal,
    Damage_Dash,
    Damage_Special,

    // 흡수
    Drain_HP,
    Drain_MP,

    // 대시
    DashSpeed,
    DashDuration,
    DashCooldown,
    DashCount,
    DashRegainSpeed,

    // 총기
    Ammo_Cap,
    Ammo_Count,
    Ammo_Speed,
    Ammo_ReloadSpeed
}
