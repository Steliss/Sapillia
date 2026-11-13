using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public enum EWeaponType
{
    SwordShield,
    Crossbow
}

public class CWeaponManager : MonoBehaviour
{

    #region 인스펙터
    [Header("무기 조준")]
    [SerializeField] private CCrossbowAim _crossbowAim;
    [SerializeField] private CSwordAndShieldAim _swordAim;

    [Header("무기 세트 목록")]
    [SerializeField] private CSwordShieldSetSO[] _weaponSets;

    [Header("시작 장비")]
    [SerializeField] private EWeaponType _startingWeaponType = EWeaponType.SwordShield;

    [Min(1)]
    [SerializeField] private int _startingTier = 1;

    [Header("전투 컴포넌트")]
    [SerializeField] private CPlayerAttack _playerAttack;
    [SerializeField] private CPlayerSpecialAttack _specialAttack;
    [SerializeField] private CPlayerGuard _playerGuard;
    [SerializeField] private CWeaponMotion _weaponMotion;

    [Header("강화 후보")]
    [SerializeField] private CWeaponUpgradeOptionSO[] _upgradeOptions;

    [Header("검과 방패 외형")]
    [SerializeField] private SpriteRenderer _swordRenderer;
    [SerializeField] private SpriteRenderer _shieldRenderer;

    [Header("석궁 세트 목록")]
    [SerializeField] private CCrossbowDataSO[] _crossbowSets;

    [Header("석궁 공격")]
    [SerializeField] private CPlayerCrossbowAttack _crossbowAttack;
    [SerializeField] private CCrossbowSpecialAttack _crossbowSpecialAttack;

    [Header("석궁 외형")]
    [SerializeField] private SpriteRenderer _crossbowRenderer;

    [Header("플레이어 데이터")]
    [SerializeField] private CPlayerController _player;
    #endregion

    #region 내부변수
    private CPlayerGamingDataSO _playerData;
    private CSwordShieldSetSO _currentSet;
    private CCrossbowDataSO _currentCrossbow;
    private CDataManger _data;

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

    // 초기 무기 장착 작업
    #region 유니테스크

    private CUniTaskOnes _weaponInitTask = new CUniTaskOnes();
    private bool _isStartingWeaponEquipped;

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
        // 무기 외형 초기 비활성화
        if (_swordRenderer != null)
        {
            _swordRenderer.enabled = false;
        }

        if (_shieldRenderer != null)
        {
            _shieldRenderer.enabled = false;
        }

        if (_crossbowRenderer != null)
        {
            _crossbowRenderer.enabled = false;
        }

        // 검·방패 전투 비활성화
        if (_playerAttack != null)
        {
            _playerAttack.enabled = false;
        }

        if (_playerGuard != null)
        {
            _playerGuard.enabled = false;
        }

        // 석궁 전투 비활성화
        if (_crossbowAttack != null)
        {
            _crossbowAttack.enabled = false;
        }

        if (_crossbowSpecialAttack != null)
        {
            _crossbowSpecialAttack.enabled = false;
        }

        // 조준 스크립트 비활성화
        if (_swordAim != null)
        {
            _swordAim.enabled = false;
        }

        if (_crossbowAim != null)
        {
            _crossbowAim.enabled = false;
        }
    }  

    private void OnEnable()
    {
        _weaponInitTask.Run(async token =>
        {
            // 데이터 매니저 생성 대기
            await CManagerDontDestroy.DataMangerAsync(token);

            // 플레이어 데이터 연결 및 초기화 완료 대기
            await UniTask.WaitUntil(
            () =>
                _player != null && _player.PlayerData != null && _player.PlayerData.IsInitialized,
                cancellationToken: token
            );
            
            token.ThrowIfCancellationRequested();

            // 중복 실행 방지
            if (!isActiveAndEnabled || _isStartingWeaponEquipped)
            {
                return;
            }

            InitializeStartingWeapon();

            _data = await CManagerDontDestroy.DataMangerAsync(token);
            CLog.Log("[CPlayerCostumeApplier] 데이터매니저 작업 완료");
        }
        );
    }

    private void OnDisable()
    {
        _weaponInitTask.Cancel();
    }

    #endregion

    #region StartWeapon Equip
    public void InitializeStartingWeapon()
    {
        if (!TrySwapWeapon(_startingWeaponType, _startingTier))
        {
            CLog.Warn($"[WeaponManager] 시작 무기 장착 실패 : " + $"{_startingWeaponType} / Tier {_startingTier}");
        }
    }

    #endregion

    #region Swap
       

    public bool TrySwapTier(int tier)
    {
        return TrySwapWeapon(_currentWeaponType, tier);
   
    }
    // 공통 무기 교체
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
                    _data.SaveGame(_data.CurrentFlow.SaveSlot);
                    return true;
                }

            case EWeaponType.Crossbow:
                {
                    CCrossbowDataSO crossbow = FindCrossbow(tier);

                    if (!ValidateCrossbow(crossbow))
                    {
                        return false;
                    }

                    ApplyCrossbow(crossbow);
                    _data.SaveGame(_data.CurrentFlow.SaveSlot);
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

        if (nextSet == null)
        {
            return;
        }

        // 기존 석궁 공격 입력 해제
        _crossbowAttack.enabled = false;

        // 일반공격 데이터 변경
        _playerAttack.SetWeaponData(nextSet.SwordData);

        // 특수공격 데이터 및 MP 비용 변경
        _specialAttack.SetSpecialAttackData(nextSet.SpecialAttackData, nextSet.SpecialManaCost);

        // 방패 설정 변경
        _playerGuard.SetShieldSettings
            (
                nextSet.GuardManaCost,
                nextSet.PerfectGuardWindow,
                nextSet.GuardBreakDuration
            );               

        // 스프라이트 변경
        ApplyWeaponSprite(nextSet);

        // 석궁 관련 비활성화 및 검과방패 활성화
        _swordRenderer.enabled = true;
        _shieldRenderer.enabled = true;
        _crossbowRenderer.enabled = false;

        // 장착 정보
        _currentSet = nextSet;
        _currentCrossbow = null;

        _currentWeaponType = EWeaponType.SwordShield;
        _currentTier = nextSet.Tier;

        // 플레이어 스탯
        _playerData.SetEquippedWeapon(EWeaponType.SwordShield, nextSet.Tier);        
        _playerData.SetWeaponStatBonus(nextSet.StatBonus);

        // 검·방패 공격 입력 활성화
        _swordAim.enabled = true;
        _crossbowAim.enabled = false;

        _crossbowSpecialAttack.enabled = false;

        _playerAttack.enabled = true;
        _playerGuard.enabled = true;

        // 장비 UI 등 외부 시스템에 변경 알림
        OnWeaponSwapped?.Invoke(nextSet);
        OnWeaponChanged?.Invoke(_currentWeaponType, _currentTier);

        // 저장
        _data.SaveGame(_data.CurrentFlow.SaveSlot);
        CLog.Log($"[WeaponManager] 검·방패 {nextSet.SetName} 장착");
    }

    // 석궁 변경 장착
    private void ApplyCrossbow(CCrossbowDataSO crossbow)
    {
        if (crossbow == null)
        {
            return;
        }

        // 기존 검·방패 입력 해제
        _playerAttack.enabled = false;
        _playerGuard.enabled = false;

        // 석궁 공격 데이터 설정
        _crossbowAttack.SetCrossbowData(crossbow);
        _crossbowSpecialAttack.SetCrossbowData(crossbow);

        // 검·방패 외형 숨기기
        _swordRenderer.enabled = false;
        _shieldRenderer.enabled = false;

        // 석궁 외형 적용
        _crossbowRenderer.sprite = crossbow.CrossbowSprite;
        _crossbowRenderer.enabled = true;

        // 장착 정보
        _currentSet = null;
        _currentCrossbow = crossbow;

        _currentWeaponType = EWeaponType.Crossbow;
        _currentTier = crossbow.Tier;

        // 플레이어 능력치 적용
        _playerData.SetEquippedWeapon(EWeaponType.Crossbow, crossbow.Tier);
        _playerData.SetWeaponStatBonus(crossbow.StatBonus);
        _playerData.InitializeCrossbowAmmo();
        CLog.Log($"[CWeaponManager] 탄 개수 : {_playerData.CurrentAmmo}");

        // 석궁 입력 활성화
        _crossbowAttack.enabled = true;
        _crossbowSpecialAttack.enabled = true;

        _swordAim.enabled = false;
        _crossbowAim.enabled = true;

        // 공통 변경 이벤트
        OnWeaponChanged?.Invoke(_currentWeaponType, _currentTier);

        // 저장
        _data.SaveGame(_data.CurrentFlow.SaveSlot);
        CLog.Log($"[WeaponManager] 석궁 {crossbow.Tier}티어 장착");
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

        // 검 방패 스프라이트 변경
        _swordRenderer.sprite = weaponSet.SwordSprite;
        _shieldRenderer.sprite = weaponSet.ShieldSprite;

        CLog.Log($"[WeaponSwap] Tier {weaponSet.Tier} 스프라이트 적용 완료");

        #region ※테스트
        CLog.Log($"[WeaponSwap] 방패 변경 후 : " + $"{_shieldRenderer.sprite?.name}");

        // 참조하고 있는 GameObject 확인
        CLog.Log($"[WeaponSwap] 방패 Renderer 오브젝트 : " + $"{_shieldRenderer.gameObject.name}");

        // 현재 티어 데이터 확인
        CLog.Log($"[WeaponSwap] Tier : {weaponSet.Tier} / " + $"SO 방패 : {weaponSet.ShieldSprite?.name}");

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

        if(_player == null || _player.PlayerData == null)
        {
            CLog.Warn($"[WeaponManager] PlayerData 연결 X");
            return false;
        }

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
        if(set== null)
        {
            CLog.Warn("[WeaponManager] 검과 방패 데이터가 없음");
            return false;
        }

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
        if (data == null || data.CrossbowSprite == null)
        {
            CLog.Warn("[WeaponManager] 석궁 데이터 누락");
            return false;
        }

        switch (data.FireMode)
        {
            case ECrossbowFireMode.Projectile:
                if (data.ProjectilePrefab == null || !data.ProjectilePrefab.TryGetComponent(out CCrossbowProjectile projectile))
                {
                    CLog.Warn("[WeaponManager] 화살 프리팹 누락");
                    return false;
                }

                break;

            case ECrossbowFireMode.Blast:
                if (data.BlastEffectPrefab == null ||!data.BlastEffectPrefab.TryGetComponent(out CShotgunBlastEffect effect))
                {
                    CLog.Warn("[WeaponManager] 샷건 이펙트 누락");
                    return false;
                }
                    break;
            default:
                return false;
        }       

        return true;
    }

    #endregion

    #region Weapon Upgrade
    public bool HasWeapon(EWeaponType type, int tier)
    {
        if (tier < 1)
        {
            return false;
        }

        switch (type)
        {
            case EWeaponType.SwordShield:
                return FindSwordShield(tier) != null;

            case EWeaponType.Crossbow:
                return FindCrossbow(tier) != null;
        }

        return false;
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

    #region 강화 무기 후보군
    public void GetAvailableUpgradeOptions(EWeaponType currentType, int currentTier, List<CWeaponUpgradeOptionSO> results)
    {
        if (results == null)
        {
            return;
        }

        results.Clear();

        if (_upgradeOptions == null)
        {
            return;
        }

        foreach (CWeaponUpgradeOptionSO option in _upgradeOptions)
        {
            if (option == null)
            {
                continue;
            }

            if (!option.IsMatch(currentType, currentTier))
            {
                continue;
            }

            // 정확히 다음 티어인지 확인
            if (option.ToTier != currentTier + 1)
            {
                continue;
            }

            // 실제 장착할 무기 데이터 확인
            if (!HasUpgradeTarget(option))
            {
                continue;
            }

            results.Add(option);
        }
    }

    public bool TryApplyUpgrade(CWeaponUpgradeOptionSO option)
    {
        if (option == null || !HasUpgradeTarget(option))
        {
            CLog.Warn("[WeaponManager] 강화 후보 없음");
            return false;
        }

        // 등록된 강화 후보인지 확인
        if (_upgradeOptions == null || Array.IndexOf(_upgradeOptions, option) < 0)
        {
            return false;
        }

        if (!ValidateReferences() || _playerData == null || !_playerData.IsInitialized)
        {
            return false;
        }

        // 현재 무기와 정확히 연결되는 강화만 허용
        if (!option.IsMatch(_playerData.WeaponType, _playerData.EquippedWeaponTier))
        {
            return false;
        }

        if (option.ToTier != _playerData.EquippedWeaponTier + 1)
        {
            return false;
        }

        switch (option.WeaponType)
        {
            case EWeaponType.SwordShield:                
                CSwordShieldSetSO set = option.SwordShieldSet;

                if (!ValidateSwordShield(set))
                {
                    return false;
                }

                ApplySet(set);
                return true;
                

            case EWeaponType.Crossbow:               
                CCrossbowDataSO crossbow = option.CrossbowData;

                if (!ValidateCrossbow(crossbow))
                {
                    return false;
                }

                ApplyCrossbow(crossbow);
                return true;                
        }  

        return false;
    }
    #endregion

    #region 무기
    // 검방
    private bool TryApplySwordShieldUpgrade(CSwordShieldSetSO nextSet)
    {
        if (nextSet == null)
        {
            CLog.Warn("[WeaponManager] SwordShieldSet 누락");
            return false;
        }

        ApplySet(nextSet);

        // 현재 장착 정보 저장
        if (_playerData != null)
        {
            _playerData.SetEquippedWeapon(EWeaponType.SwordShield, nextSet.Tier);
        }

        OnWeaponChanged?.Invoke(EWeaponType.SwordShield, nextSet.Tier);
        return true;
    }

    // 석궁
    private bool TryApplyCrossbowUpgrade(CCrossbowDataSO nextData)
    {
        if (nextData == null)
        {
            CLog.Warn("[WeaponManager] CrossbowData 누락");
            return false;
        }

        ApplyCrossbow(nextData);

        if (_playerData != null)
        {
            _playerData.SetEquippedWeapon(EWeaponType.Crossbow, nextData.Tier);
        }

        OnWeaponChanged?.Invoke(EWeaponType.Crossbow, nextData.Tier);
        return true;
    }

    #endregion

    private bool HasUpgradeTarget(CWeaponUpgradeOptionSO option)
    {
        if (option == null)
        {
            return false;
        }

        switch (option.WeaponType)
        {
            case EWeaponType.SwordShield:
                return option.SwordShieldSet != null &&
                       option.SwordShieldSet.Tier == option.ToTier;

            case EWeaponType.Crossbow:
                return option.CrossbowData != null &&
                       option.CrossbowData.Tier == option.ToTier;
        }

        return false;
    }
}
