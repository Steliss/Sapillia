using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Item/Effect", fileName = "TestEffect_")]
public class CTestEffectSO : CItemEffectSO
{
    public override void OnAttack(CPlayerController player)
    {
        CLog.Log("Test");
    }
}