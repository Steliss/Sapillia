using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/HomingMelee", fileName = "Enemy_Attack_HomingMelee_")]
public class CEnemyAttackStateSO_HomingMelee : CEnemyAttackStateSO
{
    #region 내부 구조체
    [Serializable]
    public struct SEnemyAttackInfo
    {
        [Header("프리팹")]
        public GameObject Projectile;
        public bool HasAngle;
        public float AttackDistans;
        [Header("선딜, 공격시간, 후딜")]
        public float BeforeDelay;
        public float AttackTime;
        public float AfterDelay;
        [Header("공격시 돌진 거리")]
        public float RushDistanse;
        [Header("추적 속도 (초당 회전각도)")]
        public float TrackingSpeed;

        [Header("===== 애니메이터 =====")]
        public string AniAttReady;
        public string AniAttack;
        [Header("해시용 변수 입력x")]
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
    [SerializeField] private float _lineLength = 3f;
    [Header("간격")]
    [SerializeField] private float _lineInterbal = 1f;
    [Header("위치")]
    [SerializeField] private float _lineDistanse = 2.5f;
    #endregion

    #region 내부 변수
    private CUniTaskOnes _uni = new CUniTaskOnes();
    #endregion

    #region 이벤트 함수
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
            Vector2 myPos = _mashine.transform.position;
            Vector2 targetPos = _mashine.Target.position;
            Vector2 dir = (targetPos - myPos).normalized;
            float aimAngle = DirToAngle(dir);

            for (int i = 0; i < _attackInfos.Length; i++)
            {
                // 선딜레이
                aimAngle = await TrackingAim(ct, aimAngle, _attackInfos[i]);

                // 공격
                await Attack(ct, aimAngle, _attackInfos[i]);

                // 후딜레이
                await UniTask.Delay
                    (
                    TimeSpan.FromSeconds(_attackInfos[i].AfterDelay), 
                    cancellationToken: ct
                    );
            }

            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
        _mashine.GameObjectsListClear();
    }

    public override void StateUpdate()
    {

    }


    private async UniTask<float> TrackingAim
        (CancellationToken ct, float aimAngle, SEnemyAttackInfo info)
    {
        _mashine.Nav.velocity = Vector3.zero;
        _mashine.Ani.Play(info.HashAttReady);

        Transform my = _mashine.transform;
        Transform target = _mashine.Target;

        Vector2 aimDir = AngleToDir(aimAngle);
        Vector2 aimDir90 = new Vector2(-aimDir.y, aimDir.x);

        

        // 라인 생성
        GameObject[] lines = new GameObject[4];
        float[] sides = { -1f, 1f };

        Vector3 scale = _warningLine.transform.localScale;
        scale.y = _lineLength;

        Vector2 linePos = _mashine.transform.position;
        linePos += aimDir * _lineDistanse;


        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateAttackObject
                (
                _warningLine,
                info.BeforeDelay + info.AttackTime,
                Vector2.zero,
                Quaternion.identity
                );
            lines[i].transform.localScale = scale;
        }

        // 라인으로 추적
        for (float time = 0f; time < info.BeforeDelay; time += Time.deltaTime)
        {
            Vector2 targetDir = target.position - my.position;
            float targetAngle = DirToAngle(targetDir.normalized);

            // 좌우 반전
            bool isLeft = aimDir.x < 0;
            _mashine.SpriteFlipX(isLeft);

            // TrackingSpeed의 속도로 플레이어를 추적
            aimAngle = Mathf.MoveTowardsAngle
                (
                    aimAngle, 
                    targetAngle, 
                    Time.deltaTime * info.TrackingSpeed
                );

            aimDir = AngleToDir(aimAngle).normalized;
            aimDir90 = new Vector2(-aimDir.y, aimDir.x);
            linePos = (Vector2)my.position + aimDir * _lineDistanse;
            Quaternion aimQua = Quaternion.Euler(0f, 0f, aimAngle);

            // 고정된 라인 조정
            for (int i = 2; i < 4; i++)
            {
                lines[i].transform.SetPositionAndRotation
                    (linePos + (aimDir90 * _lineInterbal * sides[i % 2]), aimQua);
            }

            float gap = Mathf.Lerp(0.3f, _lineInterbal, time / info.BeforeDelay);

            // 움직이는 라인 조정
            for (int i = 0; i < 2; i++)
            {
                lines[i].transform.SetPositionAndRotation
                    (linePos + (aimDir90 * gap * sides[i]), aimQua);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _uni.Run(async linect =>
        {
            // 공격시간동안 라인들 투명해지며 사라지기
            SpriteRenderer[] sp = new SpriteRenderer[4];
            for (int i = 0; i < sp.Length; i++)
            {
                sp[i] = lines[i].GetComponent<SpriteRenderer>();
            }

            for (float time = 0f; time < info.AttackTime; time += Time.deltaTime)
            {
                float t = 1 - (time / info.AttackTime);
                Color alpha = sp[0].color;
                alpha.a = t;
                for (int i = 0; i < sp.Length; i++)
                {
                    sp[i].color = alpha;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, linect);
            }
        });

        return aimAngle;
    }

    private async UniTask Attack(CancellationToken ct, float aimAngle, SEnemyAttackInfo info)
    {
        _mashine.Ani.Play(info.HashAttack);

        Vector2 dir = AngleToDir(aimAngle);
        _mashine.Rigidbody.AddForce(dir * info.RushDistanse, ForceMode2D.Impulse);

        Vector2 myPos = _mashine.transform.position;
        Quaternion qua = Quaternion.Euler(0f, 0f, aimAngle);

        // 공격 객체 생성
        GameObject go = CreateAttackObject
        (
            info.Projectile,
            info.AttackTime + 0.3f,
            myPos + (dir * info.AttackDistans),
            info.HasAngle ? qua : Quaternion.identity
        );

        if (go != null)
        {
            go.transform.parent = _mashine.gameObject.transform;
            // 공격 객체 데미지, 방향 설정
            CEnemyProjectile goPro = go.GetComponent<CEnemyProjectile>();
            if (goPro != null)
            {
                goPro.DamageSet(_mashine.Attack, dir, info.AttackTime);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(info.AttackTime), cancellationToken: ct);
        }
    }
}