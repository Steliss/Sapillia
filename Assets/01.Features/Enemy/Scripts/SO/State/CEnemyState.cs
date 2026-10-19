using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CEnemyState : MonoBehaviour
{
    
    [SerializeField] public string _id { get; private set; } = "Boom_Mole";
    [SerializeField] private CDataBaseEnemySO _dataBase = null;

    private CStateMashine _stateMashine;
    public Animator Ani { get; private set; }
    public NavMeshAgent Nav { get; private set; }
    public SpriteRenderer Sprite { get; private set; }
    public bool IsPatrol = true;

    private void Awake()
    {
        if (_dataBase == null)
        {
            CLog.Warn($"{name} : 참조할 데이터 베이스 누락");
            return;
        }

        if (_dataBase.TryGet(_id, out CDataEnemySO data))
        {
            _stateMashine = new CStateMashine(this, data);
        }


        Ani = GetComponent<Animator>();

        if (Ani == null)
        {
            CLog.Warn($"{name} : 애니메이터 누락");
            return;
        }


        Nav = GetComponent<NavMeshAgent>();

        if (Nav == null)
        {
            CLog.Warn($"{name} : 네브메쉬 에이전트 컴포넌트 누락");
            return;
        }
        Nav.updateRotation = false;
        Nav.updateUpAxis = false;

        Nav.speed = data.Speed;


        Sprite = GetComponent<SpriteRenderer>();

        if (Sprite == null)
        {
            CLog.Warn($"{name} : 스프라이트 컴포넌트 누락");
        }

        _stateMashine.StateChanged(_stateMashine.IdleState);
    }

    private void Update()
    {
        _stateMashine.StateUpdate();
    }

}
    

    