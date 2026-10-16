using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CMonterMoveTest : MonoBehaviour
{
    [SerializeField] private Transform _target;

    private NavMeshAgent _na;
    private SpriteRenderer _sp;
    private Animator _ani;



    void Start()
    {
        _na = GetComponent<NavMeshAgent>();
        // 네브메쉬 에이전트의 회전 끄기
        _na.updateRotation = false;
        _na.updateUpAxis = false;

        _sp = GetComponent<SpriteRenderer>();
        _ani = GetComponent<Animator>();
    }


    void Update()
    {
        _na.SetDestination(_target.position);
        _na.isStopped = false;
        if (_na.velocity.x > 0.01f)
        {
            _sp.flipX = false;
        }
        else if (_na.velocity.x < -0.01f)
        {
            _sp.flipX = true;
        }

        float vel = Mathf.Abs(_na.velocity.sqrMagnitude);
        if (vel > 0.01f)
        {
            _ani.SetBool("bMove", true);
        }
        else
        {
            _ani.SetBool("bMove", false);
        }
    }



}
