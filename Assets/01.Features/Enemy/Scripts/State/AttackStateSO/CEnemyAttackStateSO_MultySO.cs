using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/AttackState/MultySO", fileName = "Enemy_Attack_MultySO_")]
public class CEnemyAttackStateSO_MultySO : CEnemyAttackStateSO
{
    #region 인스펙터
    [Header("공격 패턴")]
    [SerializeField] private CEnemyAttackStateSO _meleeAttack;
    [SerializeField] private CEnemyAttackStateSO _rushAttack;
    [SerializeField] private CEnemyAttackStateSO _jumpAttack;
    [SerializeField] private CEnemyAttackStateSO _shout;
    #endregion

    #region 내부 변수
    private bool _isInitSet = false;
    private IEnemyState _state;
    private IEnemyState[] _states = new IEnemyState[4];
    #endregion

    private void InitSet(bool isSet)
    {
        if (isSet)
        {
            return;
        }

        _isInitSet = true;
        if (_meleeAttack != null)
        {
            CEnemyAttackStateSO so = Instantiate(_meleeAttack);
            so.Init(_mashine);
            _meleeAttack = so;
        }
        if (_rushAttack != null)
        {
            CEnemyAttackStateSO so = Instantiate(_rushAttack);
            so.Init(_mashine);
            _rushAttack = so;
        }
        if (_jumpAttack != null)
        {
            CEnemyAttackStateSO so = Instantiate(_jumpAttack);
            so.Init(_mashine);
            _jumpAttack = so;
        }
        if (_shout != null)
        {
            CEnemyAttackStateSO so = Instantiate(_shout);
            so.Init(_mashine);
            _shout = so;
        }
        _states = new IEnemyState[4] 
        { _meleeAttack, _rushAttack, _jumpAttack, _shout };
    }
    public override void StateEnter()
    {
        InitSet(_isInitSet);
        // 조건 달고 상태 넣은 후 스테이트엔터 시작
        int random = Random.Range(0, _states.Length);
        _state = _states[random];
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
}
