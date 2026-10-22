using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/State_Attack", fileName = "Enemy_Attack_")]
public class CEnemyStateSO_Attack : ScriptableObject, IEnemyState
{
    #region 인스펙터
    [Header("선딜, 공격시간, 후딜")]
    [SerializeField] private float _beforeDelay = 2f;
    [SerializeField] private float _attackTime = 1f;
    [SerializeField] private float _afterDelay = 0.5f;
    [Header("공격 이펙트, 전조 이펙트")]
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _warningLine;
    [Header("공격시 돌진 거리")]
    [SerializeField] private float _rushDistanse = 5f;
    #endregion

    #region 내부 변수
    private CStateMashine _mashine;
    private List<GameObject> _gameObjects = new List<GameObject>();
    #endregion

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

                line3.transform.position = 
                Vector2.MoveTowards(linePos, line1.transform.position, t);

                line4.transform.position = 
                Vector2.MoveTowards(linePos, line2.transform.position, t);

                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }


            // 공격
            _mashine.Ani.Play(_mashine.HashAttack);
            _mashine.Rigidbody.AddForce(dir * _rushDistanse, ForceMode2D.Impulse);
            

            GameObject go = CreateAttackObject
            (_projectile, _afterDelay + _afterDelay, myPos + dir.normalized, angle);

            if (go != null)
            {
                go.transform.parent = _mashine.gameObject.transform;
                CMeleeProjectile goPro = go.GetComponent<CMeleeProjectile>();
                if (goPro != null)
                {
                    goPro.DamageSet(_mashine.Attack, dir);
                }


                // 공격시간
                await UniTask.Delay(TimeSpan.FromSeconds(_attackTime), false, PlayerLoopTiming.Update, ct);
                goPro.AniEnd();
            }


            // 후딜레이
            await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay), false, PlayerLoopTiming.Update, ct);

            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public void StateExit()
    {
        _mashine.Uni.Cancel();

        // 생성한 오브젝트들 모두 꺼주기
        for (int i = 0; i < _gameObjects.Count; i++)
        {
            _gameObjects[i].SetActive(false);
        }
        _gameObjects.Clear();
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
            _gameObjects.Add(go);
        }

        return go;
    }

}
