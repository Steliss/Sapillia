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
        [Header("공격 판정 시간")]
        public float AttackTime;
        [Header("공격시 돌진 거리")]
        public float RushDistanse;

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
    [SerializeField] private SLineInfo[] _lineinfos;
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
                _mashine.Ani.Play(_attackInfos[i].HashAttReady);
                aimAngle = await WarningLine_Melee(ct, aimAngle, _lineinfos[i]);

                // 공격
                _mashine.Ani.Play(_attackInfos[i].HashAttack);
                await Attack(ct, aimAngle, _attackInfos[i]);

                // 후딜레이
                await UniTask.Delay
                    (
                    TimeSpan.FromSeconds(_lineinfos[i].AfterDelay), 
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

    private async UniTask Attack(CancellationToken ct, float aimAngle, SEnemyAttackInfo info)
    {

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