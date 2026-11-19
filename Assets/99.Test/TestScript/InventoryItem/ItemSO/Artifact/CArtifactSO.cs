using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Artifact_", menuName = "SO/Item/Artifact")]
public class CArtifactSO : CItemSO
{
    #region 인스펙터
    [Header("아티팩트 정보")]
    [Min(0)]
    [SerializeField] private int _maxStars;

    [Header("시너지")]
    [SerializeField] private List<EArtifactSynergy> _synergies = new();

    [Header("기본스탯")]
    [SerializeField] private SPlayerStats _baseStats;

    [Header("인챈트당 증가수치")]
    [Min(0f)]
    [SerializeField] private float _starIncreaseRate = 0.5f;

    [Header("인챈트수치")]
    [SerializeField] private int _inchant = 0;

    [Header("설명에 표시할 능력치")]
    [SerializeField] private List<EArtifactDescriptionStat> _descriptionStats = new();

    #endregion

    #region 프로퍼티

    public override EItemType Type => EItemType.Artifact;

    public int MaxStars => _maxStars;

    public int Inchant => _inchant;
    public IReadOnlyList<EArtifactSynergy> Synergies => _synergies;

    public SPlayerStats BaseStats => _baseStats;
    public float StarIncreaseRate => _starIncreaseRate;

    public IReadOnlyList<EArtifactDescriptionStat> DescriptionStats => _descriptionStats;
    #endregion


    public SPlayerStats GetStats(int stars)
    {
        // 최대 강화수치 → 아티팩트의 최대 별개수
        stars = Mathf.Clamp(stars, 0, _maxStars);

        float multiplier = 1f + stars * _starIncreaseRate;

        // 계산된 능력치 반환
        return CPlayerStatsCalculator.Multiply(_baseStats, multiplier);
    }




    /// <summary>
    /// 현재 강화 레벨에 따른 아티팩트 설명 수치 반환
    /// </summary>
    public object[] GetDescriptionValues(int totalLevel)
    {
        SPlayerStats stats = GetStats(totalLevel);

        object[] values = new object[_descriptionStats.Count];

        for (int i = 0; i < _descriptionStats.Count; i++)
        {
            EArtifactDescriptionStat statType = _descriptionStats[i];

            float value = 0f;
            bool isPercent = false;

            switch (statType)
            {
                case EArtifactDescriptionStat.MaxHP:
                    value = stats.MaxHP;
                    break;

                case EArtifactDescriptionStat.MaxMP:
                    value = stats.MaxMP;
                    break;

                case EArtifactDescriptionStat.MP_Regin:
                    value = stats.MP_Regin;
                    break;

                case EArtifactDescriptionStat.Attribute_Physical:
                    value = stats.Attribute_Physical;
                    break;

                case EArtifactDescriptionStat.Attribute_Fire:
                    value = stats.Attribute_Fire;
                    break;

                case EArtifactDescriptionStat.Attribute_Ice:
                    value = stats.Attribute_Ice;
                    break;

                case EArtifactDescriptionStat.Attribute_Lightning:
                    value = stats.Attribute_Lightning;
                    break;

                case EArtifactDescriptionStat.Def:
                    value = stats.Def;
                    break;

                case EArtifactDescriptionStat.Str:
                    value = stats.Str;
                    break;

                case EArtifactDescriptionStat.Evasion:
                    value = stats.Evasion;
                    break;

                case EArtifactDescriptionStat.Critical_Rate:
                    value = stats.Critical_Rate;
                    break;

                case EArtifactDescriptionStat.Critical_Damage:
                    value = stats.Critical_Damage;
                    break;

                case EArtifactDescriptionStat.Attack_Speed:
                    value = stats.Attack_Speed;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.Movement_Speed:
                    value = stats.Movement_Speed;
                    break;

                case EArtifactDescriptionStat.Run_SpeedMulitplier:
                    value = stats.Run_SpeedMulitplier;
                    break;

                case EArtifactDescriptionStat.Bargain:
                    value = stats.Bargain;
                    break;

                case EArtifactDescriptionStat.Luck:
                    value = stats.Luck;
                    break;

                case EArtifactDescriptionStat.RequiredEXP:
                    value = stats.RequiredEXP;
                    break;

                case EArtifactDescriptionStat.EXP_multiplier:
                    value = stats.EXP_multiplier;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.PickUpMoney_Rate:
                    value = stats.PickUpMoney_Rate;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.Levelup_Heal:
                    value = stats.Levelup_Heal;
                    break;

                case EArtifactDescriptionStat.Damage_All:
                    value = stats.Damage_All;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.Damage_Fixed:
                    value = stats.Damage_Fixed;
                    break;

                case EArtifactDescriptionStat.Damage_Break:
                    value = stats.Damage_Break;
                    break;

                case EArtifactDescriptionStat.Damage_Reflect:
                    value = stats.Damage_Reflect;
                    break;

                case EArtifactDescriptionStat.Damage_Weapon:
                    value = stats.Damage_Weapon;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.Damage_Normal:
                    value = stats.Damage_Normal;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.Damage_Dash:
                    value = stats.Damage_Dash;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.Damage_Special:
                    value = stats.Damage_Special;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.Drain_HP:
                    value = stats.Drain_HP;
                    break;

                case EArtifactDescriptionStat.Drain_MP:
                    value = stats.Drain_MP;
                    break;

                case EArtifactDescriptionStat.DashSpeed:
                    value = stats.DashSpeed;
                    break;

                case EArtifactDescriptionStat.DashDuration:
                    value = stats.DashDuration;
                    break;

                case EArtifactDescriptionStat.DashCooldown:
                    value = stats.DashCooldown;
                    break;

                case EArtifactDescriptionStat.DashCount:
                    value = stats.DashCount;
                    break;

                case EArtifactDescriptionStat.DashRegainSpeed:
                    value = stats.DashRegainSpeed;
                    isPercent = true;
                    break;

                case EArtifactDescriptionStat.Ammo_Cap:
                    value = stats.Ammo_Cap;
                    break;

                case EArtifactDescriptionStat.Ammo_Count:
                    value = stats.Ammo_Count;
                    break;

                case EArtifactDescriptionStat.Ammo_Speed:
                    value = stats.Ammo_Speed;
                    break;

                case EArtifactDescriptionStat.Ammo_ReloadSpeed:
                    value = stats.Ammo_ReloadSpeed;
                    break;


                default:
                    Debug.LogWarning($"설명용 능력치가 지정되지 않았습니다: {statType}");
                    break;
            }

            if (isPercent)
            {
                value *= 100f;
            }

            values[i] = Mathf.FloorToInt(value + 0.0001f);
        }

        return values;
    }


}