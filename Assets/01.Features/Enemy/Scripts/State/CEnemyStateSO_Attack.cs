using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/State_Attack", fileName = "Enemy_Attack_")]
public class CEnemyStateSO_Attack : ScriptableObject, IEnemyState
{
    [SerializeField] private float _beforeDelay = 2f;
    [SerializeField] private float _afterDelay = 1f;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private float _rushDistanse = 5f;

    private CStateMashine _mashine;
    private CUniTaskOnes _uni;

    public CEnemyStateSO_Attack(CStateMashine mashine)
    {
        _mashine = mashine;
        _uni = new CUniTaskOnes();
    }

    public void StateEnter()
    {
        _uni.Cancel();
        _uni.Run(async ct =>
        {
            _mashine.Ani.Play(_mashine.HashAttReady);
            await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay));

            // 공격
            _mashine.Ani.Play(_mashine.HashAttack);
            _mashine.Rigidbody.AddForce(Vector2.right * _rushDistanse, ForceMode2D.Impulse);
            

            await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay));
            _mashine.StateChanged(_mashine.IdleState);
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
