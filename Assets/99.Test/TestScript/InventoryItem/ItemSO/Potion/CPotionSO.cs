using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Potion_", menuName = "SO/Item/Potion")]
public class CPotionSO : CItemSO
{
    [SerializeField, Min(1)] private int _maxStack = 5;
    [SerializeField] private List<CItemEffectSO> _effects;

    public override EItemType Type => EItemType.Potion;
    public IReadOnlyList<CItemEffectSO> Effects => _effects;
    public int MaxStack => _maxStack;

    public override object[] GetDescriptionValues()
    {
        List<object> values = new();

        foreach (CItemEffectSO effect in _effects)
        {
            values.AddRange(effect.GetDescriptionValues());
        }

        return values.ToArray();
    }

}
