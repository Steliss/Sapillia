using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/AttackState/MultySO", fileName = "Enemy_Attack_MultySO_")]
public class CEnemyAttackStateSO_MultySO : CEnemyAttackStateSO
{
    #region 인스펙터
    [SerializeField] private CEnemyAttackStateSO _meleeAttack;
    [SerializeField] private CEnemyAttackStateSO _rushAttack;
    [SerializeField] private CEnemyAttackStateSO _jumpAttack;
    [SerializeField] private CEnemyAttackStateSO _shout;
    #endregion

    #region 내부 변수
    private IEnemyState _state;

    #endregion

    public override void StateEnter()
    {
        // 조건 달고 상태 넣은 후 스테이트엔터 시작
        _state = _meleeAttack;
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
