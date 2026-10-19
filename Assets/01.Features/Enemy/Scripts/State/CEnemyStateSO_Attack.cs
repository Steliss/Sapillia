using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/State_Attack", fileName = "Enemy_Attack_")]
public class CEnemyStateSO_Attack : ScriptableObject, IEnemyState
{
    private CStateMashine _mashine;
    private float _delay = 2f;

    public CEnemyStateSO_Attack(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        
    }

    public void StateExit()
    {
        
    }

    public void StateUpdate()
    {
        
    }

}
