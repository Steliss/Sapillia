using Cysharp.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class CStateMashine : MonoBehaviour, IDamageInfoReceiver
{
    #region 인스펙터
    [Header("몬스터 기본 정보")]
    [SerializeField] private string _id;
    [SerializeField] private CDataBaseEnemySO _dataBase = null;
    [SerializeField] private float _patrolRadius = 3f;
    [SerializeField] private float _patrolMoveRange = 3f;
    [SerializeField] private LayerMask _playerMask;

    [Header("공격 패턴 SO")]
    [SerializeField] private List<CEnemyStateSO_Attack> _attackState;

    [Header("체력 바")]
    [SerializeField] private Slider _hpBar;
    [SerializeField] private Slider _superArmorBar;


    [Header("애니메이터 스테이트")]
    [SerializeField] private readonly string _stateIdle = "Idle";
    [SerializeField] private readonly string _stateMove = "Move";
    [SerializeField] private readonly string _stateAttack = "Attack";
    [SerializeField] private readonly string _stateAttReady = "AttReady";
    [SerializeField] private readonly string _stateAir = "Air";
    [SerializeField] private readonly string _stateDown = "Down";
    #endregion

    #region 내부 변수
    private Animator _ani;
    private NavMeshAgent _nav;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private bool _isPatrol = true;
    private CUniTaskOnes _attackUni = new CUniTaskOnes();
    private CUniTaskOnes _searchUni = new CUniTaskOnes();
    private CUniTaskOnes _hurtUni = new CUniTaskOnes();
    private Transform _target;
    private Vector3 _startScale;
    private Color _startColor;
    private readonly int _outLineId = Shader.PropertyToID("_OutlineOn");
    #endregion

    #region 스테이트
    private IEnemyState _idleState;
    private IEnemyState _moveState;
    private IEnemyState _airbornState;
    private IEnemyState _deathState;
    private IEnemyState _state;
    #endregion

    #region 스탯
    private CDataEnemySO _data;
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
    public CUniTaskOnes AttackUni => _attackUni;
    public Transform Target => _target;
    public float PatrolMoveRange => _patrolMoveRange;

    // 상태
    public IEnemyState State => _state;
    public IEnemyState IdleState => _idleState;
    public IEnemyState MoveState => _moveState;
    public IEnemyState AirbornState => _airbornState;
    public IEnemyState DeathState => _deathState;
    public IReadOnlyList<CEnemyStateSO_Attack> AttackState => _attackState;

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
        _airbornState = new CEnemyState_Airborn(this);
        _deathState = new CEnemyState_Death(this);

        // 애니메이터 해시 세팅 
        AnimatorSet();

        // 네브메쉬에이전트, 리지드바디 가져오기
        _nav = GetComponent<NavMeshAgent>();

        if (Nav == null)
        {
            CLog.Warn($"{name} : 네브메쉬 에이전트 컴포넌트 누락");
            gameObject.AddComponent<NavMeshAgent>();
            _nav = GetComponent<NavMeshAgent>();
        }
        Nav.updateRotation = false;
        Nav.updateUpAxis = false;


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


        _startScale = transform.localScale;
        _startColor = _sprite.color;


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

    private void OnEnable()
    {
        // 데이터 베이스에서 데이터 갱신
        EnemyInit();
        _searchUni.Run(async ct =>
        {
            while (_isPatrol)
            {
                PatrolSearch();
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
        });
    }


    private void Update()
    {
        if (State != null)
        {
            State.StateUpdate();
        }
    }

    // 스프라이트, 애니메이터 캐싱 및 해시 세팅
    private void AnimatorSet()
    {
        _ani = GetComponentInChildren<Animator>();
        _sprite = GetComponentInChildren<SpriteRenderer>();

        if (Ani == null)
        {
            CLog.Warn($"{name} : 애니메이터 누락");
            return;
        }

        HashMove = Animator.StringToHash(_stateMove);
        HashAttack = Animator.StringToHash(_stateAttack);
        HashAir = Animator.StringToHash(_stateAir);
        HashIdle = Animator.StringToHash(_stateIdle);
        HashAttReady = Animator.StringToHash(_stateAttReady);
        HashDown = Animator.StringToHash(_stateDown);
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
            _attackUni.Cancel();
            _target = hit.transform;
            _isPatrol = false;
            StateChanged(_moveState);
        }
    }

    // 데이터베이스에서 스탯 갱신
    private void EnemyInit()
    {
        if (_dataBase == null)
        {
            CLog.Warn($"{name} : 참조할 데이터 베이스 누락");
            return;
        }
        _isPatrol = true;
        _sprite.color = _startColor;
        _dataBase.TryGet(_id, out _data);

        if (_data != null)
        {
            _hp = _data.Hp;
            _superArmor = _data.SuperArmor;
            _defense = _data.Defense;
            _attack = _data.Attack;
            _speed = _data.Speed;

            Nav.speed = _speed;
        }

        // 슈퍼아머 테두리 On / Off
        bool superOn = _superArmor > 0f;

        _sprite.material.SetFloat(_outLineId, superOn ? 1f : 0f);
    }

    // 피격 피드백
    public void HultFeedback()
    {
        _hurtUni.Cancel();
        _hurtUni.Run(async ct =>
        {
            float t = 0f;
            float tMax = 0.1f;
            try
            {
                while (t < tMax)
                {
                    _sprite.color = Color.red;
                    t += Time.deltaTime;
                    float t1510 = Mathf.Lerp(1.5f, 1f, t/tMax);

                    Vector3 scale = _startScale;
                    scale.y = t1510;
                    _sprite.transform.localScale = scale;

                    await UniTask.Yield(PlayerLoopTiming.Update, ct);
                }
            }
            finally
            {
                _sprite.transform.localScale = _startScale;
                _sprite.color = _startColor;
            }
        });
    }

    private void HpBarUpdate()
    {
        if (_hpBar == null || _superArmorBar == null)
        {
            return;
        }


        if (_superArmor <= 0)
        {
            _superArmorBar.gameObject.SetActive(false);
        }
        else
        {
            _superArmorBar.gameObject.SetActive(true);
            _superArmorBar.maxValue = _data.SuperArmor;
            _superArmorBar.value = _superArmor;
        }

        if (_hp <= 0)
        {
            _hpBar.gameObject.SetActive(false);
        }
        else
        {
            _hpBar.gameObject.SetActive(true);
            _hpBar.maxValue = _data.Hp;
            _hpBar.value = _hp;
        }
    }


    public void TakeDamage(SDamageInfo damageInfo)
    {
        if (_hp <= 0)
        {
            return;
        }

        // 넉백
        if (damageInfo.HasDirection)
        {
            _rb.AddForce(damageInfo.IncomingDirection.normalized, ForceMode2D.Impulse);
        }

        // 데미지 계산
        float finalDamage = damageInfo.Damage;

        if (_superArmor > 0)
        {
            // 슈퍼아머 데미지 감소
            _superArmor -= finalDamage;
            finalDamage *= 0.5f;
            if (_superArmor <= 0)
            {
                _superArmor = 0f;
                StateChanged(_airbornState);
                _sprite.material.SetFloat(_outLineId, 0f);
            }
        }
        float reduction = 1 - (1.025f * Mathf.Log10(1f + (0.025f * _defense)));
        

        _hp -= finalDamage * reduction;

        CLog.Log($"데미지 : {finalDamage} / 감소율 : {reduction} / 최종값 {finalDamage * reduction}");

        HpBarUpdate();

        if (_hp <= 0)
        {
            _hp = 0f;
            StateChanged(_airbornState);
            return;
        }

        HultFeedback();
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(new SDamageInfo(damage, Vector2.zero));
    }
}
