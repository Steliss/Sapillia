using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/DynamiteThrow", fileName = "Enemy_Attack_DynamiteThrow_")]
public class CEnemyAttackStateSO_DynamiteThrow : CEnemyAttackStateSO
{
    #region 인스펙터
    [Header("===== 공격 =====")]
    [Header("다이너마이트 프리팹")]
    [SerializeField] private GameObject _projectile;
    [Header("투사체 속성")]
    [SerializeField] private int _multiFire = 1;
    [SerializeField] private float _dynamiteLifeTime = 1.5f;
    [SerializeField] private Vector2 _randomPosRange = new Vector2(1f, 0.5f);

    [Space(10)]
    [Header("===== 전조 =====")]
    [SerializeField] private SLineInfo _lineInfo;

    [Space(10)]
    [Header("===== 애니메이터 =====")]
    [SerializeField] private string _aniStart;
    [SerializeField] private string _aniEnd;
    #endregion

    #region 내부 변수
    private int _hashStart;
    private int _hashEnd;
    #endregion

    #region 유니티 이벤트 함수
    private void OnEnable()
    {
        _hashStart = Animator.StringToHash(_aniStart);
        _hashEnd = Animator.StringToHash(_aniEnd);
    }
    #endregion

    public override void StateEnter()
    {
        _mashine.AttackUni.Cancel();
        _mashine.AttackUni.Run(async ct =>
        {
            // 선딜 + 조준
            _mashine.Ani.Play(_hashStart);
            Vector2 aimPos = await WarningLine_FirePos(ct, _lineInfo);
            // 던지기
            for (int i = 0; i < _multiFire; i++)
            {
                Throw(aimPos);
            }

            // 후딜
            await UniTask.Delay(TimeSpan.FromSeconds(_lineInfo.AfterDelay), cancellationToken: ct);



            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
    }

    public override void StateUpdate()
    {
        // 좌우 반전
        bool isLeft = _mashine.Target.position.x < _mashine.transform.position.x;
        _mashine.SpriteFlipX(isLeft);
    }

    private void Throw(Vector2 toPos)
    {
        _mashine.Ani.Play(_hashEnd);

        GameObject dynamite = null;
        if (CObjectPoolManager.Instance != null)
        {
            dynamite = CObjectPoolManager.Instance.GetPoolObject(_projectile, _dynamiteLifeTime + 1f);
            dynamite.transform.position = _mashine.transform.position;
        }

        // 목표 근처 랜덤범위 적용
        float ranX = UnityEngine.Random.Range(-_randomPosRange.x, _randomPosRange.x);
        float ranY = UnityEngine.Random.Range(-_randomPosRange.y, _randomPosRange.y);
        toPos.x += ranX;
        toPos.y += ranY;

        // 데미지 설정 및 도착위치 설정
        CEnemyProjectile_Dynamite pro = dynamite.GetComponent<CEnemyProjectile_Dynamite>();
        if (pro != null)
        {
            pro.DamageSet(_mashine.Attack, toPos, _dynamiteLifeTime);
        }
    }
}