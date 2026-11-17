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
    [Header("대쉬 이펙트")]
    [SerializeField] private GameObject _dashDust;
    [SerializeField] private float _dustPlusY;

    [Header("전조")]
    [SerializeField] private SLineInfo _lineInfo;

    [Space(15)]
    [Header("애니메이션 스테이트")]
    [SerializeField] private string _stateRushAttReady = "RushAttReady";
    [SerializeField] private string _stateRushAttack = "RushAttack";
    [Header("사운드")]
    [SerializeField] private string _soundStart01 = "QTempleMiniboss_Mouse_JumpUp";
    [SerializeField] private string _soundStart02 = "SpellArcaneBullet";
    [SerializeField] private string _soundStep = "PrologueBit";
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

            _mashine.SoundPlay(_soundStart01);
            _mashine.SoundPlay(_soundStart02);
            await WarningLine_Melee(ct, DirToAngle(dir), _lineInfo);

            // ===== 돌진 =====
            _mashine.Ani.Play(_hashRushAttack);

            float stepTime = 0f;
            for (float time = 0f; time < _rushTime; time += Time.fixedDeltaTime)
            {
                stepTime -= Time.fixedDeltaTime;
                if (stepTime < 0f)
                {
                    stepTime = 0.2f;
                    _mashine.SoundPlay(_soundStep);
                    Vector2 dustPos = _mashine.transform.position;
                    dustPos.y = _mashine.transform.position.y + _dustPlusY;
                    GameObject dust = CreateAttackObject(_dashDust, 1f, dustPos, Quaternion.identity);
                    CEnemyEffect_DashDust effect = dust.GetComponent<CEnemyEffect_DashDust>();
                    effect.SetDir(dir);
                }


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
