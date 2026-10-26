using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/SpwanState", fileName = "Enemy_Spwan_Drop_")]
public class CEnemySpawnStateSO_Drop : CEnemySpawnStateSO
{
    [Header("이펙트 프리팹")]
    [SerializeField] private GameObject _stamp;

    private float _startHigh = 15f;
    private CUniTaskOnes _uni = new CUniTaskOnes();

    public override void StateEnter()
    {
        _uni.Cancel();
        _uni.Run(async ct =>
        {
            Transform tr = _mashine.Sprite.transform;
            Vector2 high = Vector3.up * _startHigh;
            tr.localPosition = high;
            float speed = 0f;

            try
            {

                while (tr.localPosition.y > 0)
                {
                    speed += 9.81f * Time.deltaTime;
                    high.y -= speed * Time.deltaTime;

                    tr.localPosition = high;

                    await UniTask.Yield(PlayerLoopTiming.Update, ct);

                }
                tr.localPosition = Vector3.zero;

                // 이펙트 생성
                GameObject go = CObjectPoolManager.Instance.TryGetObjectPool(_stamp, 2f);
                Vector2 goPos = tr.position;
                goPos.y -= 0.4f;
                go.transform.position = goPos;

                // 1초 대기 후 진행
                await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ct);

                
            }
            finally
            {
                tr.localPosition = Vector3.zero;
                _mashine.StateChanged(_mashine.IdleState);
            }
        });
    }

    public override void StateExit()
    {
        _uni.Cancel();
    }

    public override void StateUpdate()
    {
        
    }

}
