using System;
using UnityEngine;

/*
!추후에 아이템으로 기본 런타임 능력치 변경시 CoreStats 가져와서 SetStats()로 다시 전달할 것.!


*/

[CreateAssetMenu(menuName = "SO/Player/PlayerData", fileName = "PlayerData")]
public class CPlayerGamingDataSO : ScriptableObject
{
    #region 인스펙터
    [Header("Base Data")]
    [SerializeField] private CPlayerDataSO _baseData;

    #endregion

    #region 내부 변수
    private SCostumeStatBonus _costumeBonus; // 코스튬 추가 능력치
    private SPlayerStats _coreStats; // 코스튬 추가 능력치 제외 능력치
    private SPlayerStats _stats; // 최종 능력치

    private int _currentHP;
    private int _currentMP;
    private int _currentDashCount;

    private int _currentEXP;
    private int _requiredEXP;
    private int _level;
    private int _money;

    private float _mpRegenProgress;
    private float _dashRegenProgress;

    #endregion

    #region 프로퍼티
    public SPlayerStats Stats => _stats;
    public SPlayerStats CoreStats => _coreStats;
    public int CurrentHP => _currentHP;
    public int CurrentMP => _currentMP;
    public float CurrentDashCount => _currentDashCount;
    public int CurrentEXP => _currentEXP;
    public int RequiredEXP => _requiredEXP;
    public int Level => _level;
    public int Money => _money;
    public bool IsInitialized { get; private set; }
    #endregion

    #region 이벤트
    public event Action<SPlayerStats> OnStatsChanged;

    public event Action<int, int> OnHPChanged;
    public event Action<int, int> OnMPChanged;
    public event Action<int, int> OnDashCountChanged;

    public event Action<int, int> OnEXPChanged;
    public event Action<int> OnMoneyChanged;
    public event Action<int> OnLevelChanged;
    #endregion


    // 데이터 초기화
    public void Initalize()
    {
        IsInitialized = false;

        if (_baseData == null)
        {
            CLog.Warn("Player Data 누락");
            return;
        }

        // 기본 데이터를 현재 게임 데이터로 복사
        _coreStats = _baseData.BaseStats;
        _coreStats.Validate();

        _costumeBonus = default;

        _stats = _coreStats;

        // 최소값 보장 및 자원 초기화
        _currentHP = Mathf.Max(1, _stats.MaxHP);
        _currentMP = Mathf.Max(0, _stats.MaxMP);
        _currentDashCount = Mathf.Max(1, _stats.DashCount);

        _currentEXP = 0;
        _requiredEXP = _stats.RequiredEXP;
        _level = 1;

        _money = _baseData.StartMoney;

        _mpRegenProgress = 0f;
        _dashRegenProgress = 0f;

        IsInitialized = true;

        NotifyAll();
    }

    
    private void NotifyAll()
    {
        OnStatsChanged?.Invoke(_stats);

        OnHPChanged?.Invoke(_currentHP, _stats.MaxHP);
        OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);
        OnDashCountChanged?.Invoke(_currentDashCount, _stats.DashCount);

        OnEXPChanged?.Invoke(_currentEXP, _requiredEXP);
        OnMoneyChanged?.Invoke(_money);
        OnLevelChanged?.Invoke(_level);
    }

    // 스탯값 설정
    public void SetStats(SPlayerStats newStats)
    {
        if (!IsInitialized)
        {
            return;
        }

        newStats.Validate();
        _stats = newStats;
        RecalculateStats();

        // 최대치 변경 후 현재값 보정
        _currentHP = Mathf.Clamp(_currentHP, 0, _stats.MaxHP);
        _currentMP = Mathf.Clamp(_currentMP, 0, _stats.MaxMP);
        _currentDashCount = Mathf.Clamp(_currentDashCount, 0, _stats.DashCount);

        if (_level == 1)
        {
            _requiredEXP = _stats.RequiredEXP;
        }

        NotifyAll();
    }

    // 코스튬 효과 가져오기
    public void SetCostumeBonus(SCostumeStatBonus bonus)
    {
        if (!IsInitialized)
        {
            return;
        }

        _costumeBonus = bonus;

        RecalculateStats();
    }

    // 코스튬 효과 적용하기
    private void RecalculateStats()
    {
        if (!IsInitialized)
        {
            return;
        }

        SPlayerStats result = _coreStats;

        // 방어
        result.Def += _costumeBonus.def;

        // HP / MP
        result.MaxHP += _costumeBonus.MaxHP;
        result.MaxMP += _costumeBonus.MaxMP;

        // 공격 속성
        result.Attribute_Physical += _costumeBonus.Physical;
        result.Attribute_Fire += _costumeBonus.Fire;
        result.Attribute_Ice += _costumeBonus.Ice;
        result.Attribute_Lightning += _costumeBonus.Lightning;

        result.Validate();

        _stats = result;

        // 현재 자원 최대치 보정
        _currentHP = Mathf.Clamp(_currentHP, 0, _stats.MaxHP);
        _currentMP = Mathf.Clamp(_currentMP, 0, _stats.MaxMP);
        _currentDashCount = Mathf.Clamp(_currentDashCount, 0, _stats.DashCount);

        if (_level == 1)
        {
            _requiredEXP = _stats.RequiredEXP;
        }

        NotifyAll();
    }

    // 속성치 가져오기
    public int GetAttributePower(EAttribute attribute)
    {
        return _stats.GetAttributePower(attribute);
    }

    // 속성치 증가
    public void AddAttributePower(EAttribute attribute, int amount)
    {
        SPlayerStats newStats = _stats;

        newStats.AddAttributePower(attribute, amount);

        SetStats(newStats);
    }

    // 플레이어가 받는 데미지 처리
    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        _currentHP = Mathf.Max(0, _currentHP - damage);
        OnHPChanged?.Invoke(_currentHP, _stats.MaxHP);
    }

    // 체력 회복
    public void Heal(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentHP = (int)MathF.Min(_stats.MaxHP, _currentHP + amount);
        OnHPChanged?.Invoke(_currentHP, _stats.MaxHP);
    }

    // MP 회복
    public void RecoverMP(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentMP = Mathf.Min(_stats.MaxMP, _currentMP + amount);

        OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);
    }

    // MP 자연회복
    public void RecoverMPperTime(float deltaTime)
    {
        if (_currentMP >= _stats.MaxMP)
        {
            _mpRegenProgress = 0f;
            return;
        }

        _mpRegenProgress += _stats.MP_Regin * deltaTime;

        // FloorToInt()는 소수점 이하를 버리고 int로 변환
        int recovered = Mathf.FloorToInt(_mpRegenProgress);

        if (recovered <= 0)
        {
            return;
        }

        _mpRegenProgress -= recovered;
        RecoverMP(recovered);
    }

    // 마나 사용 시도
    public bool TryUseMP(int amount)
    {
        if (amount < 0 || _currentMP < amount)
        {
            return false;
        }

        _currentMP -= amount;
        OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);

        return true;
    }


    // 대시 사용 시도
    public bool TryConsumeDash()
    {
        if (_currentDashCount < 1)
        {
            return false;
        }

        _currentDashCount--;

        OnDashCountChanged?.Invoke(_currentDashCount, _stats.DashCount);

        return true;
    }

    // 자연 대시 게이지 회복
    public void ReChargeDashperTime(float deltaTime)
    {
        if (_currentDashCount >= _stats.DashCount)
        {
            _dashRegenProgress = 0f;
            return;
        }

        _dashRegenProgress += _stats.DashRegainSpeed * deltaTime;

        int recovered = Mathf.FloorToInt(_dashRegenProgress);

        if (recovered <= 0)
        {
            return;
        }

        _dashRegenProgress -= recovered;

        _currentDashCount = Mathf.Min(_stats.DashCount, _currentDashCount + recovered);
    }

    // 경험치 획득
    public void AddEXP(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _currentEXP += amount;

        while (_currentEXP >= _requiredEXP)
        {
            _currentEXP -= _requiredEXP;

            LevelUp();
        }

        OnEXPChanged?.Invoke(_currentEXP, _requiredEXP);
    }

    // 레벨업
    private void LevelUp()
    {
        _level++;

        float growth = _baseData.ExpGrowthMultiplier;

        // Mathf.CeilToInt()는 소수점 이하를 올림하여 int로 변환
        _requiredEXP = Mathf.Max(1, Mathf.CeilToInt(_requiredEXP * growth));

        Heal(Mathf.RoundToInt(_stats.Levelup_Heal));

        OnLevelChanged?.Invoke(_level);
    }

    // 돈 획득
    public void AddMoney(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        _money += amount;
        OnMoneyChanged?.Invoke(_money);
    }

    // 돈 사용 시도
    public bool TrySpendMoney(int amount)
    {
        if (amount < 0 || _money < amount)
        {
            return false;
        }

        _money -= amount;

        OnMoneyChanged?.Invoke(_money);

        return true;


    }
   
    // 현재자원(HP, MP) 최대로 초기화
    // 메인 게임 시작시 or 체력 혹은 마나 변경되는 코스튬 적용시에만 사용하게될 예정
    public void FillResourceToMax()
    {
        if (!IsInitialized)
        {
            return;
        }

        _currentHP = _stats.MaxHP;
        _currentMP = _stats.MaxMP;
        _currentDashCount = _stats.DashCount;

        _mpRegenProgress = 0f;
        _dashRegenProgress = 0f;

        OnHPChanged?.Invoke(_currentHP, _stats.MaxHP);
        OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);

        OnDashCountChanged?.Invoke(_currentDashCount, _stats.DashCount);
    }


}
