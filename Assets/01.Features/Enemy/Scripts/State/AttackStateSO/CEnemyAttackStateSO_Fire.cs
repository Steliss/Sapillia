using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

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
    [SerializeField] private float _trackingSpeed = 50f;
    [Header("투사체 속성")]
    [SerializeField] private int _multiFire = 1;
    [SerializeField] private float _firePower = 3f;
    [SerializeField] private float _bulletLifeTime = 3f;
    [SerializeField] private float _randomPowerRange = 0.5f;
    [SerializeField] private float _randomAngleRange = 5f;

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
            GameObject effect = CreateAttackObject
                (_fireEffect, _bulletLifeTime, _mashine.Sprite.transform.position, Quaternion.identity);

            for (int i = 0; i < _multiFire; i++)
            {
                float ranPow = 0f;
                float ranAngle = 0f;
                if (i != 0)
                {
                    ranPow = UnityEngine.Random.Range(-_randomPowerRange, _randomPowerRange);
                    ranAngle = UnityEngine.Random.Range(-_randomAngleRange, _randomAngleRange);
                }
                BulletFire(aim + ranAngle, _firePower + ranPow);
            }


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

    private async UniTask BeforeDelay(CancellationToken ct)
    {
        _mashine.Ani.Play(_mashine.HashAttReady);
        _mashine.Nav.velocity = Vector3.zero;
        
        await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay), cancellationToken: ct);
    }

    private async UniTask<float> TrackingAim(CancellationToken ct)
    {
        _mashine.Ani.Play(_mashine.HashAttack);

        Vector2 myPos = _mashine.transform.position;
        Vector2 targetPos = _mashine.Target.position;

        // 라인 생성
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
        for (float time = 0f; time < _trackingTime; time += Time.deltaTime)
        {
            myPos = _mashine.transform.position;
            targetPos = _mashine.Target.position;
            Vector2 dir = targetPos - myPos;

            
            // 타겟 각도 갱신
            targetAngle = DirToAngle(dir.normalized);

            // 에임 따라가기
            aimAngle = Mathf.MoveTowardsAngle(aimAngle, targetAngle, Time.deltaTime * _trackingSpeed);

            Vector2 aimDir = AngleToDir(aimAngle);
            Vector2 aimDir90 = new Vector2(-aimDir.y, aimDir.x);
            Vector2 linePos = myPos + aimDir * _lineLength * 0.5f;
            Quaternion qua = Quaternion.Euler(0, 0, aimAngle);
            float gap = Mathf.Lerp(_interbal, 0f, time / _trackingTime);

            // 선 따라가기
            for (int i = 0; i < lines.Length; i++)
            {
                lines[i].transform.SetPositionAndRotation(linePos + (aimDir90 * gap * sides[i]), qua);
            }

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }
        return aimAngle;
    }

    private void BulletFire(float aim, float power)
    {
        Vector2 aimDir = AngleToDir(aim);
        Vector2 myPos = _mashine.transform.position;

        GameObject bullet = CreateAttackObject
        (_projectile, _bulletLifeTime + 1f, myPos + aimDir.normalized * 0.5f, Quaternion.identity);

        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(aimDir * power, ForceMode2D.Impulse);
        }
        CEnemyProjectile pro = bullet.GetComponent<CEnemyProjectile>();
        if (pro != null)
        {
            pro.DamageSet(_mashine.Attack, aimDir.normalized, _bulletLifeTime);
        }
    }

    

}
