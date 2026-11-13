using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "Effect_", menuName = "SO/Item/Effect/Heal")]
public class CHealEffect : CItemEffectSO
{
    [SerializeField] private bool _flatHeal = false;
    [SerializeField, Min(0f)] private float _amount = 30f;


    public override void Apply(CPlayerController player)
    {
        if (player == null)
        {
            CLog.Log("Effect player Null");
            return;
        }

        // 실제 플레이어 데이터 접근 경로에 맞게 변경
        CPlayerGamingDataSO data = player.PlayerData;

        if (data == null)
        {
            return;
        }

        int healAmount = _flatHeal
            ? Mathf.RoundToInt(data.Stats.MaxHP * _amount / 100f)
            : Mathf.RoundToInt(_amount);

        data.Heal(healAmount);
    }
}

