using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Effect_", menuName = "SO/Item/Effect/Heal")]
public class CHealEffect : CItemEffectSO
{
    [SerializeField] private bool _flatHeal = false;
    [SerializeField, Min(0f)] private float _amount = 30f;


    public override void Apply(CPlayerGamingDataSO player)
    {
        if (player == null)
        {
            CLog.Log("Effect player Null");
            return;
        }

        if (player == null)
        {
            return;
        }

        int healAmount = _flatHeal
            ? Mathf.RoundToInt(player.Stats.MaxHP * _amount / 100f)
            : Mathf.RoundToInt(_amount);

        player.Heal(healAmount);
    }
}

