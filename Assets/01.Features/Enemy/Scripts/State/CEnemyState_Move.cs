using UnityEngine;

public class CEnemyState_Move : IEnemyState
{
    private CStateMashine _mashine;
    private Vector3 _target;
    private float _patrolRange = 3f;

    public CEnemyState_Move(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        CLog.Log("이동 시작");
        _mashine.Nav.isStopped = false;
        _mashine.Ani.Play(_mashine.HashMove);

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
    }

    public void StateUpdate()
    {
        // 왼쪽 오른쪽 전환
        if (_mashine.Nav.velocity.x > 0)
        {
            _mashine.Sprite.flipX = false;
        }
        else if (_mashine.Nav.velocity.x < 0)
        {
            _mashine.Sprite.flipX = true;
        }


        // Idle 전환
        if (_mashine.Nav.remainingDistance < _mashine.Nav.stoppingDistance + 0.1f)
        {
            if (_mashine.AttackState.Count >= 1)
            {
                _mashine.StateChanged(_mashine.AttackState[0]);
            }
            else
            {
                _mashine.StateChanged(_mashine.IdleState);
            }

        }
        
    }


}
