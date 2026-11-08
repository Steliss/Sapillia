using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/Strike", fileName = "Enemy_Attack_Strike_")]
public class CEnemyAttackStateSO_Strike : CEnemyAttackStateSO
{
    #region 내부 구조체
    [Serializable]
    public struct SBulletInfo
    {
        [Header("총알 프리팹")]
        [SerializeField] public GameObject Projectile;
        [Header("총알 속도")]
        [SerializeField] public float BulletSpeed;
        [Header("총알 수명")]
        [SerializeField] public float BulletLifeTime;
        [Header("추적 하는 투사체")]
        [SerializeField] public float TrackingSpeed;
        [Header("각도 보정")]
        [SerializeField] public float AnglePlus;

    }

    [Serializable]
    public struct SAttackInfo_Square
    {
        [Header("이펙트 프리팹")]
        public GameObject Effect;
        [Header("투사체 정보")]
        public SBulletInfo[] BulletInfos;

        [Header("===== 애니메이터 =====")]
        public string AniAttReady;
        public string AniAttack;

        [Header("해시용 변수. 입력x")]
        [HideInInspector] public int HashAttReady;
        [HideInInspector] public int HashAttack;
    }
    #endregion


    #region 인스펙터
    [SerializeField] private SAttackInfo_Square[] _attackInfos;
    [SerializeField] private SLineInfo[] _lineInfo;
    #endregion

    private void OnEnable()
    {
        for (int i = 0; i < _attackInfos.Length; i++)
        {
            _attackInfos[i].HashAttReady = Animator.StringToHash(_attackInfos[i].AniAttReady);
            _attackInfos[i].HashAttack = Animator.StringToHash(_attackInfos[i].AniAttack);
        }
    }

    public override void StateEnter()
    {
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
                // 전조 표시
                WarningLine_FireAngle(ct, _lineInfo[i]).Forget();
                await WarningLine_Square(ct, myPos, _lineInfo[i]);

                // 공격과 투사체 생성
                _mashine.Ani.Play(_attackInfos[i].HashAttack);
                if (_attackInfos[i].Effect != null)
                {
                    CreateAttackObject(_attackInfos[i].Effect, 1f, myPos, Quaternion.identity);     // 이펙트
                }
                Attack_Square(myPos, _lineInfo[i].SquareRange);                                     // 공격 판정
                for (int j = 0; j < _attackInfos[i].BulletInfos.Length; j++)
                {
                    BulletFire_Homing(_attackInfos[i].BulletInfos[j]);                              // 투사체 생성
                }

                await UniTask.Delay(TimeSpan.FromSeconds(_lineInfo[i].AfterDelay), cancellationToken: ct);
            }


            _mashine.StateChanged(_mashine.IdleState);
        });
    }
    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
    }


    #region 공격 함수
    protected void BulletFire_Homing(SBulletInfo info)
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

    protected void Attack_Square(Vector2 point, Vector2 range)
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
    #endregion
}