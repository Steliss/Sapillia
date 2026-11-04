using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class CPlayerCrossbowAttack : MonoBehaviour, IInputCommand<float>, IInputCommand
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

    // 입력 시스템 / 탄창 시스템
    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _reloadTask = new CUniTaskOnes();    

    private bool _isReloading;

    private bool _isFireHeld;
    private float _nextFireTime;

    #endregion

    #region 프로퍼티

    public bool IsFiring => _isFireHeld;
    public bool IsReloading => _isReloading;
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
            
            // 좌클릭 공격
            _inputHub.Register(EKeyAction.Attack, (IInputCommand<float>)this);
            // G키 재장전
            _inputHub.Register(EKeyAction.Reloading, (IInputCommand)this);
            CLog.Log("[Crossbow] 입력 등록 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _reloadTask.Cancel();

        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Attack, (IInputCommand<float>)this);
            _inputHub.Unregister(EKeyAction.Reloading, (IInputCommand)this);
        }

        _isFireHeld = false;
        _isReloading = false;
        _nextFireTime = 0f;
    }

    private void Update()
    {
        if (!_isFireHeld || _isReloading)
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

    public void Execute() // G키 : 재장전
    {
        TryReload();
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
        GameObject projectileObject = CObjectPoolManager.Instance.GetPoolObject(_crossbowData.ProjectilePrefab, _firePoint);

        if (projectileObject == null)
        {
            CLog.Log("[CCrossbowAttack] 화살 프리팹 비어있음");
            return;
        }

        // 투사체 컴포넌트 확인
        if (!projectileObject.TryGetComponent(out CCrossbowProjectile projectile))
        {
            CLog.Warn("[Crossbow] Projectile 컴포넌트 누락");

            return;
        }

        // 석궁이 바라보는 방향으로 발사
        projectile.Launch
            (
                _firePoint.right,
                _crossbowData.ProjectileSpeed,
                _crossbowData.ProjectileRange,
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

    #region Reload

    private void TryReload()
    {
        if (_isReloading || _crossbowData == null || _player == null || _player.PlayerData == null)
        {
            return;
        }

        CPlayerGamingDataSO playerData = _player.PlayerData;

        // 예비 탄창이 없으면 재장전 불가
        // 현재 탄약이 가득 찼는지는 검사하지 않음
        if (!playerData.CanReloadMagazine)
        {
            CLog.Log("[Crossbow] 예비 탄창 없음");
            return;
        }

        SPlayerStats stats = playerData.Stats;

        // 재장전 속도 계산
        float reloadSpeed = Mathf.Max(0.01f, stats.Ammo_ReloadSpeed);

        float reloadTime = Mathf.Max(0.01f, _crossbowData.ReloadDuration / reloadSpeed);

        _isReloading = true;

        CLog.Log($"[Crossbow] 재장전 시작 : {reloadTime:F2}초");

        _reloadTask.Run(async token =>
        {
            try
            {
                // 재장전 시간 대기
                await UniTask.Delay(TimeSpan.FromSeconds(reloadTime), cancellationToken: token);

                token.ThrowIfCancellationRequested();

                if (!isActiveAndEnabled || !_isReloading)
                {
                    return;
                }

                // 완료 시 탄창 교체
                if (playerData.TryReloadMagazine())
                {
                    CLog.Log(
                        $"[Crossbow] 재장전 완료 / " +
                        $"Ammo : {playerData.CurrentAmmo}/" +
                        $"{playerData.MaxAmmo} / " +
                        $"Magazine : {playerData.CurrentMagazineCount}");
                }

                _isReloading = false;
            }
            catch (OperationCanceledException)
            {                
            }
        });
    }

    #endregion
}
