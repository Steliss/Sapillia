using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;



[CreateAssetMenu(menuName = "SO/Enemy/AttackState/RushAttack", fileName = "Enemy_Attack_RushAttack_")]
public class CEnemyAttackStateSO_RushAttack : CEnemyAttackStateSO
{
    #region 인스펙터
    [Header("돌진 공격")]
    [SerializeField] private float _beforeDelay = 1f;
    [SerializeField] private float _rushTime = 1.5f;
    [SerializeField] private float _afterDelay = 1f;
    [SerializeField] private float _rushSpeed = 3f;
    [SerializeField] private Vector2 _hitBox = new Vector2(2.5f, 2f);


    [Space(15)]
    [Header("전조 이펙트")]
    [SerializeField] private GameObject _warningLine;
    [SerializeField] private float _lineLength = 5f;
    [SerializeField] private float _linePos = 5f;
    [SerializeField] private float _interbal = 3f;


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
            bool flipX = dir.x < 0f;
            _mashine.SpriteFlipX(flipX);

            // 전조
            _uni.Run(async ct =>
            {
                await WarningLine(ct, dir);
            });


            await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay), cancellationToken: ct);

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
            await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay), cancellationToken: ct);
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

    public async UniTask WarningLine(CancellationToken ct, Vector2 dir)
    {
        Vector2 dir90 = new Vector2(-dir.y, dir.x);

        // 라인 생성
        GameObject[] lines = new GameObject[4];
        float[] sides = { -1f, 1f };

        Vector2 scale = _warningLine.transform.localScale;
        scale.y = _lineLength;

        Vector2 linePos = _mashine.transform.position;
        linePos += dir * _linePos;
        Quaternion qua = Quaternion.Euler(0f, 0f, DirToAngle(dir));
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateAttackObject
                (
                _warningLine,
                _beforeDelay + _rushTime,
                linePos + (dir90 * _interbal * sides[i % 2]),
                qua
                );
            lines[i].transform.localScale = scale;
        }

        // 라인 점점 벌리기
        for (float time = 0f; time < _beforeDelay; time += Time.deltaTime)
        {
            float gap = Mathf.Lerp(0.3f, _interbal, time / _beforeDelay);

            for (int i = 0; i < 2; i++)
            {
                lines[i].transform.position = linePos + (dir90 * gap * sides[i]);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        // 라인 페이드아웃
        SpriteRenderer[] sp = new SpriteRenderer[4];
        for (int i = 0; i < sp.Length; i++)
        {
            sp[i] = lines[i].GetComponent<SpriteRenderer>();
        }

        for (float time = 0f; time < _rushTime; time += Time.deltaTime)
        {
            float t = 1 - (time / _rushTime);
            Color alpha = sp[0].color;
            alpha.a = t;
            for (int i = 0; i < sp.Length; i++)
            {
                sp[i].color = alpha;
            }
            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }


}
