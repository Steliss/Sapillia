using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class CItemEffectSO : ScriptableObject
{
    public virtual void OnAttack(CPlayerController player)
    {

    }

    public virtual void PassiveEffect(CPlayerController player)
    {

    }
}
