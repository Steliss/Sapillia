
using System;
using UnityEngine;

public class CWeaponManager : MonoBehaviour
{
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

    [Header("무기 외형")]
    [SerializeField] private SpriteRenderer _swordRenderer;
    [SerializeField] private SpriteRenderer _shieldRenderer;

    [Header("플레이어 데이터")]
    [SerializeField] private CPlayerController _player;
    #endregion

    #region 내부변수
    private CPlayerGamingDataSO _playerData;
    private CSwordShieldSetSO _currentSet;

    // 공격 중 장비 변경 요청을 기억
    private int _pendingTier = -1;

    #endregion

    #region 프로퍼티

    public CSwordShieldSetSO CurrentSet => _currentSet;
    public int CurrentTier => _currentSet != null ? _currentSet.Tier : 0;
    public bool HasPendingSwap => _pendingTier > 0;

    #endregion

    #region 이벤트

    public event Action<CSwordShieldSetSO> OnWeaponSwapped;

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
        TrySwapTier(tier);
    }

    public bool TrySwapTier(int tier)
    {
        if (!ValidateReferences())
        {
            return false;
        }

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

        return true;
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

    private CSwordShieldSetSO FindSet(int tier)
    {
        if (_weaponSets == null)
            return null;

        foreach (CSwordShieldSetSO set in _weaponSets)
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
        if (_playerAttack == null || _specialAttack == null || _playerGuard == null || _weaponMotion == null)
        {
            CLog.Warn("[WeaponSwap] 전투 컴포넌트 참조 누락");
            return false;
        }

        return true;
    }

    private bool ValidateSet(CSwordShieldSetSO set)
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

        if (_shieldRenderer != null &&
            set.ShieldSprite == null)
        {
            CLog.Warn($"[WeaponSwap] Tier {set.Tier} " + "방패 스프라이트 누락");

            return false;
        }

        return true;
    }

    #endregion

    #region Weapon Swap Test

    // 테스트 버튼을 누를 때마다 다음 티어 장착
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
