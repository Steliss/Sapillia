using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Potion_", menuName = "SO/Item/Potion")]
public class CPotionSO : CItemSO
{

    [SerializeField] private EArtifactGrade _grade;
    [SerializeField, Min(1)] private int _maxStack = 5;
    [SerializeField] private List<CItemEffectSO> _effects;

    public EArtifactGrade Grade => _grade;
    public override EItemType Type => EItemType.Potion;
    public IReadOnlyList<CItemEffectSO> Effects => _effects;
    public int MaxStack => _maxStack;   


}
