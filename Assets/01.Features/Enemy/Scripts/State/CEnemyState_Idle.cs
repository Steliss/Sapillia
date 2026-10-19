using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyState_Idle : IEnemyState
{
    private CStateMashine _mashine;
    private float _idleTime = 0f;
    private float _idleTimeMax = 3f;

    public CEnemyState_Idle(CStateMashine mashine)
    {
        _mashine = mashine;
    }


    public void StateEnter()
    {
        _mashine.Ani.Play(_mashine.HashIdle);
    }

    public void StateExit()
    {

    }

    public void StateUpdate()
    {
        _idleTime += Time.deltaTime;
        if (_idleTime >= _idleTimeMax)
        {
            _idleTime = 0f;
            _mashine.StateChanged(_mashine.MoveState);
        }
    }
}
