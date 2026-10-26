using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CEnemySpawnStateSO : ScriptableObject, IEnemyState
{
    protected CStateMashine _mashine;
    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public abstract void StateEnter();
    public abstract void StateExit();
    public abstract void StateUpdate();
}
