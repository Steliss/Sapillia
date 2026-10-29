
using UnityEngine;

public class CPlayerCrossbowAttack : MonoBehaviour, IInputCommand<float>
{
    #region 인스펙터
    [Header("입력")]
    [SerializeField] private CInputCommandHub _inputHub;

    [Header("참조")]
    [SerializeField] private CPlayerController _player;
    [SerializeField] private Transform _firePoint;

    [Header("석궁 데이터")]
    [SerializeField] private CCrossbowDataSO _crossbowData;

    #endregion

    #region 내부변수

    private CUniTaskOnes _inputTask = new CUniTaskOnes();

    private bool _isFireHeld;
    private float _nextFireTime;

    #endregion

    #region 프로퍼티

    public bool IsFiring => _isFireHeld;

    #endregion

    #region Unity

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            CInputCommandHub hub = await CManagerDontDestroy.InputCommandHubAsync(token);

            if (token.IsCancellationRequested || !isActiveAndEnabled || hub == null)
            {
                return;
            }

            _inputHub = hub;

            _inputHub.Register(EKeyAction.Attack, (IInputCommand<float>)this);
            CLog.Log("[Crossbow] 입력 등록 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask.Cancel();

        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Attack, (IInputCommand<float>)this);
        }

        _isFireHeld = false;
        _nextFireTime = 0f;
    }

    private void Update()
    {
        if (!_isFireHeld)
        {
            return;
        }

        if (Time.time < _nextFireTime)
        {
            return;
        }

        TryFire();
    }

    #endregion

    #region Input

    public void Execute(float value) // 공격은 CWeaponMotion에서 관리
    {
        _isFireHeld = value > 0.5f;
    }

    #endregion

    #region Attack

    private void TryFire()
    {
        if (_crossbowData == null || _firePoint == null || _player == null || _player.PlayerData == null)
        {
            return;
        }

        if (CObjectPoolManager.Instance == null)
        {
            CLog.Warn("[Crossbow] ObjectPoolManager 없음");
            return;
        }

        if (_crossbowData.ProjectilePrefab == null)
        {
            CLog.Warn("[Crossbow] 투사체 프리팹 누락");
            return;
        }

        // 플레이어 최종 스탯
        SPlayerStats stats = _player.PlayerData.Stats;

        // 공격력 계산
        float damage = _crossbowData.BaseDamage * stats.Damage_All * stats.Damage_Weapon * stats.Damage_Normal;

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage) + stats.Damage_Fixed);

        // 기존 공용 오브젝트 풀 이용
        GameObject projectileObject = CObjectPoolManager.Instance.GetPoolObject(_crossbowData.ProjectilePrefab, _crossbowData.ProjectileLifetime);

        if (projectileObject == null)
        {
            return;
        }

        // 투사체 컴포넌트 확인
        if (!projectileObject.TryGetComponent(out CCrossbowProjectile projectile))
        {
            CLog.Warn("[Crossbow] Projectile 컴포넌트 누락");
            CObjectPoolManager.Instance.ReturnPool(projectileObject);

            return;
        }

        // 석궁이 바라보는 방향으로 발사
        projectile.Launch
            (
                _firePoint.position,
                _firePoint.right,
                _crossbowData.ProjectileSpeed,
                finalDamage,
                _player.transform
            );

        // 플레이어 공격 속도 반영
        float attackSpeed = Mathf.Max(0.01f, stats.Attack_Speed);

        _nextFireTime = Time.time + _crossbowData.AttackInterval / attackSpeed;
    }

    #endregion

    #region Weapon Data

    public void SetCrossbowData(CCrossbowDataSO data)
    {
        if (data == null)
        {
            return;
        }

        _crossbowData = data;

        _isFireHeld = false;
        _nextFireTime = 0f;

        CLog.Log($"[Crossbow] {data.WeaponName} 데이터 적용");
    }

    #endregion
}
