
using System;
using UnityEngine;

public class CWeaponManager : MonoBehaviour
{
    public enum EWeaponType
    {
        SwordShield,
        Crossbow
    }

    #region 인스펙터

    [Header("무기 세트 목록")]
    [SerializeField] private CSwordShieldSetSO[] _weaponSets;

    [Header("시작 장비")]
    [Min(1)]
    [SerializeField] private int _startingTier = 1;

    [Header("전투 컴포넌트")]
    [SerializeField] private CPlayerAttack _playerAttack;
    [SerializeField] private CPlayerSpecialAttack _specialAttack;
    [SerializeField] private CPlayerGuard _playerGuard;
    [SerializeField] private CWeaponMotion _weaponMotion;

    [Header("검과 방패 외형")]
    [SerializeField] private SpriteRenderer _swordRenderer;
    [SerializeField] private SpriteRenderer _shieldRenderer;

    [Header("석궁 세트 목록")]
    [SerializeField] private CCrossbowDataSO[] _crossbowSets;

    [Header("석궁 공격")]
    [SerializeField] private CPlayerCrossbowAttack _crossbowAttack;

    [Header("석궁 외형")]
    [SerializeField] private SpriteRenderer _crossbowRenderer;

    [Header("플레이어 데이터")]
    [SerializeField] private CPlayerController _player;
    #endregion

    #region 내부변수
    private CPlayerGamingDataSO _playerData;
    private CSwordShieldSetSO _currentSet;
    private CCrossbowDataSO _currentCrossbow;

    // 초기값 → 검방
    private EWeaponType _currentWeaponType = EWeaponType.SwordShield;
    private int _currentTier;


    #endregion

    #region 프로퍼티
    public EWeaponType CurrentWeaponType => _currentWeaponType;
    public int CurrentTier => _currentTier;
    public CSwordShieldSetSO CurrentSet => _currentSet;
    public CCrossbowDataSO CurrentCrossbow => _currentCrossbow;

    #endregion

    #region 이벤트

    public event Action<CSwordShieldSetSO> OnWeaponSwapped;
    public event Action<CCrossbowDataSO> OnCrossbowSwapped;

    // 무기 종류를 포함한 공통 이벤트
    public event Action<EWeaponType, int> OnWeaponChanged;
    #endregion

    #region Unity
    private void Awake()
    {
        
    }

    private void Start()
    {
       
    }

 

    #endregion

    #region Swap

    // UI 버튼이나 테스트 입력에서 호출
    public void EquipTier(int tier)
    {
        //TrySwapTier(tier);
    }

    public bool TrySwapTier(int tier)
    {
        return TrySwapWeapon(_currentWeaponType, tier);
        /*
        CSwordShieldSetSO nextSet = FindSet(tier);

        if (nextSet == null)
        {
            CLog.Warn($"[WeaponSwap] Tier {tier} 데이터 없음");
            return false;
        }

        if (!ValidateSet(nextSet))
        {
            return false;
        }

        // 이미 같은 티어라면 예약만 취소
        if (_currentSet == nextSet)
        {
            _pendingTier = -1;
            return true;
        }       

        _pendingTier = -1;
        ApplySet(nextSet);
        */

        //return true;
    }
    // 신규 공통 무기 교체 API
    public bool TrySwapWeapon(EWeaponType type, int tier)
    {
        if (!ValidateReferences())
        {
            return false;
        }

        // 이미 장착한 무기
        if (_currentTier == tier &&  _currentWeaponType == type)
        {
            return true;
        }


        switch (type)
        {
            case EWeaponType.SwordShield:
                {
                    CSwordShieldSetSO set = FindSwordShield(tier);

                    if (!ValidateSwordShield(set))
                    {
                        return false;
                    }

                    ApplySet(set);
                    return true;
                }

            case EWeaponType.Crossbow:
                {
                    CCrossbowDataSO crossbow = FindCrossbow(tier);

                    if (!ValidateCrossbow(crossbow))
                        return false;

                    ApplyCrossbow(crossbow);
                    return true;
                }
        }

        return false;
    }

    private void ApplySet(CSwordShieldSetSO nextSet)
    {
        _playerData = _player.PlayerData;

        if(_playerData == null)
        {
            CLog.Log("[CWeaponManager] PlayerData 비어있음");
            
        }

        CLog.Log($"[CWeaponManager] {nextSet.StatBonus} 확인");

        if (nextSet == null)
        {
            return;
        }               

        // 1. 일반공격 데이터 변경
        _playerAttack.SetWeaponData(nextSet.SwordData);

        // 2. 특수공격 데이터 및 MP 비용 변경
        _specialAttack.SetSpecialAttackData(nextSet.SpecialAttackData, nextSet.SpecialManaCost);

        // 3. 방패 설정 변경
        _playerGuard.SetShieldSettings(
            nextSet.GuardManaCost,
            nextSet.PerfectGuardWindow,
            nextSet.GuardBreakDuration);

        // 4. 플레이어 스탯
        _playerData.SetWeaponStatBonus(nextSet.StatBonus);

        // 5. 스프라이트 변경
        ApplyWeaponSprite(nextSet);

        // 6. 현재 티어 저장
        _playerData.SetEquippedWeaponTier(nextSet.Tier);

        _currentSet = nextSet;

        // 장비 UI 등 외부 시스템에 변경 알림
        OnWeaponSwapped?.Invoke(nextSet);
    }

    private void ApplyWeaponSprite(CSwordShieldSetSO weaponSet)
    {
        if (weaponSet == null)
        {
            return;
        }

        #region ※테스트
        if (_swordRenderer == null || _shieldRenderer == null)
        {
            CLog.Warn("[WeaponSwap] Renderer 참조 누락");
            return;
        }

        if (_swordRenderer == _shieldRenderer)
        {
            CLog.Warn("[WeaponSwap] 검과 방패 Renderer가 동일함");
            return;
        }

        #endregion

        // 검 스프라이트 변경
        if (_swordRenderer != null)
        {
            _swordRenderer.sprite = weaponSet.SwordSprite;
        }

        // 방패 스프라이트 변경
        if (_shieldRenderer != null)
        {
            _shieldRenderer.sprite = weaponSet.ShieldSprite;
        }

        CLog.Log($"[WeaponSwap] Tier {weaponSet.Tier} 스프라이트 적용 완료");

        #region ※테스트
        CLog.Log(
            $"[WeaponSwap] 방패 변경 후 : " +
            $"{_shieldRenderer.sprite?.name}");

        // 참조하고 있는 GameObject 확인
        CLog.Log(
            $"[WeaponSwap] 방패 Renderer 오브젝트 : " +
            $"{_shieldRenderer.gameObject.name}");

        // 현재 티어 데이터 확인
        CLog.Log(
            $"[WeaponSwap] Tier : {weaponSet.Tier} / " +
            $"SO 방패 : {weaponSet.ShieldSprite?.name}");

        #endregion
        // 변경 후 확인
    }

    #endregion

    #region Search

    private CSwordShieldSetSO FindSwordShield(int tier)
    {
        if (_weaponSets == null)
        {
            return null;
        }

        foreach (CSwordShieldSetSO set in _weaponSets)
        {
            if (set != null && set.Tier == tier)
            {
                return set;
            }
        }

        return null;
    }

    private CCrossbowDataSO FindCrossbow(int tier)
    {
        if(_crossbowSets == null)
        {
            return null;
        }

        foreach (CCrossbowDataSO set in _crossbowSets)
        {
            if (set != null && set.Tier == tier)
            {
                return set;
            }
        }

        return null;
    }

    #endregion

    #region Validation

    private bool ValidateReferences()
    {
        _playerData = _player.PlayerData;

        if (_playerAttack == null || _specialAttack == null || _playerGuard == null || _weaponMotion == null || _crossbowAttack == null)
        {
            CLog.Warn("[WeaponSwap] 전투 컴포넌트 참조 누락");
            return false;
        }

        if (_swordRenderer == null || _shieldRenderer == null || _crossbowRenderer == null)
        {
            CLog.Warn("[WeaponManager] SpriteRenderer 누락");
            return false;
        }

        if (_swordRenderer == _shieldRenderer || _swordRenderer == _crossbowRenderer || _shieldRenderer == _crossbowRenderer)
        {
            CLog.Warn("[WeaponManager] Renderer 중복 참조");
            return false;
        }


        return true;
    }

    private bool ValidateSwordShield(CSwordShieldSetSO set)
    {
        if (set.SwordData == null || set.SpecialAttackData == null)
        {
            CLog.Warn($"[WeaponSwap] Tier {set.Tier} " + "공격 데이터 누락");

            return false;
        }

        if (_swordRenderer != null && set.SwordSprite == null)
        {
            CLog.Warn($"[WeaponSwap] Tier {set.Tier} " + "검 스프라이트 누락");

            return false;
        }

        if (_shieldRenderer != null && set.ShieldSprite == null)
        {
            CLog.Warn($"[WeaponSwap] Tier {set.Tier} " + "방패 스프라이트 누락");

            return false;
        }

        return true;
    }

    private bool ValidateCrossbow(CCrossbowDataSO data)
    {
        if (data == null || data.CrossbowSprite == null || data.ProjectilePrefab == null)
        {
            CLog.Warn("[WeaponManager] 석궁 데이터 누락");
            return false;
        }

        // 프리팹 루트에 필수 컴포넌트가 있어야 함(없을 시에만 사용되므로 안정감 올릴 수 있고, 성능저하는 크게 없음)
        if (!data.ProjectilePrefab.TryGetComponent(out CCrossbowProjectile projectile))
        {
            CLog.Warn("[WeaponManager] 투사체 프리팹에 " + "CCrossbowProjectile 누락");

            return false;
        }

        return true;
    }

    #endregion

    #region Weapon Swap Test

    // Test : 1티어 석궁 장착 ※
    public void TestEquipCrossbow()
    {
        TrySwapWeapon(EWeaponType.Crossbow, 1);
    }

    // Test : 1티어 검·방패 장착 ※
    public void TestEquipSwordShield()
    {
        TrySwapWeapon(EWeaponType.SwordShield, 1);
    }

    // 테스트 버튼을 누를 때마다 다음 티어 장착 ※
    public void TestSwapNextTier()
    {
        const int maxTier = 2;

        int nextTier = CurrentTier + 1;

        // 3티어 다음에는 1티어로 돌아감
        if (nextTier > maxTier)
        {
            nextTier = 1;
        }

        // 기존 무기 교체 함수를 그대로 사용
        if (!TrySwapTier(nextTier))
        {
            CLog.Warn($"[WeaponTest] Tier {nextTier} 변경 실패");
            return;
        }

        CLog.Log($"[WeaponTest] Tier {nextTier} 변경 요청");
    }

    #endregion
}
