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

	public void Initalize()
	{
		
	}
}
