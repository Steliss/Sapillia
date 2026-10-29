using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/SpawnState/Summon", fileName = "Enemy_Spawn_Summon_")]
public class CEnemySpawnStateSO_Summon : CEnemySpawnStateSO
{
    [SerializeField] private GameObject _magicCircle;
    [SerializeField] private GameObject _summonEffect;
    [SerializeField] private float _spawnTime = 2f;
    [SerializeField] private float _afterDelay = 1f;

    private int _hashEnd = Animator.StringToHash("End");

    public override void StateEnter()
    {
        if (_magicCircle == null)
        {
            _mashine.StateChanged(_mashine.IdleState);
        }
        _mashine.AttackUni.Run(async ct =>
        {
            await UniTask.Yield(PlayerLoopTiming.Update, ct);

            Vector2 startScale = _mashine.Sprite.transform.localScale;
            _mashine.Sprite.transform.localScale = Vector2.zero;


            GameObject magicCircle = null;
            if (CObjectPoolManager.Instance != null )
            {
                magicCircle = CObjectPoolManager.Instance.
                    GetPoolObject(_magicCircle, _spawnTime + _afterDelay);
                magicCircle.transform.position = _mashine.transform.position;
            }
            
            await UniTask.Delay(TimeSpan.FromSeconds(_spawnTime), cancellationToken: ct);

            if (magicCircle != null)
            {
                Animator ani = magicCircle.GetComponent<Animator>();
                ani.Play(_hashEnd);
            }
            GameObject summonEffect = null;
            if (CObjectPoolManager.Instance != null)
            {
                summonEffect = CObjectPoolManager.Instance.
                    GetPoolObject(_summonEffect, _afterDelay);
                summonEffect.transform.position = _mashine.transform.position;
            }

            _mashine.Sprite.transform.localScale = startScale;

            await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay), cancellationToken: ct);
            _mashine.StateChanged(_mashine.AttackState);
        });
    }

    public override void StateExit()
    {
    }

    public override void StateUpdate()
    {
    }

}
