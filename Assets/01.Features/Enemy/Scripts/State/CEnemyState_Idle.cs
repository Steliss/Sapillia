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
        CLog.Log("Idle진입");
    }

    public void StateExit()
    {
        CLog.Log("Idle퇴장");
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
