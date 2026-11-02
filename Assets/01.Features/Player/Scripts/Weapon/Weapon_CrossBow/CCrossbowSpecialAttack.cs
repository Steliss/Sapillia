using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CCrossbowSpecialAttack : MonoBehaviour, IInputCommand<float>
{
	#region 인스펙터
	[Header("입력")]
	[SerializeField] private CInputCommandHub _inputHub;

	[Header("플레이어")]
	[SerializeField] private CPlayerController _player;
	[SerializeField] private CPlayerMovement _movement;

	[Header("석궁")]
	[SerializeField] private CCrossbowAim _crossbowAim;
	[SerializeField] private Transform _firePoint;

	#endregion

	#region 내부 변수
	private CUniTaskOnes _inputTask = new CUniTaskOnes();
	private CCrossbowDataSO _crossbowData;
	private bool _isButtonHeld;
	private float _nextSpecialTime;

    #endregion

    #region Unity 생명주기

    private void OnEnable()
    {
		_inputTask.Run(async token =>
		{
        _inputHub = await CManagerDontDestroy.InputCommandHubAsync(token);
        _inputHub.Register(EKeyAction.SpecialAttack, (IInputCommand<float>)this);
        CLog.Log("CCrossbow OnEnable 작업 완료");
        });

    }

    private void OnDisable()
    {
        _inputTask.Cancel();

        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.SpecialAttack, (IInputCommand<float>)this);
        }

        _isButtonHeld = false;
    }

    #endregion

    #region Input
    public void Execute(float value)
    {
        bool pressed = value > 0.5f;

        // 우클릭 해제
        if (!pressed)
        {
            _isButtonHeld = false;
            return;
        }

        _isButtonHeld = true;

        TrySpecialAttack();
    }
    #endregion


    #region 특수공격
    private void TrySpecialAttack()
    {
        if (_crossbowData == null || _player == null || _movement == null || _crossbowAim == null || _firePoint == null)
        {
            CLog.Error("[CCrossbowSpecialAttack] 필수 참조가 비어있음");
            return;
        }

        // 특수공격 발사 간격
        if (Time.time < _nextSpecialTime)
        {
            return;
        }

        CPlayerGamingDataSO playerData = _player.PlayerData;

        if (playerData == null || !playerData.IsInitialized)
        {
            CLog.Warn("[CrossbowSpecial] 플레이어 데이터 오류");
            return;
        }

        // MP 확인
        if (playerData.CurrentMP < _crossbowData.SpecialManaCost)
        {
            CLog.Warn("[CrossbowSpecial] MP 부족");
            return;
        }

        GameObject prefab = _crossbowData.SpecialProjectilePrefab;
        CObjectPoolManager pool = CObjectPoolManager.Instance;

        if (prefab == null || pool == null)
        {
            CLog.Warn("[CrossbowSpecial] 투사체 또는 풀 누락");
            return;
        }

        if (_crossbowData.SpecialProjectileSpeed <= 0f)
        {
            return;
        }

        // 발사 직전 최신 마우스 방향 반영
        _crossbowAim.RefreshAim();

        Vector2 fireDirection = _firePoint.right;

        if (fireDirection.sqrMagnitude < 0.0001f)
        {
            return;
        }

        fireDirection.Normalize();

        // 플레이어 최종 능력치
        SPlayerStats stats = playerData.Stats;

        // 석궁 기본 데미지 × 플레이어 특수공격 배율
        float damage = _crossbowData.BaseDamage * stats.Damage_All * stats.Damage_Weapon * stats.Damage_Special;

        int finalDamage = Mathf.Max(0, Mathf.RoundToInt(damage) + stats.Damage_Fixed);

        // 공용 풀에서 스킬 투사체 한 개 획득
        GameObject projectileObject = pool.GetPoolObject(prefab, _firePoint);

        if (projectileObject == null)
        {
            return;
        }

        if (!projectileObject.TryGetComponent(out CCrossbowProjectile projectile))
        {
            CLog.Warn("[CrossbowSpecial] Projectile 누락");
            pool.ReturnPool(projectileObject);
            return;
        }

        // 실제 발사 직전에 MP 소비
        if (!playerData.TryUseMP(_crossbowData.SpecialManaCost))
        {
            pool.ReturnPool(projectileObject);
            return;
        }

        // 투사체 발사
        projectile.Launch
            (
                fireDirection,
                _crossbowData.SpecialProjectileSpeed,
                _crossbowData.SpecialProjectileRange,
                finalDamage,
                _player.transform
            );

        // 발사 방향의 반대로 반동
        _movement.ApplyRecoil(-fireDirection, _crossbowData.SpecialRecoilSpeed, _crossbowData.SpecialRecoilDeceleration);
        _nextSpecialTime = Time.time + _crossbowData.SpecialCooldown;

        CLog.Log($"[CrossbowSpecial] 발사 / MP -{_crossbowData.SpecialManaCost} / " + $"Damage : {finalDamage}");
    }

    #endregion

    #region Data

    public void SetCrossbowData(CCrossbowDataSO data)
    {
        _crossbowData = data;
        _isButtonHeld = false;
    }

    #endregion

}
