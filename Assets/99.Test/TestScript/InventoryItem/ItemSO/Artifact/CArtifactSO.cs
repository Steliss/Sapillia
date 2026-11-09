using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Artifact_", menuName = "SO/Item/Artifact")]
public class CArtifactSO : CItemSO
{
    #region 인스펙터

    [Header("아티팩트 정보")]
    [SerializeField] private EArtifactGrade _grade;
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

    #endregion

    #region 프로퍼티

    public override EItemType Type => EItemType.Artifact;

    public EArtifactGrade Grade => _grade;
    public int MaxStars => _maxStars;

    public int Inchant => _inchant;
    public IReadOnlyList<EArtifactSynergy> Synergies => _synergies;

    public SPlayerStats BaseStats => _baseStats;
    public float StarIncreaseRate => _starIncreaseRate;

    #endregion


    public SPlayerStats GetStats(int stars)
    {
        // 최대 강화수치 → 아티팩트의 최대 별개수
        stars = Mathf.Clamp(stars, 0, _maxStars);

        float multiplier = 1f + stars * _starIncreaseRate;

        // 계산된 능력치 반환
        return CPlayerStatsCalculator.Multiply(_baseStats, multiplier);
    }

}