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
    [Header("공격 판정 시간")]
    [SerializeField] private float _attackTime = 1f;
    [Header("공격시 돌진 거리")]
    [SerializeField] private float _rushDistanse = 5f;
    [Header("연속 공격 횟수")]
    [SerializeField, Min(1)] private int _attackCount = 1;
    [Space(10)]
    [Header("===== 전조 =====")]
    [SerializeField] private SLineInfo _lineInfo;
    [Header("===== 전조 =====")]
    [SerializeField] private string _soundMelee = "AttackSpear_Dash";
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
            float angle = DirToAngle(dir);                          // 각도
            Quaternion qua = Quaternion.Euler(0, 0, angle);         // 쿼터니언

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
            _mashine.Ani.Play(_mashine.HashAttReady);
            await WarningLine_Melee(ct, angle, _lineInfo);

            // 공격
            for (int i = 0; i < _attackCount; i++)
            {
                await Attack(ct, dir, qua);

                if (i != _attackCount - 1)
                {
                    _mashine.Ani.Play(_mashine.HashAttReady);
                }
                // 후딜레이
                await UniTask.Delay(TimeSpan.FromSeconds(_lineInfo.AfterDelay), cancellationToken: ct);
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


    private async UniTask Attack(CancellationToken ct, Vector2 dir, Quaternion qua)
    {
        _mashine.Ani.Play(_mashine.HashAttack);
        _mashine.Rigidbody.AddForce(dir * _rushDistanse, ForceMode2D.Impulse);

        Vector2 myPos = _mashine.transform.position;
        // 공격 객체 생성
        GameObject go = CreateAttackObject
        (
            _projectile, 
            _attackTime + 0.5f, 
            myPos + (dir.normalized * _attackDistans), 
            _hasAngle ? qua : Quaternion.identity
        );
        _mashine.SoundPlay(_soundMelee);

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
