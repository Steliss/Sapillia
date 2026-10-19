using UnityEngine;

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
        _mashine.Nav.isStopped = false;
        _mashine.Ani.SetBool("bMove", true);

        if (_mashine.IsPatrol)
        {
            Vector3 targetPos = _mashine.transform.position;
            float z = Random.Range(0f, 360f);
            Quaternion angle = Quaternion.Euler(0f, 0f, z);

            _target = targetPos + (angle * new Vector3(_patrolRange, 0f, 0f));
            _mashine.Nav.SetDestination(_target);
        }
    }

    public void StateExit()
    {
        _mashine.Nav.isStopped = true;
        _mashine.Ani.SetBool("bMove", false);
    }

    public void StateUpdate()
    {
        if (_mashine.Nav.velocity.x > 0)
        {
            _mashine.Sprite.flipX = false;
        }
        else if (_mashine.Nav.velocity.x < 0)
        {
            _mashine.Sprite.flipX = true;
        }


        _moveTime += Time.deltaTime;
        if (_moveTime > 2f)
        {
            _moveTime = 0f;
            _mashine.StateChanged(_mashine.IdleState);
        }

    }
}
