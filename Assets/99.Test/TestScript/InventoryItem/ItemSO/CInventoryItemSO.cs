using System.Collections.Generic;
using UnityEngine;


public enum ESynergy
{
    None,
    // 시너지 종류 추가 
}


[CreateAssetMenu(menuName = "SO/Item/Items")]
public class CInventoryItemSO : CItemSO
{
    [SerializeField] private List<CItemEffectSO> _effect;
    [SerializeField] private List<ESynergy> _synergy;

    public IReadOnlyList<ESynergy> Synergy => _synergy;

    public void OnAttackHit(CPlayerController player)
    {
        foreach (CItemEffectSO effect in _effect)
        {
            effect?.OnAttack(player);
        }
    }

    public void PassiveEffect(CPlayerController player)
    {
        foreach (CItemEffectSO effect in _effect)
        {
            effect?.PassiveEffect(player);
        }
    }
}

