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
    [Header("공격 패턴 쿨타임 (턴)")]
    [SerializeField] private int _shoutCoolTimeMax = 7;
    [SerializeField] private int _jumpCoolTimeMax = 3;
    #endregion

    #region 내부 변수
    private bool _isInitSet = false;
    private IEnemyState _state;
    private IEnemyState[] _states = new IEnemyState[4];
    private int _shoutCoolTime = 0;
    private int _jumpCoolTime = 0;
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
        if (_shoutCoolTime <= 0 && _mashine.SuperArmor <= 0f)
        {
            _state = _shout;
            _shoutCoolTime = _shoutCoolTimeMax;
        }
        else if (_jumpCoolTime <= 0)
        {
            _state = _jumpAttack;
            _jumpCoolTime = _jumpCoolTimeMax;
        }
        else
        {
            int random = Random.Range(0, 2);
            _state = _states[random];
            _jumpCoolTime--;
        }

        if (_mashine.SuperArmor <= 0f)
        {
            _shoutCoolTime--;
        }

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
