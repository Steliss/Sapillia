using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

public class CEnemyState_Airborn : IEnemyState
{
    private CStateMashine _mashine;
    private CUniTaskOnes _uni = new CUniTaskOnes();
    private float _airbornTimeMax = 1.5f;
    private float _downTime = 2f;
    private Transform _tr;

    public CEnemyState_Airborn(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        _tr = _mashine.Sprite.transform;
        _uni.Cancel();
        _uni.Run(async ct =>
        {
            // 위로 올라갔다가 떨어지기
            _mashine.Ani.Play(_mashine.HashAir);

            float airbornTime = 0f;
            while (airbornTime < _airbornTimeMax)
            {
                airbornTime += Time.deltaTime;

                float t = airbornTime / _airbornTimeMax;

                float y = Mathf.Sin(t * Mathf.PI);

                Vector3 air = _tr.localPosition;
                air.y = y;
                _tr.localPosition = air;

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            // 바닥에 떨어짐.
            _tr.localPosition = Vector3.zero;
            _mashine.Ani.Play(_mashine.HashDown);

            await UniTask.Delay(TimeSpan.FromSeconds(_downTime), false, PlayerLoopTiming.Update, ct);

            if (_mashine.Hp > 0)
            {
                _mashine.StateChanged(_mashine.MoveState);
            }
            else
            {
                _mashine.StateChanged(_mashine.DeathState);
            }
        });

    }

    public void StateExit()
    {

    }

    public void StateUpdate()
    {
        
    }
}
