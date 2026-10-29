using UnityEngine;

public class CEnemyState_Move : IEnemyState
{
    private CStateMashine _mashine;
    private Vector3 _ranPos;
    private float _patrolRange => _mashine.PatrolMoveRange;

    public CEnemyState_Move(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        _mashine.Nav.isStopped = false;
        _mashine.Ani.Play(_mashine.HashMove);

        // 순찰 상태면 _patrolRange거리를 랜덤방향 이동
        if (_mashine.IsPatrol)
        {
            CLog.Log("랜덤이동 시작");
            Vector3 targetPos = _mashine.transform.position;
            float z = Random.Range(0f, 360f);
            Quaternion angle = Quaternion.Euler(0f, 0f, z);

            _ranPos = targetPos + (angle * new Vector3(_patrolRange, 0f, 0f));
            _mashine.Nav.SetDestination(_ranPos);
        }
        else
        {
            _mashine.Nav.SetDestination(_mashine.Target.position);
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
            _mashine.SpriteFlipX(false);
        }
        else if (_mashine.Nav.velocity.x < 0)
        {
            _mashine.SpriteFlipX(true);
        }

        // 거리 계산중이면 리턴
        if (_mashine.Nav.pathPending)
        {
            return;
        }


        if (!_mashine.IsPatrol)
        {
            _mashine.Nav.SetDestination(_mashine.Target.position);
        }

        // 상태 전환
        if (_mashine.Nav.remainingDistance < _mashine.Nav.stoppingDistance + 0.1f)
        {
            if (_mashine.IsPatrol)
            {
                _mashine.StateChanged(_mashine.IdleState);
            }
            else
            {
                _mashine.StateChanged(_mashine.AttackState[0]);
            }
        }


    }


}
