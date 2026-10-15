using System;
using System.Collections;
using System.Collections.Generic;
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
	private float _currentDashCount;

	#endregion

	#region 프로퍼티
	public SPlayerStats Stats => _stats;
	public int CurrentHP => _currentHP;
	public int CurrentMP => _currentMP;
	public float CurrentDashCount => _currentDashCount;

	#endregion

	#region 이벤트
	public event Action<int, int> OnHPChanged;
	public event Action<int, int> OnMPChanged;
	public event Action<float,  float> OnDashCountChanged;

    #endregion

    private void Awake()
    {
        Initalize();
    }

	// 데이터 연동
	public void Initalize()
	{
		if(_baseData == null)
		{
			CLog.Warn("Player Data 누락");
			return;
		}

		// 기본 데이터 현재 데이터로 복사
		_stats = _baseData.BaseStats;

		// 최소값 보정 및 자원 초기화
		_currentHP = Mathf.Max(1, _stats.MaxHP);
		_currentMP = Mathf.Max(0, _stats.MaxMP);
		_currentDashCount = Mathf.Max(1f, _stats.DashCount);

		OnHPChanged?.Invoke(_currentHP, _stats.MaxHP);
		OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);
		OnDashCountChanged?.Invoke(_currentDashCount, _stats.DashCount);
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
		OnHPChanged(_currentHP, _stats.MaxHP);
	}

	// 레벨업 체력 회복
	public void LevelUpHeal()
	{
		if(_currentHP <= 0)
		{
			return;
		}

		_currentHP = (int)MathF.Min(_stats.MaxHP, _currentHP + (_stats.MaxHP * 0.1f));
		OnHPChanged?.Invoke(_currentHP, _stats.MaxHP);
	}

	// 마나 사용 시도
	public bool TryUseMana(int amount)
	{
		if(amount < 0 || _currentMP < amount)
		{
			return false;
		}

		_currentMP -= amount;
		OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);

		return true;
	}

	// 마나 자연 회복
	public void RegainMana()
	{
        /*
		// 비전투 상황일 경우 빠르게 회복
		if (!_isBattle)
		{

			_currentMP = (int)Mathf.Min(_stats.MaxMP, _currentMP + (_stats.MaxMP / 10f));
			OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);
			return;
		}
		*/


        // 전투 상황일 경우 초당 회복
        _currentMP = (int)Mathf.Min(_stats.MaxMP, _currentMP + (_stats.MP_Regin / 10f));
		OnMPChanged?.Invoke(_currentMP, _stats.MaxMP);
	}

	// 이동 속도
	public void AddMoveSpeed(float amount)
	{
		_stats.Movement_Speed = Mathf.Max(1f, _stats.Movement_Speed + amount);
		
	}
}
