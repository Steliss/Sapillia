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
    [Header("선딜시간")]
    [SerializeField] private float _beforeDelay = 2f;
    [Header("투사체 속성")]
    [SerializeField] private int _multiFire = 1;
    [SerializeField] private float _firePower = 3f;
    [SerializeField] private float _bulletLifeTime = 3f;
    [SerializeField] private float _randomPowerRange = 0.5f;
    [SerializeField] private float _randomAngleRange = 5f;

    [Space(10)]
    [Header("===== 전조 =====")]
    [SerializeField] private SLineInfo _lineInfo;
    [Header("===== 사운드 =====")]
    [SerializeField] private string _soundIng = "AttackDynamiteRed";
    [SerializeField] private string _soundEnd = "HitDynamite";
    #endregion


    public override void StateEnter()
    {
        _mashine.AttackUni.Cancel();
        _mashine.AttackUni.Run(async ct =>
        {
            // 선딜
            _mashine.Ani.Play(_mashine.HashAttReady);
            _mashine.SoundPlay(_soundIng);
            await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay), cancellationToken: ct);

            // 조준
            _mashine.Ani.Play(_mashine.HashAttack);
            _mashine.SoundPlay(_soundIng);
            float aim = await WarningLine_FireAngle(ct, _lineInfo);

            // 발사
            _mashine.SoundPlay(_soundEnd);
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


    private void BulletFire(float aim, float power)
    {
        Vector2 aimDir = AngleToDir(aim);
        Vector2 myPos = _mashine.transform.position;

        GameObject bullet = null;

        if (CObjectPoolManager.Instance != null)
        {
            bullet = CObjectPoolManager.Instance.GetPoolObject(_projectile, _bulletLifeTime + 1f);
            bullet.transform.position = myPos + aimDir.normalized * 0.5f;
        }

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
