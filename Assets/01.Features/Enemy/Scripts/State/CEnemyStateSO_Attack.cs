using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/State_Attack", fileName = "Enemy_Attack_")]
public class CEnemyStateSO_Attack : ScriptableObject, IEnemyState
{
    [SerializeField] private float _beforeDelay = 2f;
    [SerializeField] private float _afterDelay = 1f;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _warningLine;
    [SerializeField] private float _rushDistanse = 5f;
    [SerializeField] private float _attackRange = 3f;
    
    private CStateMashine _mashine;

    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        _mashine.Uni.Cancel();
        _mashine.Uni.Run(async ct =>
        {
            // 선딜레이
            _mashine.Nav.velocity = Vector3.zero;
            _mashine.Ani.Play(_mashine.HashAttReady);

            Vector2 myPos = _mashine.transform.position;
            Vector2 tarPos = _mashine.Target.position;

            // 방향
            Vector2 dir = (tarPos - myPos).normalized;
            // 법선
            Vector2 dir90 = new Vector2(-dir.y, dir.x);

            float z = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) - 90f;
            Quaternion angle = Quaternion.Euler(0, 0, z);

            if (dir.x < 0)
            {
                _mashine.Sprite.flipX = true;
            }
            else
            {
                _mashine.Sprite.flipX = false;
            }

            Vector2 linePos = myPos + (dir * _rushDistanse / 2f);

            // 고정된 라인
            GameObject line1 = CreateAttackObject
            (_warningLine, _beforeDelay, linePos + dir90 * 1f, angle);
            GameObject line2 = CreateAttackObject
            (_warningLine, _beforeDelay, linePos + -dir90 * 1f, angle);
            // 움직이는 라인
            GameObject line3 = CreateAttackObject
            (_warningLine, _beforeDelay, linePos, angle);
            GameObject line4 = CreateAttackObject
            (_warningLine, _beforeDelay, linePos, angle);

            Vector3 scale = line1.transform.localScale;
            scale.y = _rushDistanse;

            line1.transform.localScale = scale;
            line2.transform.localScale = scale;
            line3.transform.localScale = scale;
            line4.transform.localScale = scale;
            
            // 라인 점점 벌리기
            float delayTime = 0f;
            while (delayTime < _beforeDelay)
            {
                delayTime += Time.deltaTime;

                float t = Mathf.Lerp(0.3f, 1f, delayTime / _beforeDelay);

                line3.transform.position = Vector2.MoveTowards
                    (
                    linePos, 
                    line1.transform.position, 
                    t
                    );

                line4.transform.position = Vector2.MoveTowards
                    (
                    linePos,
                    line2.transform.position,
                    t
                    );

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }


            // 공격
            _mashine.Ani.Play(_mashine.HashAttack);
            _mashine.Rigidbody.AddForce(dir * _rushDistanse, ForceMode2D.Impulse);
            

            GameObject go = null; 

            if (_projectile != null && CObjectPoolManager.Instance != null)
            {
                go = CObjectPoolManager.Instance.TryGetObjectPool(_projectile, _afterDelay);
                go.transform.localRotation = Quaternion.identity;
                go.transform.rotation = angle;
                go.transform.position = myPos + dir.normalized;
                go.transform.parent = _mashine.gameObject.transform;
            }

            
            // 후딜레이
            await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay), false, PlayerLoopTiming.Update, ct);
            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public void StateExit()
    {
        _mashine.Uni.Cancel();
    }

    public void StateUpdate()
    {
        
    }

    private GameObject CreateAttackObject(GameObject prefeb, float lifeTime, Vector2 pos, Quaternion angle)
    {
        GameObject go = null;

        if (prefeb != null && CObjectPoolManager.Instance != null)
        {
            go = CObjectPoolManager.Instance.TryGetObjectPool(prefeb, lifeTime);
            go.transform.rotation = angle;
            go.transform.position = pos;
        }

        return go;
    }

}
