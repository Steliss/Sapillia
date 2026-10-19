using FMOD;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CEnemyState_Move : IEnemyState
{
    private CStateMashine _mashine;
    private Vector3 _target;
    private float _patrolRange = 3f;
    private float _moveTime = 0f;

    public CEnemyState_Move(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        _mashine.Enemy.Nav.isStopped = false;
        _mashine.Enemy.Ani.SetBool("bMove", true);

        if (_mashine.Enemy.IsPatrol)
        {
            Vector3 targetPos = _mashine.Enemy.transform.position;
            float z = Random.Range(0f, 360f);
            Quaternion angle = Quaternion.Euler(0f, 0f, z);

            _target = targetPos + (angle * new Vector3(_patrolRange, 0f, 0f));
            _mashine.Enemy.Nav.SetDestination(_target);
        }
    }

    public void StateExit()
    {
        _mashine.Enemy.Nav.isStopped = true;
        _mashine.Enemy.Ani.SetBool("bMove", false);
    }

    public void StateUpdate()
    {
        if (_mashine.Enemy.Nav.velocity.x > 0)
        {
            _mashine.Enemy.Sprite.flipX = false;
        }
        else if (_mashine.Enemy.Nav.velocity.x < 0)
        {
            _mashine.Enemy.Sprite.flipX = true;
        }


        _moveTime += Time.deltaTime;
        if (_moveTime > 2f)
        {
            _moveTime = 0f;
            _mashine.StateChanged(_mashine.IdleState);
        }

    }
}
