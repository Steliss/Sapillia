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
    /// 현재 강화 레벨을 기준으로 아티팩트 설명에 사용할 수치 반환
    /// </summary>
    public object[] GetDescriptionValues(int totalLevel)
    {
        SPlayerStats stats = GetStats(totalLevel);

        object[] values = new object[_descriptionStats.Count];

        for (int i = 0; i < _descriptionStats.Count; i++)
        {
            EArtifactDescriptionStat statType = _descriptionStats[i];

            float value = 0f;

            switch (statType)
            {
                case EArtifactDescriptionStat.Attribute_Physical:
                    value = stats.Attribute_Physical;
                    break;

                case EArtifactDescriptionStat.Damage_Special:
                    value = stats.Damage_Special * 100f;
                    break;
            }

            values[i] = Mathf.FloorToInt(value + 0.0001f);
        }

        return values;
    }

}