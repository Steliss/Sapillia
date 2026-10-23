using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactData_", menuName = "SO/Artifact/ArtifactData")]
public class CArtifactDataSO : ScriptableObject
{
    #region 인스펙터
    [Header("기본정보")]
    [SerializeField] private string _artifactID;
    [SerializeField] private string _artifactName;
    [SerializeField] private bool _unique;
    [SerializeField] private string _description;
    [SerializeField] private EArtifactGrade _grade;
    [SerializeField] private string _story;
    [SerializeField] private Sprite _icon;

    [Header("시너지")]
    [SerializeField] private EArtifactSynergy _synergy;

    [Header("능력치")]
    [Min(0)]
    [SerializeField] private int _maxStars;

    [SerializeField] private List<SArtifactLevelData> _starStats = new List<SArtifactLevelData>();

    #endregion

    #region 프로퍼티
    public string ArtifactID => _artifactID;
    public string ArtifactName => _artifactName;
    public string Description => _description;
    public bool Unique => _unique;
    public EArtifactGrade Grade => _grade;
    public string Story => _story;
    public Sprite Icon => _icon;
    public int MaxStars => _maxStars;

    public EArtifactSynergy Synergy => _synergy;
    public IReadOnlyList<SArtifactLevelData> StarStats => _starStats;

    #endregion








    public SPlayerStats GetStats(int star)
    {
        if(star < 0 || star > _maxStars)
        {
            return default;
        }


        foreach (SArtifactLevelData data in _starStats)
        {
            if(data.Level == star)
            {
                return data.playerStats;
            }


        }

        Debug.Log("데이터 없음");
        return default;

    }


}
