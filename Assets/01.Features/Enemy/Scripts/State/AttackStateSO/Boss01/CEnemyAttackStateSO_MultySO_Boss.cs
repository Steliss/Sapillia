using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/AttackState/MultySO/Boss01", fileName = "Enemy_Attack_MultySO_Boss01")]
public class CEnemyAttackStateSO_MultySO_Boss : CEnemyAttackStateSO
{
    #region 내부 구조체
    [Serializable]
    public struct SAttackData
    {
        [SerializeField] public CEnemyAttackStateSO AttackSO;
        [SerializeField] public int StartCoolTime;
        [SerializeField] public int MaxCoolTime;
    }
    #endregion


    #region 인스펙터
    [Header("쿨타임 돌면 우선 시전하는 패턴 (이 패턴중엔 쿨감x)")]
    [SerializeField] private SAttackData[] _firstAttacks;
    [Header("쿨타임 중엔 제외되는 랜덤 패턴 (이 패턴하면 쿨감)")]
    [SerializeField] private SAttackData[] _randomAttacks;
    #endregion

    #region 내부 변수
    private bool _isInitSet = false;
    private IEnemyState _state;
    #endregion

    private void InitSet(bool isSet)
    {
        if (isSet)
        {
            return;
        }

        for (int i = 0; i < _firstAttacks.Length; i++)
        {
            _firstAttacks[i].AttackSO.Init(_mashine);
        }

        for (int i = 0; i < _randomAttacks.Length; i++)
        {
            _randomAttacks[i].AttackSO.Init(_mashine);
        }
        _isInitSet = true;


    }
    public override void StateEnter()
    {
        InitSet(_isInitSet);
        // 조건 달고 상태 넣은 후 스테이트엔터 시작
        _state = PatternSelecter();
        _state.StateEnter();
    }

    public override void StateExit()
    {
        _state?.StateExit();
    }

    public override void StateUpdate()
    {
        _state?.StateUpdate();
    }

    private IEnemyState PatternSelecter()
    {
        IEnemyState state = null;

        // 우선공격 쿨타임이 돌았으면 우선공격 리턴
        for (int i = 0; i < _firstAttacks.Length; i++)
        {
            if (_firstAttacks[i].StartCoolTime <= 0)
            {
                state = _firstAttacks[i].AttackSO;
                _firstAttacks[i].StartCoolTime = _firstAttacks[i].MaxCoolTime;
                return state;
            }
        }

        // 우선공격 안할때만 쿨타임 감소
        for (int i = 0; i < _firstAttacks.Length; i++)
        {
            if (_firstAttacks[i].StartCoolTime > 0)
            {
                _firstAttacks[i].StartCoolTime--;
            }
        }

        // 랜덤 패턴 뽑기
        List<int> randomList = new List<int>();
        for (int i = 0; i < _randomAttacks.Length; i++)
        {
            if (_randomAttacks[i].StartCoolTime <= 0)
            {
                randomList.Add(i);
            }
            _randomAttacks[i].StartCoolTime--; // 쿨타임감소
        }
        int random = UnityEngine.Random.Range(0, randomList.Count);
        int index = randomList[random];

        // 뽑힌 랜덤패턴 쿨타임 적용
        _randomAttacks[index].StartCoolTime = _randomAttacks[index].MaxCoolTime;

        return _randomAttacks[index].AttackSO;
    }

}
