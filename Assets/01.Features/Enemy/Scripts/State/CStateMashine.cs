using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CStateMashine : MonoBehaviour
{
    #region 인스펙터    
    [SerializeField] private string _id;
    [SerializeField] private CDataBaseEnemySO _dataBase = null;
    [SerializeField] private List<CEnemyStateSO_Attack> _attackState;
    #endregion

    #region 내부 변수
    private Animator _ani;
    private NavMeshAgent _nav;
    private SpriteRenderer _sprite;
    private bool _isPatrol = true;
    #endregion

    #region 스테이트
    private CEnemyState_Idle _idleState;
    private CEnemyState_Move _moveState;
    private IEnemyState _state;
    #endregion

    #region 스탯
    private float _hp;
    private float _superArmor;
    private float _defense;
    private float _attack;
    private float _speed;
    #endregion

    #region 프로퍼티
    public string Id => _id;
    public Animator Ani => _ani;
    public NavMeshAgent Nav => _nav;
    public SpriteRenderer Sprite => _sprite;
    public bool IsPatrol => _isPatrol;
    public CEnemyState_Idle IdleState => _idleState;
    public CEnemyState_Move MoveState => _moveState;
    public IEnemyState State => _state;
    public float Hp => _hp;
    public float SuperArmor => _superArmor;
    public float Defense => _defense;
    public float Attack => _attack;
    public float Speed => _speed;
    #endregion



    public void StateChanged(IEnemyState next)
    {
        if (_state == next)
        {
            return;
        }

        if (_state != null)
        {
            State.StateExit();
        }

        _state = next;

        _state.StateEnter();
    }



    private void Awake()
    {
        // id가 비어있을 경우 오브젝트 이름으로 id 설정
        if (string.IsNullOrWhiteSpace(_id))
        {
            _id = gameObject.name;
        }

        // 기본 상태 생성
        _idleState = new CEnemyState_Idle(this);
        _moveState = new CEnemyState_Move(this);

        // 데이터베이스에서 스탯 가져오기
        if (_dataBase == null)
        {
            CLog.Warn($"{name} : 참조할 데이터 베이스 누락");
            return;
        }

        _dataBase.TryGet(_id, out CDataEnemySO data);

        if (data != null)
        {
            _hp = data.Hp;
            _superArmor = data.SuperArmor;
            _defense = data.Defense;
            _attack = data.Attack;
            _speed = data.Speed;
        }

        // 애니메이터, 네브메쉬에이전트, 스프라이트 가져오기
        _ani = GetComponent<Animator>();

        if (Ani == null)
        {
            CLog.Warn($"{name} : 애니메이터 누락");
            return;
        }


        _nav = GetComponent<NavMeshAgent>();

        if (Nav == null)
        {
            CLog.Warn($"{name} : 네브메쉬 에이전트 컴포넌트 누락");
            return;
        }
        Nav.updateRotation = false;
        Nav.updateUpAxis = false;

        Nav.speed = _speed;


        _sprite = GetComponent<SpriteRenderer>();

        if (Sprite == null)
        {
            CLog.Warn($"{name} : 스프라이트 컴포넌트 누락");
        }

        // 공격 패턴 생성
        if (_attackState.Count > 0)
        {
            for (int i = 0; i < _attackState.Count; i++)
            {
                _attackState[i] = new CEnemyStateSO_Attack(this);
            }
        }

        // idle 상태로 시작
        StateChanged(IdleState);
    }

    private void Update()
    {
        if (State != null)
        {
            State.StateUpdate();
        }
    }


}
