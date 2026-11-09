using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/Charge", fileName = "Enemy_Attack_Charge_")]
public class CEnemyAttackStateSO_Charge : CEnemyAttackStateSO
{
    #region 내부 구조체
    [Serializable]
    public struct SBulletInfo
    {
        [Header("총알 프리팹")]
        [SerializeField] public GameObject Projectile;
        [Header("총알 수명")]
        [SerializeField] public float BulletLifeTime;
        [Header("각도 보정")]
        [SerializeField] public float AnglePlus;
        [Header("거리 보정")]
        [SerializeField] public float DistansePlus;

    }

    [Serializable]
    public struct SAttackInfo_Square
    {
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
            _attackInfos[i].HashAttack = Animator.StringToHash(_attackInfos[i].AniAttack);
            _attackInfos[i].HashAttReady = Animator.StringToHash(_attackInfos[i].AniAttReady);
        }
    }

    public override void StateEnter()
    {
        _mashine.AttackUni.Run(async ct =>
        {
            for (int i = 0; i < _attackInfos.Length; i++)
            {
                // 선 딜레이

                _mashine.Ani.Play(_attackInfos[i].HashAttReady);
                await WarningLine_Square(ct, _mashine.transform.position, _lineInfo[i]);

                // 공격과 투사체 생성

                _mashine.Ani.Play(_attackInfos[i].HashAttack);
                Attack_Square(_mashine.transform.position, _lineInfo[i].SquareRange);
                for (int j = 0; j < _attackInfos[i].BulletInfos.Length; j++)
                {
                    // 투사체 발사
                    Throw(_attackInfos[i].BulletInfos[j]);
                }

                // 후 딜레이
                await UniTask.Delay(TimeSpan.FromSeconds(_lineInfo[i].AfterDelay), cancellationToken: ct);
            }

            _mashine.StateChanged(_mashine.IdleState);

        });
    }


    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
    }


    #region 함수
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
    private void Throw(SBulletInfo info)
    {
        GameObject dynamite = null;
        if (CObjectPoolManager.Instance != null)
        {
            dynamite = CObjectPoolManager.Instance.GetPoolObject(info.Projectile, info.BulletLifeTime + 1f);
            dynamite.transform.position = _mashine.transform.position;
        }

        Vector2 toPos = _mashine.Target.position;
        Vector2 myPos = _mashine.transform.position;
        Vector2 dir = toPos - myPos;
        float angle = DirToAngle(dir);
        angle += info.AnglePlus;
        dir = AngleToDir(angle);
        toPos = myPos + (dir * info.DistansePlus);

        float lifeTime = info.BulletLifeTime;
        float ranTime = UnityEngine.Random.Range(-lifeTime * 0.1f, lifeTime * 0.1f);
        lifeTime += ranTime;


        // 데미지 설정 및 도착위치 설정
        CEnemyProjectile_Dynamite pro = dynamite.GetComponent<CEnemyProjectile_Dynamite>();
        if (pro != null)
        {
            pro.DamageSet(_mashine.Attack, toPos, lifeTime);
        }
    }
    #endregion
}
