using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyState_Idle : IEnemyState
{
    private CStateMashine _mashine;
    private float _idleTime = 3f;
    private float _time = 0f;

    public CEnemyState_Idle(CStateMashine mashine)
    {
        _mashine = mashine;
    }


    public void StateEnter()
    {
        _time = 0f;
        _idleTime = _mashine.IsPatrol ? 3f : 1.5f;
        _mashine.Ani.Play(_mashine.HashIdle);
    }

    public void StateExit()
    {

    }

    public void StateUpdate()
    {
        _time += Time.deltaTime;
        if (_time > _idleTime)
        {
            _mashine.StateChanged(_mashine.MoveState);
        }
    }

}
