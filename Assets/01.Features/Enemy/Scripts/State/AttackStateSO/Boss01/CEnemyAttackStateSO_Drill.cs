using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/Drill", fileName = "Enemy_Attack_Drill_")]
public class CEnemyAttackStateSO_Drill : CEnemyAttackStateSO
{
    #region 내부 구조체
    [Serializable]
    public struct SBulletInfo
    {
        [Header("총알 프리팹")]
        [SerializeField] public GameObject Projectile;
        [Header("총알 수명")]
        [SerializeField] public float LifeTime;
        [Header("총알 발사 쿨타임")]
        [SerializeField] public float CoolTime;
        [Header("랜덤 위치 범위")]
        [SerializeField] public Vector2 RandomRangePos;

    }

    [Serializable]
    public struct SAttackInfo
    {
        public float DrillSpeed;
        public Vector2 AttackRange;
        public float JumpReadyTime;
        public float JumpTime;
        public float CenterX;
        public float PlusX;

        public int AttackCount;

        public GameObject DrillEffect;
        public GameObject WallEffect;
        public GameObject JumpEffect;

        public SBulletInfo BulletInfo;


        public string AniJumpReady;
        public string AniJump;
        public string AniDrill;
        public string AniLanding;
        public string AniLandingEnd;

        [Header("해시용 변수. 입력x")]
        [HideInInspector] public int HashJumpReady;
        [HideInInspector] public int HashJump;
        [HideInInspector] public int HashDrill;
        [HideInInspector] public int HashLanding;
        [HideInInspector] public int HashLandingEnd;
    }


    #endregion


    #region 인스펙터
    [SerializeField] private SAttackInfo _attackInfo;
    [SerializeField] private SLineInfo _lineInfo;
    #endregion

    private void OnEnable()
    {
        _attackInfo.HashJumpReady = Animator.StringToHash(_attackInfo.AniJumpReady);
        _attackInfo.HashJump = Animator.StringToHash(_attackInfo.AniJump);
        _attackInfo.HashDrill = Animator.StringToHash(_attackInfo.AniDrill);
        _attackInfo.HashLanding = Animator.StringToHash(_attackInfo.AniLanding);
        _attackInfo.HashLandingEnd = Animator.StringToHash(_attackInfo.AniLandingEnd);
    }

    public override void StateEnter()
    {
        _mashine.AttackUni.Run(async ct =>
        {
            // 점프준비
            _mashine.Ani.Play(_attackInfo.HashJumpReady);
            int RandomLR = UnityEngine.Random.Range(0, 2);
            int[] LR = { -1, 1 };
            _mashine.SpriteFlipX(LR[RandomLR] < 0);
            await UniTask.Delay(TimeSpan.FromSeconds(_attackInfo.JumpReadyTime), cancellationToken: ct);

            // 점프
            Transform myTr = _mashine.transform;
            _mashine.Ani.Play(_attackInfo.HashJump);
            _mashine.Collider.enabled = false;
            _mashine.Nav.enabled = false;
            CreateAttackObject(_attackInfo.JumpEffect, 1f, myTr.position, Quaternion.identity);

            Transform spTr = _mashine.Sprite.transform;
            for (float time = 0f; time < _attackInfo.JumpTime; time += Time.deltaTime)
            {
                float t = time / _attackInfo.JumpTime;
                float y = Mathf.Lerp(0f, 20f, t);
                float x = Mathf.Lerp(0f, 5f * LR[RandomLR], t);
                Vector2 pos = new Vector2(x, y);
                spTr.localPosition = pos;
                await UniTask.Yield(ct);
            }

            Vector2 myPos = myTr.position;
            float startX = _attackInfo.CenterX + (_attackInfo.PlusX * LR[RandomLR]);
            myPos.x = startX;
            myPos.y = _mashine.Target.position.y;
            myTr.position = myPos;


            // 드릴공격 시작 벽 x좌표 11 ~ 36  중앙 23.5f  +-12.5f
            await WarningLine_FireAngle(ct, _lineInfo);

            spTr.localPosition = Vector2.zero;
            _mashine.Collider.enabled = true;
            _mashine.Collider.isTrigger = true;

            // 진행방향
            bool isLeftGo = LR[RandomLR] > 0;
            for (int i = 0; i < _attackInfo.AttackCount; i++)
            {
                _mashine.SpriteFlipX(isLeftGo);
                _mashine.Ani.Play(_attackInfo.HashDrill);
                await DrillAttack(ct, isLeftGo, i == 0);

                isLeftGo = !isLeftGo;
            }

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


    private async UniTask DrillAttack(CancellationToken ct, bool isLeftGo, bool isFirst)
    {
        // 시작위치 설정
        Transform myTr = _mashine.transform;
        int LR = isLeftGo ? -1 : 1;
        Vector2 myPos = myTr.position;

        // 벽 위치, 방향 설정
        float startX = _attackInfo.CenterX + (_attackInfo.PlusX * LR * -1f);
        float endX = _attackInfo.CenterX + _attackInfo.PlusX;
        float endX2 = _attackInfo.CenterX - _attackInfo.PlusX;
        myPos.x = startX;
        myPos.y = _mashine.Target.position.y;
        if (!isFirst)
        {
            myTr.position = myPos;
        }
        Vector2 dir = Vector2.right * LR;

        float throwCoolTime = 0f;
        float effectCoolTime = 0f;
        GameObject effect = CreateAttackObject(_attackInfo.WallEffect, 2f, myTr.position, Quaternion.identity);
        effect.transform.localScale = new Vector3(-LR, 1f, 1f);
        for (float myX = myTr.position.x; myX <= endX && myX >= endX2;)
        {
            myX = myTr.position.x;
            _mashine.Rigidbody.velocity = dir * _attackInfo.DrillSpeed;

            throwCoolTime += Time.fixedDeltaTime;
            effectCoolTime += Time.fixedDeltaTime;
            if (throwCoolTime > _attackInfo.BulletInfo.CoolTime)
            {
                throwCoolTime = 0f;
                Throw(_attackInfo.BulletInfo);
            }
            if (effectCoolTime > 0.2f)
            {
                effectCoolTime = 0f;
                GameObject go = CreateAttackObject(_attackInfo.DrillEffect, 1f, myTr.position, Quaternion.identity);
                go.transform.localScale = new Vector3(dir.x, 1f, 1f);
            }

            Collider2D[] hits = Physics2D.OverlapBoxAll
                    (
                        myTr.position,
                        _attackInfo.AttackRange,
                        0f,
                        _mashine.TargetLayer
                    );
            for (int i = 0; i < hits.Length; i++)
            {
                IDamageInfoReceiver damage = hits[i].GetComponent<IDamageInfoReceiver>();
                if (damage != null)
                {
                    SDamageInfo info = new SDamageInfo(Mathf.RoundToInt(_mashine.Attack), dir);
                    damage.TakeDamage(info);
                }
            }


            await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
        }

        GameObject effect2 = CreateAttackObject(_attackInfo.WallEffect, 2f, myTr.position, Quaternion.identity);
        effect2.transform.localScale = new Vector3(LR, 1f, 1f);

        await UniTask.Delay(TimeSpan.FromSeconds(_attackInfo.JumpReadyTime), cancellationToken: ct);
    }

    private void Throw(SBulletInfo info)
    {
        GameObject dynamite = null;
        if (CObjectPoolManager.Instance != null)
        {
            dynamite = CObjectPoolManager.Instance.GetPoolObject(info.Projectile, info.LifeTime + 1f);
            dynamite.transform.position = _mashine.transform.position;
        }
        

        Vector2 toPos = _mashine.transform.position;
        Vector2 ranPos = info.RandomRangePos * 0.5f;
        float ranX = UnityEngine.Random.Range(-ranPos.x, ranPos.x);
        float ranY = UnityEngine.Random.Range(-ranPos.y, ranPos.y);
        toPos += new Vector2(ranX, ranY);

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
