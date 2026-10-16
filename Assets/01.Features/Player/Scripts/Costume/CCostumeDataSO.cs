using UnityEngine;

[CreateAssetMenu(menuName = "SO/Player/Costume", fileName = "Costum_")]
public class CCostumeDataSO : ScriptableObject
{
    #region 인스펙터
    [Header("식별 정보")]
    [SerializeField] private string _costumeID;
    [SerializeField] private string _costumeName;

    // 코스튬 설명
    [TextArea]
    [SerializeField] private float _description;

    [Header("이미지 정보")]
    [SerializeField] private Sprite _icon;

    [SerializeField] private AnimatorOverrideController _animatorOverride;

    [Header("Stat Bonus")]
    [SerializeField] private SCostumeStatBonus _bonus;
    #endregion

    #region 프로퍼티
    public string CostumID => _costumeID;
    public string CostumeName => _costumeName;
    public float Description => _description;

    public Sprite Icon => _icon;

    public AnimatorOverrideController AnimatorOverride => _animatorOverride;

    public SCostumeStatBonus Bonus => _bonus;
    #endregion
}