using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/Strike", fileName = "Enemy_Attack_Strike_")]
public class CEnemyAttackStateSO_Strike : CEnemyAttackStateSO
{
    #region 내부 구조체
    [Serializable]
    public struct SEnemyBulletInfo
    {
        [Header("총알 프리팹")]
        [SerializeField] public GameObject Projectile;
        [Header("총알 속도")]
        [SerializeField] public float BulletSpeed;
        [Header("추적 속도 (초당 회전각도)")]
        [SerializeField] public float TrackingSpeed;
        [Header("시작 각도 (기본 타겟각도)")]
        [SerializeField] public float AnglePlus;
        [Header("총알 수명")]
        [SerializeField] public float BulletLifeTime;
    }

    [Serializable]
    public struct SEnemyAttackInfo
    {
        [Header("이펙트 프리팹")]
        public GameObject Effect;
        [Header("선딜, 후딜")]
        public float BeforeDelay;
        public float AfterDelay;
        [Header("공격 범위")]
        public Vector2 Range;
        [Header("투사체 정보")]
        public SEnemyBulletInfo[] BulletInfos;

        [Header("===== 애니메이터 =====")]
        public string AniAttReady;
        public string AniAttack;

        [Header("해시용 변수. 입력x")]
        [HideInInspector] public int HashAttReady;
        [HideInInspector] public int HashAttack;
    }

    
    #endregion

    #region 인스펙터
    [Header("===== 공격 =====")]
    [SerializeField] private SEnemyAttackInfo[] _attackInfos;

    [Space(10)]
    [Header("===== 전조 =====")]
    [Header("프리팹")]
    [SerializeField] private GameObject _warningLine;
    [Header("길이")]
    [SerializeField] private float _xLineLength = 3f;
    [SerializeField] private float _yLineLength = 3f;
    #endregion

    #region 내부 변수
    private CUniTaskOnes _uni = new CUniTaskOnes();
    #endregion

    #region 유니티 이벤트 함수
    private void OnEnable()
    {
        for (int i = 0; i < _attackInfos.Length; i++)
        {
            _attackInfos[i].HashAttReady = Animator.StringToHash(_attackInfos[i].AniAttReady);
            _attackInfos[i].HashAttack = Animator.StringToHash(_attackInfos[i].AniAttack);
        }
    }

    private void OnDisable()
    {
        _uni.Cancel();
    }
    #endregion


    public override void StateEnter()
    {
        _mashine.AttackUni.Cancel();
        _mashine.AttackUni.Run(async ct =>
        {
            for (int i = 0; i < _attackInfos.Length; i++)
            {
                // 좌우 반전
                Vector2 myPos = _mashine.transform.position;
                bool isLeft = ((Vector2)_mashine.Target.position - myPos).x < 0;
                _mashine.SpriteFlipX(isLeft);

                // 선 딜레이
                _mashine.Ani.Play(_attackInfos[i].HashAttReady);
                await BeforeDelay(ct, myPos, _attackInfos[i]);

                // 공격과 투사체 생성
                _mashine.Ani.Play(_attackInfos[i].HashAttack);
                if (_attackInfos[i].Effect != null)
                {
                    CreateAttackObject(_attackInfos[i].Effect, 1f, myPos, Quaternion.identity);
                }
                AttackRange(myPos, _attackInfos[i].Range);
                for (int j = 0; j < _attackInfos[i].BulletInfos.Length; j++)
                {
                    // 투사체 생성
                    FireBullet(_attackInfos[i].BulletInfos[j]);
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_attackInfos[i].AfterDelay), cancellationToken: ct);
            }


            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
    }

    public override void StateUpdate()
    {

    }

    private async UniTask BeforeDelay(CancellationToken ct, Vector2 pos, SEnemyAttackInfo info)
    {
        Vector2 range = info.Range * 0.5f;

        // 라인 생성
        GameObject[] xLines = new GameObject[4];
        GameObject[] yLines = new GameObject[4];
        float[] sides = { -1f, 1f };

        for (int i = 0; i < xLines.Length; i++)
        {
            Vector2 linePos = pos;
            linePos.x += range.x * sides[i % 2];
            xLines[i] = CreateAttackObject
                (
                    _warningLine,
                    info.BeforeDelay + info.AfterDelay,
                    linePos,
                    Quaternion.Euler(0, 0, 0)
                );
            Vector2 scale = xLines[i].transform.localScale;
            scale.y = _xLineLength;
            xLines[i].transform.localScale = scale;
        }

        for (int i = 0; i < yLines.Length; i++)
        {
            Vector2 linePos = pos;
            linePos.y += range.y * sides[i % 2];
            yLines[i] = CreateAttackObject
                (
                    _warningLine,
                    info.BeforeDelay + info.AfterDelay,
                    linePos,
                    Quaternion.Euler(0, 0, 90)
                );
            Vector2 scale = yLines[i].transform.localScale;
            scale.y = _yLineLength;
            yLines[i].transform.localScale = scale;
        }

        // 라인 이동
        float moveTime = info.BeforeDelay;
        for (float time = 0f; time < moveTime; time += Time.deltaTime)
        {
            float xGap = Mathf.Lerp(_yLineLength * 0.5f, range.x, time / moveTime);
            float yGap = Mathf.Lerp(_xLineLength * 0.5f, range.y, time / moveTime);

            for (int i = 0; i < 2; i++)
            {
                xLines[i].transform.position = pos + (Vector2.right * xGap * sides[i]);
                yLines[i].transform.position = pos + (Vector2.up * yGap * sides[i]);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _uni.Run(async linect =>
        {
            // 페이드 아웃
            SpriteRenderer[] sps = new SpriteRenderer[8];
            for (int i = 0; i < 4; i++)
            {
                sps[i] = xLines[i].GetComponent<SpriteRenderer>();
                sps[i + 4] = yLines[i].GetComponent<SpriteRenderer>();
            }

            for (float time = 0f; time < info.AfterDelay; time += Time.deltaTime)
            {
                float t = 1 - (time / info.AfterDelay);
                Color alpha = sps[0].color;
                alpha.a = t;

                for (int i = 0; i < sps.Length; i++)
                {
                    sps[i].color = alpha;
                }

                await UniTask.Yield(PlayerLoopTiming.Update, linect);
            }
        });
        
    }

    private void FireBullet(SEnemyBulletInfo info)
    {
        GameObject bullet = CreateAttackObject
            (
                info.Projectile, 
                info.BulletLifeTime + 0.5f, 
                _mashine.transform.position, 
                Quaternion.identity
            );

        CEnemyProjectile_Homing homing =
                bullet.GetComponent<CEnemyProjectile_Homing>();

        Vector2 dir = _mashine.Target.position - _mashine.transform.position;
        float angle = DirToAngle(dir) + info.AnglePlus;
        dir = AngleToDir(angle);

        if (homing != null)
        {
            homing.DamageSet(_mashine.Attack, dir, info.BulletLifeTime);
            homing.HomingTargetSet
                (
                    _mashine.Target,
                    dir,
                    info.BulletSpeed,
                    info.TrackingSpeed
                );
        }
    }

    private void AttackRange(Vector2 point, Vector2 range)
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(point, range, 0f, _mashine.TargetLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            IDamageInfoReceiver damage = hits[i].GetComponent<IDamageInfoReceiver>();
            if (damage != null)
            {
                Vector2 dir = hits[i].transform.position - _mashine.transform.position;
                SDamageInfo info = new SDamageInfo(Mathf.RoundToInt(_mashine.Attack), dir.normalized);

                damage.TakeDamage(info);
            }
        }
    }
}