using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyState_Idle : IEnemyState
{
    private CStateMashine _mashine;
    private float _idleTime = 3f;

    public CEnemyState_Idle(CStateMashine mashine)
    {
        _mashine = mashine;
    }


    public void StateEnter()
    {
        _mashine.Uni.Cancel();
        _mashine.Uni.Run(async ct =>
        {
            _mashine.Ani.Play(_mashine.HashIdle);
            await UniTask.Delay(TimeSpan.FromSeconds(_idleTime));

            _mashine.StateChanged(_mashine.MoveState);
        });
    }

    public void StateExit()
    {
        _mashine.Uni.Cancel();
    }

    public void StateUpdate()
    {
        _mashine.PatrolSearch();
    }

}
