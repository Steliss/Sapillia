using System;
using UnityEngine;

public class CPlayerData : MonoBehaviour
{
	#region 인스펙터
	[Header("Base Data")]
	[SerializeField] private CPlayerDataSO _baseData;

	#endregion

	#region 내부변수
	private SPlayerStats _stats;

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
	public int CurrentHP => _currentHP;
	public int CurrentMP => _currentMP;
	public float CurrentDashCount => _currentDashCount;
    public int CurrentEXP => _currentEXP;
    public int RequiredEXP => _requiredEXP;
    public int Level => _level;
    public int Money => _money;

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


    // 데이터 연동
    public void Initalize()
	{
		if(_baseData == null)
		{
			CLog.Warn("Player Data 누락");
			enabled = false;
			return;
		}

		// 기본 데이터 현재 데이터로 복사
		_stats = _baseData.BaseStats;
		_stats.Validate();

		// 최소값 보정 및 자원 초기화
		_currentHP = Mathf.Max(1, _stats.MaxHP);
		_currentMP = Mathf.Max(0, _stats.MaxMP);
		_currentDashCount = Mathf.Max(1, _stats.DashCount);

        _currentEXP = 0;
        _requiredEXP = _stats.RequiredEXP;
        _level = 1;
        _money = _baseData.StartMoney;

        _mpRegenProgress = 0f;
        _dashRegenProgress = 0f;

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
	
	// 스텟갑 변경
	public void SetStats(SPlayerStats newStats)
	{
		newStats.Validate();

		_stats = newStats;

        // 최대치 감소 시 현재값 보정
        _currentHP = Mathf.Clamp(_currentHP, 0, _stats.MaxHP);
		_currentMP = Mathf.Clamp(_currentMP, 0, _stats.MaxMP);
		_currentDashCount = Mathf.Clamp(_currentDashCount, 0, _stats.DashCount);
		
		if(_level == 1)
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

	// 속성치 증감
	public void AddAttributePower(EAttribute attribute, int amount)
	{
		SPlayerStats newStats = _stats;

		newStats.AddAttributePower(attribute, amount);

		SetStats(newStats);
	}

    // 플레이어 받는 데미지 계산
    public void TakeDamage(int damage)
	{
		if(damage <= 0)
		{
			return;
		}

		_currentHP = Mathf.Max(0,_currentHP - damage);
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



	public void RecoverMP(int amount)
	{
		if(amount <= 0)
		{
			return;
		}

		_currentMP = Mathf.Min(_stats.MaxMP, _currentMP + amount);

		OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);
	}

	private void RecoverMPperTime(float deltaTime)
	{
		if (_currentMP >= _stats.MaxMP)
		{
			_mpRegenProgress = 0f;
			return;
		}

		_mpRegenProgress += _stats.MP_Regin * deltaTime;
		
		// FloorToInt() → 소숫점 첫번째자리 올림 → int 반환
		int recovered = Mathf.FloorToInt(_mpRegenProgress);

		if(recovered <= 0)
		{
			return;
		}

		_mpRegenProgress -= recovered;
		RecoverMP(recovered);
	}

	// 마나 사용 시도
	public bool TryUseMP(int amount)
	{
		if(amount < 0 || _currentMP < amount)
		{
			return false;
		}

		_currentMP -= amount;
		OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);

		return true;
	}


	// 대쉬 사용 시도
	public bool TryConsumeDash()
	{
		if(_currentDashCount < 1)
		{
			return false;
		}

		_currentDashCount--;

		OnDashCountChanged?.Invoke(_currentDashCount, _stats.DashCount);

		return true;
	}

	// 자연 대쉬 게이지 회복
	private void ReChargeDashperTime(float deltaTime)
	{
		if(_currentDashCount >= _stats.DashCount)
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
		if(amount <= 0)
		{
			return;
		}

		_currentEXP += amount;

		while(_currentEXP >= _requiredEXP)
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

        //  Mathf.CeilToInt(_requiredEXP * growth) → _requiredEXP * growth 값을 첫번째 자리 올림 → int형 반환
        _requiredEXP = Mathf.Max(1, Mathf.CeilToInt(_requiredEXP * growth));

		Heal(Mathf.RoundToInt(_stats.Levelup_Heal));

		OnLevelChanged?.Invoke(_level);
	}

	// 돈 획득
	public void AddMoney(int amount)
	{
		if(amount <= 0)
		{
			return;						
		}

		_money += amount;
		OnMoneyChanged?.Invoke(_money);
	}

	// 돈 사용 시도
	public bool TrySpendMoney(int amount)
	{
		if(amount < 0 || _money < amount)
		{
			return false;
		}

		_money -= amount;

		OnMoneyChanged?.Invoke(_money);

		return true;
	}
    private void Awake()
    {
        Initalize();
    }

    private void Update()
    {
		RecoverMPperTime(Time.deltaTime);
		ReChargeDashperTime(Time.deltaTime);
    }


	
}
