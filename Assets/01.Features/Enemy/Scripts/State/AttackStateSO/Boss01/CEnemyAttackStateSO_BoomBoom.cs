using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/BoomBoom", fileName = "Enemy_Attack_BoomBoom_")]
public class CEnemyAttackStateSO_BoomBoom : CEnemyAttackStateSO
{
    #region 내부 구조체
    [Serializable]
    public struct SBulletInfo
    {
        [Header("총알 프리팹")]
        public GameObject Projectile;
        public GameObject Projectile_Drop;
        [Header("총알 수명")]
        public float LifeTime;
    }

    [Serializable]
    public struct SAttackInfo
    {
        [Header("폭탄 뿌릴 범위 지정")]
        public Vector2 RandomRangeStart;
        public Vector2 RandomRangeEnd;
        public float JumpReadyTime;
        public float JumpTime;
        public float BoomBoomTime;
        [Header("총알 발사 쿨타임")]
        public float CoolTime;
        public GameObject JumpEffect;

        public string AniJumpReady;
        public string AniJump;
        public string AniLanding;
        public string AniLandingEnd;

        [Header("해시용 변수. 입력x")]
        [HideInInspector] public int HashJumpReady;
        [HideInInspector] public int HashJump;
        [HideInInspector] public int HashLanding;
        [HideInInspector] public int HashLandingEnd;
    }
    #endregion


    #region 인스펙터
    [SerializeField] private SAttackInfo _attackInfo;
    [SerializeField] private SBulletInfo _bulletInfo;
    #endregion

    private void OnEnable()
    {
        _attackInfo.HashJumpReady = Animator.StringToHash(_attackInfo.AniJumpReady);
        _attackInfo.HashJump = Animator.StringToHash(_attackInfo.AniJump);
        _attackInfo.HashLanding = Animator.StringToHash(_attackInfo.AniLanding);
        _attackInfo.HashLandingEnd = Animator.StringToHash(_attackInfo.AniLandingEnd);
    }

    public override void StateEnter()
    {
        _mashine.AttackUni.Run(async ct =>
        {
            // 점프준비
            _mashine.Ani.Play(_attackInfo.HashJumpReady);
            await UniTask.Delay(TimeSpan.FromSeconds(_attackInfo.JumpReadyTime), cancellationToken: ct);

            // 점프
            Transform myTr = _mashine.transform;
            _mashine.Ani.Play(_attackInfo.HashJump);
            _mashine.Collider.enabled = false;
            _mashine.Nav.enabled = false;
            CreateAttackObject(_attackInfo.JumpEffect, 1f, myTr.position, Quaternion.identity);
            for (int i = 0; i < 3; i++)
            {
                Vector2 myPos = _mashine.transform.position;
                float ranX = UnityEngine.Random.Range(-4f, 4f);
                float ranY = UnityEngine.Random.Range(-3f, 3f);
                Vector2 randomPos = new Vector2(ranX, ranY);
                BoomThrow(_bulletInfo, myPos, myPos + randomPos);
            }


            Transform spTr = _mashine.Sprite.transform;
            for (float time = 0f; time < _attackInfo.JumpTime; time += Time.deltaTime)
            {
                float t = time / _attackInfo.JumpTime;
                float y = Mathf.Lerp(0f, 20f, t);
                Vector2 pos = new Vector2(0f, y);
                spTr.localPosition = pos;
                await UniTask.Yield(ct);
            }

            float cool = 0f;
            for (float time = 0f; time < _attackInfo.BoomBoomTime; time += Time.deltaTime)
            {
                if (cool > _attackInfo.CoolTime)
                {
                    cool = 0f;
                    float ranX = UnityEngine.Random.Range(_attackInfo.RandomRangeStart.x, _attackInfo.RandomRangeEnd.x);
                    float ranY = UnityEngine.Random.Range(_attackInfo.RandomRangeStart.y, _attackInfo.RandomRangeEnd.y);
                    Vector2 randomPos = new Vector2(ranX, ranY);
                    BoomDrop(_bulletInfo, randomPos);
                }
                cool += Time.deltaTime;
                await UniTask.Yield(ct);
            }


            // 공격 종료. 착지시작
            myTr.position = _mashine.Target.position;
            _mashine.Collider.enabled = false;
            _mashine.Ani.Play(_attackInfo.HashLanding);


            for (float time = 0f; time < _attackInfo.JumpTime; time += Time.deltaTime)
            {
                float t = time / _attackInfo.JumpTime;
                float y = Mathf.Lerp(20f, 0f, t);
                Vector2 pos = new Vector2(0f, y);
                spTr.localPosition = pos;
                await UniTask.Yield(ct);
            }
            CreateAttackObject(_attackInfo.JumpEffect, 1f, myTr.position, Quaternion.identity);
            _mashine.Collider.enabled = true;

            _mashine.Ani.Play(_attackInfo.HashLandingEnd);

            await UniTask.Delay(TimeSpan.FromSeconds(_attackInfo.JumpReadyTime), cancellationToken: ct);
            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
        _mashine.Collider.enabled = true;
        _mashine.Nav.enabled = true;
        _mashine.Collider.isTrigger = false;
    }


    private void BoomDrop(SBulletInfo info, Vector2 pos)
    {
        GameObject dynamite = null;
        if (CObjectPoolManager.Instance != null)
        {
            dynamite = CObjectPoolManager.Instance.GetPoolObject(info.Projectile_Drop, info.LifeTime + 1f);
            dynamite.transform.position = pos;
        }


        float lifeTime = info.LifeTime;
        float ranTime = UnityEngine.Random.Range(-lifeTime * 0.1f, lifeTime * 0.1f);
        lifeTime += ranTime;


        // 데미지 설정 및 도착위치 설정
        CEnemyProjectile_DynamiteDrop pro = dynamite.GetComponent<CEnemyProjectile_DynamiteDrop>();
        if (pro != null)
        {
            pro.DamageSet(_mashine.Attack, pos, lifeTime);
        }
    }

    private void BoomThrow(SBulletInfo info, Vector2 fromPos, Vector2 toPos)
    {
        GameObject dynamite = null;
        if (CObjectPoolManager.Instance != null)
        {
            dynamite = CObjectPoolManager.Instance.GetPoolObject(info.Projectile, info.LifeTime + 1f);
            dynamite.transform.position = fromPos;
        }


        float lifeTime = info.LifeTime;
        float ranTime = UnityEngine.Random.Range(-lifeTime * 0.1f, lifeTime * 0.1f);
        lifeTime += ranTime;


        // 데미지 설정 및 도착위치 설정
        CEnemyProjectile_Dynamite pro = dynamite.GetComponent<CEnemyProjectile_Dynamite>();
        if (pro != null)
        {
            pro.DamageSet(_mashine.Attack, toPos, lifeTime);
        }
    }

}
