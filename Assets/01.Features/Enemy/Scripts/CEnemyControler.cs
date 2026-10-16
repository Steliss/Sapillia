using System;
using System.Collections.Generic;
using UnityEngine;




public abstract class CEnemyStateSO : ScriptableObject, IEnemyState
{
    public abstract void StateEnter();
    public abstract void StateExit();
    public abstract void StateUpdate();
}


[CreateAssetMenu(menuName = "SO/Enemy/State_Idle", fileName = "State_Idle")]
public class CEnemyStateIdleSO : CEnemyStateSO
{
    [SerializeField] private string _name = "Idle";
    
    

    public override void StateEnter()
    {
        Debug.Log("Idle진입");
    }

    public override void StateExit()
    {
        Debug.Log("Idle나감");

    }

    public override void StateUpdate()
    {
        
    }
}

[CreateAssetMenu(menuName = "SO/Enemy/State_Move", fileName = "State_Move_")]
public class CEnemyStateMoveSO : CEnemyStateSO
{
    [SerializeField] private string _name = "Move";
    [Min(0.1f)]
    [SerializeField] private float _peed = 10f;



    public override void StateEnter()
    {
        Debug.Log("Move진입");
    }

    public override void StateExit()
    {
        Debug.Log("Move나감");

    }

    public override void StateUpdate()
    {

    }
}

[Serializable]
public class StateControler
{
    private CEnemyStateSO _state;

    public CEnemyStateSO State => _state;



    public void StateChanged(CEnemyStateSO after)
    {
        if (_state == after)
        {
            return;
        }

        if (_state != null)
        {
            _state.StateExit();
        }

        _state = after;
        _state.StateEnter();
    }

    public void StateUpdate()
    {
        if (_state != null)
        {
            _state.StateUpdate();
        }
    }
}

public class CEnemyControler : MonoBehaviour
{
    [SerializeField] private List<CEnemyStateSO> _states = new List<CEnemyStateSO>();

    [SerializeField] private StateControler _stateControler = new StateControler();


    void Start()
    {
        
    }


    void Update()
    {
        
    }

    public void StateChange(int index)
    {
        _stateControler.StateChanged(_states[index]);
    }
}
