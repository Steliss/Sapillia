using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class CStateMashine : MonoBehaviour
{
    #region 인스펙터    
    [SerializeField] private string _id;
    [SerializeField] private CDataBaseEnemySO _dataBase = null;
    [SerializeField] private float _patrolRadius = 3f;
    [SerializeField] private float _patrolMoveRange = 3f;
    [SerializeField] private LayerMask _playerMask;

    [Header("공격 패턴 SO")]
    [SerializeField] private List<CEnemyStateSO_Attack> _attackState;

    [Header("애니메이터 스테이트")]
    [SerializeField] private string _paramIdle = "Idle";
    [SerializeField] private string _paramMove = "Move";
    [SerializeField] private string _paramAttack = "Attack";
    [SerializeField] private string _paramAttReady = "AttReady";
    [SerializeField] private string _paramAir = "Air";
    [SerializeField] private string _paramDown = "Down";
    #endregion

    #region 내부 변수
    private Animator _ani;
    private NavMeshAgent _nav;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private bool _isPatrol = true;
    private CUniTaskOnes _uni = new CUniTaskOnes();
    private Transform _target;
    #endregion

    #region 스테이트
    private IEnemyState _idleState;
    private IEnemyState _moveState;
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
    public Rigidbody2D Rigidbody => _rb;
    public bool IsPatrol => _isPatrol;
    public IEnemyState IdleState => _idleState;
    public IEnemyState MoveState => _moveState;
    public IReadOnlyList<CEnemyStateSO_Attack> AttackState => _attackState;
    public IEnemyState State => _state;
    public CUniTaskOnes Uni => _uni;
    public Transform Target => _target;
    public float PatrolMoveRange => _patrolMoveRange;

    // 스탯
    public float Hp => _hp;
    public float SuperArmor => _superArmor;
    public float Defense => _defense;
    public float Attack => _attack;
    public float Speed => _speed;

    // 애니메이터 해쉬
    public int HashMove { get; private set; }
    public int HashAttack { get; private set; }
    public int HashAttReady { get; private set; }
    public int HashAir { get; private set; }
    public int HashIdle { get; private set; }
    public int HashDown { get; private set; }
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

        CLog.Log($"상태변경 {_state} -> {next}");

        _state = next;

        next.StateEnter();
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
        AnimatorSet();

        _nav = GetComponent<NavMeshAgent>();

        if (Nav == null)
        {
            CLog.Warn($"{name} : 네브메쉬 에이전트 컴포넌트 누락");
            gameObject.AddComponent<NavMeshAgent>();
            _nav = GetComponent<NavMeshAgent>();
        }
        Nav.updateRotation = false;
        Nav.updateUpAxis = false;
        Nav.speed = _speed;


        _sprite = GetComponent<SpriteRenderer>();
        if (Sprite == null)
        {
            CLog.Warn($"{name} : 스프라이트 컴포넌트 누락");
            gameObject.AddComponent<SpriteRenderer>();
            _sprite = GetComponent<SpriteRenderer>();
        }

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
        {
            CLog.Warn($"{name} : 리지드바디2D 컴포넌트 누락");
            gameObject.AddComponent<Rigidbody2D>();
            _rb = GetComponent<Rigidbody2D>();
        }


        // 공격 패턴 생성
        if (_attackState.Count > 0)
        {
            for (int i = 0; i < _attackState.Count; i++)
            {
                CEnemyStateSO_Attack instance = Instantiate(_attackState[i]);
                instance.Init(this);
                _attackState[i] = instance;
                
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

    // 애니메이터 해시 세팅
    private void AnimatorSet()
    {
        _ani = GetComponent<Animator>();

        if (Ani == null)
        {
            CLog.Warn($"{name} : 애니메이터 누락");
            return;
        }

        HashMove = Animator.StringToHash(_paramMove);
        HashAttack = Animator.StringToHash(_paramAttack);
        HashAir = Animator.StringToHash(_paramAir);
        HashIdle = Animator.StringToHash(_paramIdle);
        HashAttReady = Animator.StringToHash(_paramAttReady);
        HashDown = Animator.StringToHash(_paramDown);
    }

    public void PatrolSearch()
    {
        if (!_isPatrol)
        {
            return;
        }


        Collider2D hit = Physics2D.OverlapCircle(transform.position, _patrolRadius, _playerMask);

        if (hit != null)
        {
            CLog.Log("플레이어 발견");
            _target = hit.transform;
            _isPatrol = false;
            StateChanged(_moveState);
        }
    }

}
