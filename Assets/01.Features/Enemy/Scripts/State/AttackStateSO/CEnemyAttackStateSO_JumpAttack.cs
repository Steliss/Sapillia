using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/AttackState/JumpAttack", fileName = "Enemy_Attack_JumpAttack_")]
public class CEnemyAttackStateSO_JumpAttack : CEnemyAttackStateSO
{
    #region 인스펙터
    [Header("내려찍기 후 생성할 투사체")]
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _stamp;
    [SerializeField] private float _projectileSpeed = 3f;
    [SerializeField] private float _projectileTurnSpeed = 50f;
    [SerializeField] private float _projectileLifeTime = 3f;

    [Header("점프공격")]
    [SerializeField] private Vector2 _range = new Vector2(4f, 3f);
    [SerializeField] private float _beforeDelay = 1f;
    [SerializeField] private float _jumpTime = 1f;
    [SerializeField] private float _afterDelay = 1f;
    [SerializeField] private float _jumpHigh = 4f;

    [Space(15)]
    [Header("전조 이펙트")]
    [SerializeField] private GameObject _warningLine;
    [SerializeField] private float _xLineLength = 1f;
    [SerializeField] private float _yLineLength = 1.5f;


    [Space(15)]
    [Header("애니메이션 스테이트")]
    [SerializeField] private string _stateJumpAttReady = "JumpAttReady";
    [SerializeField] private string _stateJump = "Jump";
    [SerializeField] private string _stateJumpAttack = "JumpAttack";

    #endregion

    #region 내부 변수
    private int _hashJumpAttReady;
    private int _hashJump;
    private int _hashJumpAttack;
    private CUniTaskOnes _uni = new CUniTaskOnes();
    #endregion

    private void OnEnable()
    {
        _hashJumpAttReady = Animator.StringToHash(_stateJumpAttReady);
        _hashJump = Animator.StringToHash(_stateJump);
        _hashJumpAttack = Animator.StringToHash(_stateJumpAttack);
    }

    public override void StateEnter()
    {
        _mashine.AttackUni.Run(async ct =>
        {
            Vector2 targetPos = _mashine.Target.position;
            
            // 전조 별도 진행
            _uni.Run(async lines =>
            {
                await WarningLine(lines, targetPos);
            });


            // ===== 선딜레이 ===== 
            _mashine.Ani.Play(_hashJumpAttReady);
            await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay), cancellationToken: ct);

            //  ===== 점프 ===== 
            _mashine.Ani.Play(_hashJump);

            Vector3 startPos = _mashine.transform.position;
            for (float time = 0f; time < _jumpTime; time += Time.deltaTime)
            {
                float t = time / _jumpTime;

                float y = Mathf.Sin(t * Mathf.PI);

                Vector3 air = _mashine.Sprite.transform.localPosition;
                air.y = y * _jumpHigh;
                _mashine.Sprite.transform.localPosition = air;

                _mashine.transform.position = Vector2.Lerp(startPos, targetPos, t * 0.9f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            _mashine.Sprite.transform.localPosition = Vector2.zero;

            // ===== 공격과 투사체 생성 ===== 
            _mashine.Ani.Play(_hashJumpAttack);

            // 히트판정
            Collider2D[] hits = Physics2D.OverlapBoxAll(targetPos, _range, 0f, _mashine.TargetLayer);

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

            // 투사체 생성
            CreateAttackObject(_stamp, _afterDelay, targetPos, Quaternion.identity);
            Vector2[] dirs =
            {
                Vector2.up,
                Vector2.down,
                Vector2.left + Vector2.up,
                Vector2.left + Vector2.down,
                Vector2.right + Vector2.up,
                Vector2.right + Vector2.down,
            };

            GameObject[] projectiles = new GameObject[6];

            for (int i = 0; i < projectiles.Length; i++) 
            {
                projectiles[i] = CreateAttackObject
                (
                _projectile,
                _projectileLifeTime + 1f,
                targetPos,
                Quaternion.identity
                );
                CEnemyProjectile_Homing homing = 
                    projectiles[i].GetComponent<CEnemyProjectile_Homing>();

                if (homing != null)
                {
                    homing.DamageSet(_mashine.Attack, dirs[i], _projectileLifeTime);
                    homing.HomingTargetSet
                        (
                            _mashine.Target, 
                            dirs[i].normalized, 
                            _projectileSpeed, 
                            _projectileTurnSpeed
                        );
                }
            }
            

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

    private async UniTask WarningLine(CancellationToken ct, Vector2 pos)
    {
        Vector2 range = _range * 0.5f;

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
                    _beforeDelay + _jumpTime + _afterDelay,
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
                    _beforeDelay + _jumpTime + _afterDelay,
                    linePos,
                    Quaternion.Euler(0, 0, 90)
                );
            Vector2 scale = yLines[i].transform.localScale;
            scale.y = _yLineLength;
            yLines[i].transform.localScale = scale;
        }

        // 라인 이동
        float moveTime = _beforeDelay + _jumpTime;
        for (float time = 0f; time < moveTime; time += Time.deltaTime)
        {
            float xGap = Mathf.Lerp(_yLineLength * 0.5f, range.x, time / moveTime);
            float yGap = Mathf.Lerp(_xLineLength * 0.5f, range.y, time / moveTime);

            for (int i = 0; i < 2; i++)
            {
                ct.ThrowIfCancellationRequested();
                xLines[i].transform.position = pos + (Vector2.right * xGap * sides[i]);
                yLines[i].transform.position = pos + (Vector2.up * yGap * sides[i]);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        // 페이드 아웃
        SpriteRenderer[] sps = new SpriteRenderer[8];
        for (int i = 0; i < 4; i++)
        {
            sps[i] = xLines[i].GetComponent<SpriteRenderer>();
            sps[i + 4] = yLines[i].GetComponent<SpriteRenderer>();
        }

        for (float time = 0f; time < _afterDelay; time += Time.deltaTime)
        {
            float t = 1 - (time / _afterDelay);
            Color alpha = sps[0].color;
            alpha.a = t;

            for (int i = 0; i < sps.Length; i++)
            {
                sps[i].color = alpha;
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
    }
}
