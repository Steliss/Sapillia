using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/Melee", fileName = "Enemy_Attack_Melee_")]
public class CEnemyAttackStateSO_Melee : CEnemyAttackStateSO
{
    #region 인스펙터

    [Header("===== 공격 =====")]
    [Header("프리팹")]
    [SerializeField] private GameObject _projectile;
    [SerializeField] private bool _hasAngle = true;
    [SerializeField] private float _attackDistans = 1f;
    [Header("선딜, 공격시간, 후딜")]
    [SerializeField] private float _beforeDelay = 2f;
    [SerializeField] private float _attackTime = 1f;
    [SerializeField] private float _afterDelay = 0.5f;
    [Header("공격시 돌진 거리")]
    [SerializeField] private float _rushDistanse = 5f;
    [Header("연속 공격 횟수")]
    [Min(1)]
    [SerializeField] private int _AttackCount = 2;
    [Space(10)]
    [Header("===== 전조 =====")]
    [Header("프리팹")]
    [SerializeField] private GameObject _warningLine;
    [Header("길이")]
    [SerializeField] private float _lineLength = 3f;
    [Header("간격")]
    [SerializeField] private float _interbal = 1f;
    [Header("위치")]
    [SerializeField] private float _distanse = 2.5f;
    #endregion

    #region 내부 변수
    private CUniTaskOnes _uni = new CUniTaskOnes();
    #endregion

    public override void StateEnter()
    {
        _mashine.AttackUni.Cancel();
        _mashine.AttackUni.Run(async ct =>
        {
            // 사용할 변수 계산
            Vector2 myPos = _mashine.transform.position;
            Vector2 tarPos = _mashine.Target.position;

            Vector2 dir = (tarPos - myPos).normalized;              // 방향
            Vector2 dir90 = new Vector2(-dir.y, dir.x);             // 법선
            float angle = DirToAngle(dir);                          // 각도
            Quaternion qua = Quaternion.Euler(0, 0, angle);         // 쿼터니언
            Vector2 linePos = myPos + (dir * _distanse);

            // 좌우 반전
            if (dir.x < 0)
            {
                _mashine.SpriteFlipX(true);
            }
            else
            {
                _mashine.SpriteFlipX(false);
            }

            // 선딜레이
            await BeforeDelay(ct, linePos, dir90, qua);

            // 공격
            for (int i = 0; i < _AttackCount; i++)
            {
                await Attack(ct, dir, qua);

                if (i != _AttackCount - 1)
                {
                    _mashine.Ani.Play(_mashine.HashAttReady);
                }
                // 후딜레이
                await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay), cancellationToken: ct);
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


    private async UniTask BeforeDelay
        (CancellationToken ct, Vector2 linePos, Vector2 dir90, Quaternion qua)
    {
        _mashine.Nav.velocity = Vector3.zero;
        _mashine.Ani.Play(_mashine.HashAttReady);

        // 라인 생성
        GameObject[] lines = new GameObject[4];
        float[] sides = { -1f, 1f };

        Vector3 scale = _warningLine.transform.localScale;
        scale.y = _lineLength;

        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateAttackObject
                (
                _warningLine, 
                _beforeDelay + _attackTime, 
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

        _uni.Run(async token =>
        {
            // 공격시간동안 라인들 투명해지며 사라지기
            SpriteRenderer[] sp = new SpriteRenderer[4];
            for (int i = 0; i < sp.Length; i++)
            {
                sp[i] = lines[i].GetComponent<SpriteRenderer>();
            }

            for (float time = 0f; time < _attackTime; time += Time.deltaTime)
            {
                float t = 1 - (time / _attackTime);
                Color alpha = sp[0].color;
                alpha.a = t;
                for (int i = 0; i < sp.Length; i++)
                {
                    sp[i].color = alpha;
                }
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        });
    }

    private async UniTask Attack(CancellationToken ct, Vector2 dir, Quaternion qua)
    {
        _mashine.Ani.Play(_mashine.HashAttack);
        _mashine.Rigidbody.AddForce(dir * _rushDistanse, ForceMode2D.Impulse);

        Vector2 myPos = _mashine.transform.position;
        // 공격 객체 생성
        GameObject go = CreateAttackObject
        (
            _projectile, 
            _attackTime + _afterDelay, 
            myPos + (dir.normalized * _attackDistans), 
            _hasAngle ? qua : Quaternion.identity
        );

        if (go != null)
        {
            go.transform.parent = _mashine.gameObject.transform;
            // 공격 객체 데미지, 방향 설정
            CEnemyProjectile goPro = go.GetComponent<CEnemyProjectile>();
            if (goPro != null)
            {
                goPro.DamageSet(_mashine.Attack, dir, _attackTime);
            }

            await UniTask.Delay(TimeSpan.FromSeconds(_attackTime), cancellationToken: ct);
        }
    }
}
