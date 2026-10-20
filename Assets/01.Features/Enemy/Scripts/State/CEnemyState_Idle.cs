using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyState_Idle : IEnemyState
{
    private CStateMashine _mashine;
    private float _idleTime = 3f;
    private CUniTaskOnes _uni;

    public CEnemyState_Idle(CStateMashine mashine)
    {
        _mashine = mashine;
        _uni = new CUniTaskOnes();
    }


    public void StateEnter()
    {
        _uni.Cancel();
        _uni.Run(async ct =>
        {
            _mashine.Ani.Play(_mashine.HashIdle);
            await UniTask.Delay(TimeSpan.FromSeconds(_idleTime));

            _mashine.StateChanged(_mashine.MoveState);
        });
    }

    public void StateExit()
    {
        _uni.Cancel();
    }

    public void StateUpdate()
    {

    }

}
