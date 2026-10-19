using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CStateMashine
{


    #region 스테이트
    public CEnemyState_Idle IdleState;
    public CEnemyState_Move MoveState;
    public IEnemyState State;
    #endregion

    public CEnemyState Enemy { get; private set; }

    #region 스텟
    public float _hp { get; private set; }
    public float _superArmor { get; private set; }
    public float _defense { get; private set; }
    public float _attack { get; private set; }
    public float _speed { get; private set; }
    #endregion


    public CStateMashine(CEnemyState enemy, CDataEnemySO data)
    {
        Enemy = enemy;
        IdleState = new CEnemyState_Idle(this);
        MoveState = new CEnemyState_Move(this);


        if (data != null)
        {
            _hp = data.Hp;
            _superArmor = data.SuperArmor;
            _defense = data.Defense;
            _attack = data.Attack;
            _speed = data.Speed;
        }

        StateChanged(IdleState);
    }

    public void StateChanged(IEnemyState next)
    {
        if (State == next)
        {
            return;
        }

        if (State != null)
        {
            State.StateExit();
        }

        State = next;

        State.StateEnter();
    }

    public void StateUpdate()
    {
        if (State != null)
        {
            State.StateUpdate();
        }
    }

}
