using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class CEnemyAttackStateSO : ScriptableObject, IEnemyState
{
    protected CStateMashine _mashine;
    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public abstract void StateEnter();
    public abstract void StateExit();
    public abstract void StateUpdate();
    protected GameObject CreateAttackObject(GameObject prefeb, float lifeTime, Vector2 pos, Quaternion angle)
    {
        GameObject go = null;

        if (prefeb != null && CObjectPoolManager.Instance != null)
        {
            go = CObjectPoolManager.Instance.GetPoolObject(prefeb, lifeTime);
            go.transform.rotation = angle;
            go.transform.position = pos;
            _mashine.GameObjectsListAdd(go);
        }

        return go;
    }

    protected float DirToAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
    }

    protected Vector2 AngleToDir(float angle)
    {
        return Quaternion.Euler(0, 0, angle) * Vector2.up;
    }
}
