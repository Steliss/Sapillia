using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTownGateNear : CTriggerPlayer
{
    [SerializeField] private GameObject _imsiGate;


    protected override void OnPlayerEnter()
    {
        _imsiGate.SetActive(true);
    }

    protected override void OnPlayerExit()
    {

    }

}
