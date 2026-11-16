using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;



[CreateAssetMenu(menuName = "SO/Enemy/AttackState/RushAttack", fileName = "Enemy_Attack_RushAttack_")]
public class CEnemyAttackStateSO_RushAttack : CEnemyAttackStateSO
{
    #region 인스펙터
    [Header("돌진 공격")]
    [SerializeField] private float _rushTime = 1.5f;
    [SerializeField] private float _rushSpeed = 3f;
    [SerializeField] private Vector2 _hitBox = new Vector2(2.5f, 2f);


    [Space(15)]
    [Header("전조")]
    [SerializeField] private SLineInfo _lineInfo;


    [Space(15)]
    [Header("애니메이션 스테이트")]
    [SerializeField] private string _stateRushAttReady = "RushAttReady";
    [SerializeField] private string _stateRushAttack = "RushAttack";
    #endregion

    #region 내부 변수
    private int _hashRushAttReady;
    private int _hashRushAttack;
    private CUniTaskOnes _uni = new CUniTaskOnes();
    #endregion

    private void OnEnable()
    {
        _hashRushAttReady = Animator.StringToHash(_stateRushAttReady);
        _hashRushAttack = Animator.StringToHash(_stateRushAttack);
    }

    public override void StateEnter()
    {
        _mashine.AttackUni.Run(async ct =>
        {
            // ===== 선 딜레이 =====
            _mashine.Ani.Play(_hashRushAttReady);
            _mashine.Rigidbody.velocity = Vector2.zero;

            Vector2 dir = _mashine.Target.transform.position - _mashine.transform.position;
            dir.Normalize();

            await WarningLine_Melee(ct, DirToAngle(dir), _lineInfo);

            // ===== 돌진 =====
            _mashine.Ani.Play(_hashRushAttack);


            for (float time = 0f; time < _rushTime; time += Time.fixedDeltaTime)
            {
                Collider2D[] hits = Physics2D.OverlapBoxAll
                    (
                        _mashine.transform.position,
                        _hitBox,
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

                _mashine.Rigidbody.velocity = dir * _rushSpeed;
                await UniTask.Yield(PlayerLoopTiming.FixedUpdate, ct);
            }

            // ===== 후 딜레이 =====
            _mashine.Ani.Play(_mashine.HashIdle);
            _mashine.Rigidbody.velocity = Vector2.zero;
            await UniTask.Delay(TimeSpan.FromSeconds(_lineInfo.AfterDelay), cancellationToken: ct);
            _mashine.StateChanged(_mashine.IdleState);
        });

    }

    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
        _uni.Cancel();
    }

    public override void StateUpdate()
    {
    }


}
