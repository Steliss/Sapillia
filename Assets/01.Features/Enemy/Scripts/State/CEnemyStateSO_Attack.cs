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
    private CUniTaskOnes _uni = new CUniTaskOnes();

    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        _uni.Cancel();
        _uni.Run(async ct =>
        {
            _mashine.Ani.Play(_mashine.HashAttReady);
            await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay), false, PlayerLoopTiming.Update, ct);

            // 공격
            _mashine.Ani.Play(_mashine.HashAttack);
            _mashine.Rigidbody.AddForce(Vector2.right * _rushDistanse, ForceMode2D.Impulse);

            GameObject go; 

            if (_projectile == null)
            {
                CLog.Log("오브젝트가 없다잖아");

            }


            if (CObjectPoolManager.Instance != null)
            {
                go = CObjectPoolManager.Instance.TryGetObjectPool(_projectile, 3f);
            }
            else
            {
                go = null;
                CLog.Log("풀 인스턴스가 없다자나");
            }

            if (go != null)
            {
                go.transform.position = _mashine.gameObject.transform.position;
            }
            else
            {
                CLog.Log("없다는디요?");
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay), false, PlayerLoopTiming.Update, ct);
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
