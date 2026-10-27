using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;
using static UnityEditor.PlayerSettings;

[CreateAssetMenu(menuName = "SO/Enemy/AttackState/Fire", fileName = "Enemy_Attack_Fire_")]
public class CEnemyAttackStateSO_Fire : CEnemyAttackStateSO
{
    #region 인스펙터

    [Header("===== 공격 =====")]
    [Header("총알 프리팹")]
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _fireEffect;
    [Header("선딜시간, 조준 추적")]
    [SerializeField] private float _beforeDelay = 2f;
    [SerializeField] private float _trackingTime = 1f;
    [SerializeField] private float _trackingSpeed = 1f;
    [Header("투사체 속성")]
    [SerializeField] private int _multiFire = 1;

    [SerializeField] private float _firePower = 3f;
    [SerializeField] private float _fireAngle = 0f;
    [SerializeField] private float _bulletLifeTime = 3f;

    [Space(10)]
    [Header("===== 전조 =====")]
    [Header("프리팹")]
    [SerializeField] private GameObject _warningLine;
    [Header("길이")]
    [SerializeField] private float _lineLength = 3f;
    [Header("간격")]
    [SerializeField] private float _interbal = 1f;
    #endregion


    public override void StateEnter()
    {
        _mashine.AttackUni.Cancel();
        _mashine.AttackUni.Run(async ct =>
        {
            // 선딜
            await BeforeDelay(ct);

            // 조준
            float aim = await TrackingAim(ct);
            
            // 발사
            BulletFire(aim);


            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
    }

    public override void StateUpdate()
    {
        
    }

    private async UniTask BeforeDelay(CancellationToken ct)
    {
        _mashine.Ani.Play(_mashine.HashAttReady);
        _mashine.Nav.velocity = Vector3.zero;
        
        await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay), cancellationToken: ct);
    }

    private async UniTask<float> TrackingAim(CancellationToken ct, float lookAngle = 0f)
    {
        _mashine.Ani.Play(_mashine.HashAttack);

        Vector2 myPos = _mashine.transform.position;
        Vector2 targetPos = _mashine.Target.position;

        // 라인 초기 세팅
        GameObject[] lines = new GameObject[2];
        float[] sides = { -1f, 1f };
        for (int i = 0; i < lines.Length; i++)
        {
            lines[i] = CreateAttackObject
            (_warningLine, _trackingTime + 1f, myPos, Quaternion.identity);

            Vector2 scale = lines[i].transform.localScale;
            scale.y = _lineLength;
            lines[i].transform.localScale = scale;
        }

        // 플레이어 위치부터 조준 시작
        float targetAngle = DirToAngle((targetPos - myPos).normalized);
        float aimAngle = targetAngle;

        // 조준 트래킹
        float time = 0f;
        while (time < _trackingTime)
        {
            time += Time.deltaTime;
            float t01 = time / _trackingTime;
            myPos = _mashine.transform.position;
            targetPos = _mashine.Target.position;

            // 타겟 각도 갱신
            targetAngle = DirToAngle((targetPos - myPos).normalized);

            // 에임 따라가기
            aimAngle = Mathf.MoveTowardsAngle(aimAngle, targetAngle, Time.deltaTime * _trackingSpeed);

            Vector2 aimDir = AngleToDir(aimAngle);
            Vector2 aimDir90 = new Vector2(-aimDir.y, aimDir.x);
            Vector2 linePos = myPos + aimDir * _lineLength * 0.5f;
            Quaternion qua = Quaternion.Euler(0, 0, aimAngle);
            float gap = Mathf.Lerp(_interbal, 0f, time / _trackingTime);


            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].transform.SetPositionAndRotation(linePos + (aimDir90 * gap * sides[i]), qua);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        return aimAngle;
    }

    private void BulletFire(float aim)
    {
        Vector2 aimDir = AngleToDir(aim);
        Vector2 myPos = _mashine.transform.position;

        GameObject effect = CreateAttackObject
            (_fireEffect, _bulletLifeTime, myPos + aimDir, Quaternion.identity);

        GameObject bullet = CreateAttackObject
        (_projectile, _bulletLifeTime, myPos + aimDir, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(aimDir * _firePower, ForceMode2D.Impulse);
            CLog.Log($"{aimDir * _firePower}로 발사");
        }
    }

    private float DirToAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
    }

    private Vector2 AngleToDir(float angle)
    {
        return Quaternion.Euler(0, 0, angle) * Vector2.up;
    }

}
