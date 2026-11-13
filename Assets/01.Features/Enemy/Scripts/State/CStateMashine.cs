using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
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
    [SerializeField] private LayerMask _targetMask;


    [Header("스폰 SO")]
    [SerializeField] private CEnemySpawnStateSO _spawnState;

    [Header("공격 패턴 SO")]
    [SerializeField] private CEnemyAttackStateSO _attackState;

    [Header("페이즈 전환용 SO. 비우면 기본 사망")]
    [SerializeField] private CEnemyPhaseStateSO _phaseSO;

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

    [Header("드롭 재화 오브젝트")]
    [SerializeField] private GameObject _exp;
    [SerializeField] private GameObject _expMini;
    [SerializeField] private GameObject _money;
    [SerializeField] private GameObject _moneyMini;
    #endregion

    #region 내부 변수
    private Animator _ani;
    private NavMeshAgent _nav;
    private SpriteRenderer _sprite;
    private Rigidbody2D _rb;
    private Collider2D _coll;
    private bool _isPatrol = true;
    private CUniTaskOnes _attackUni = new CUniTaskOnes();
    private CUniTaskOnes _hurtUni = new CUniTaskOnes();
    private Transform _target;
    private Vector3 _startScale;
    private Color _startColor;
    private readonly int _outLineId = Shader.PropertyToID("_OutlineOn");
    private List<GameObject> _gameObjects = new List<GameObject>();
    #endregion

    #region 스테이트
    private IEnemyState _idleState;
    private IEnemyState _moveState;
    private IEnemyState _airbornState;
    private IEnemyState _deathState;
    private IEnemyState _state;
    #endregion

    #region 스탯 데이터
    private CDataEnemySO _data;
    private float _hp;
    private float _superArmor;
    private float _defense;
    private float _attack;
    private float _speed;
    private int _extraLife;
    #endregion

    #region 프로퍼티
    public string Id => _id;
    public Animator Ani => _ani;
    public NavMeshAgent Nav => _nav;
    public SpriteRenderer Sprite => _sprite;
    public Rigidbody2D Rigidbody => _rb;
    public Collider2D Collider => _coll;
    public bool IsPatrol => _isPatrol;
    public CUniTaskOnes AttackUni => _attackUni;
    public Transform Target => _target;
    public float PatrolMoveRange => _patrolMoveRange;
    public float PatrolRadius => _patrolRadius;
    public LayerMask TargetLayer => _targetMask;

    // 상태
    public IEnemyState State => _state;
    public IEnemyState IdleState => _idleState;
    public IEnemyState MoveState => _moveState;
    public IEnemyState DeathState => _deathState;
    public IEnemyState AttackState => _attackState;

    // 스탯
    public float Hp => _hp;
    public float SuperArmor => _superArmor;
    public float Defense => _defense;
    public float Attack => _attack;
    public float Speed => _speed;
    public int ExtraLife => _extraLife;

    // 애니메이터 해쉬
    public int HashMove { get; private set; }
    public int HashAttack { get; private set; }
    public int HashAttReady { get; private set; }
    public int HashAir { get; private set; }
    public int HashIdle { get; private set; }
    public int HashDown { get; private set; }
    #endregion


    #region 유니티 이벤트 함수들
    private void Awake()
    {
        // id가 비어있을 경우 오브젝트 이름으로 id 설정
        if (string.IsNullOrWhiteSpace(_id))
        {
            _id = gameObject.name;
        }

        // 상태 생성
        if (_spawnState != null)
        {
            CEnemySpawnStateSO spawnSO = Instantiate(_spawnState);
            spawnSO.Init(this);
            _spawnState = spawnSO;
        }
        if (_attackState != null)
        {
            CEnemyAttackStateSO attSO = Instantiate(_attackState);
            attSO.Init(this);
            _attackState = attSO;
        }
        _idleState = new CEnemyState_Idle(this);
        _moveState = new CEnemyState_Move(this);
        _airbornState = new CEnemyState_Airborn(this);

        if (_phaseSO == null)
        {
            _deathState = new CEnemyState_Death(this);
        }
        else
        {
            CEnemyPhaseStateSO phase = Instantiate(_phaseSO);
            phase.Init(this);
            _deathState = phase;
        }


        // 애니메이터 해시 세팅 
        AnimatorSet();

        // 네브메쉬에이전트, 리지드바디 가져오기
        _nav = GetComponent<NavMeshAgent>();

        if (_nav == null)
        {
            CLog.Warn($"{name} : 네브메쉬 에이전트 컴포넌트 누락");
            gameObject.AddComponent<NavMeshAgent>();
            _nav = GetComponent<NavMeshAgent>();
        }
        _nav.updateRotation = false;
        _nav.updateUpAxis = false;


        if (_sprite == null)
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

        _coll = GetComponent<Collider2D>();
        if (_coll == null)
        {
            CLog.Warn($"{name} : 콜라이더2D 컴포넌트 누락");
            gameObject.AddComponent<Collider2D>();
            _coll = GetComponent<Collider2D>();
        }

        _startScale = _sprite.transform.localScale;
        _startColor = _sprite.color;
    }

    private void OnEnable()
    {
        // 데이터 베이스에서 데이터 갱신
        EnemyInit();
    }

    private void OnDisable()
    {
        GameObjectsListClear();
        _attackUni.Cancel();
        _hurtUni.Cancel();
    }


    private void Update()
    {
        State?.StateUpdate();
    }

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

    // 주변 플레이어 검색
    public void PatrolSearch(float radius)
    {
        if (!_isPatrol)
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, radius, _targetMask);

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
        _rb.simulated = true;

        if (_data != null)
        {
            _hp = _data.Hp;
            _defense = _data.Defense;
            _attack = _data.Attack;
            _speed = _data.Speed;
            _extraLife = _data.ExtraLife;

            Nav.speed = _speed;

            // 슈퍼아머 확률 획득
            if (_data.SuperArmorRate > 0f)
            {
                float ran = UnityEngine.Random.Range(0f, 100f);
                if (ran <= _data.SuperArmorRate)
                {
                    SuperArmorGet();
                }
            }
            float hasSuperArmor = _superArmor > 0f ? 1f : 0f;
            _sprite.material.SetFloat(_outLineId, hasSuperArmor);
        }


        // 초기 상태 지정
        if (_spawnState != null)
        {
            StateChanged(_spawnState);
        }
        else
        {
            StateChanged(_idleState);
        }
    }

    // 피격 피드백
    private void HultFeedback()
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

    private void DropItem()
    {
        int money = _data.Money / 10;
        int moneyMini = _data.Money % 10;
        int exp = _data.Exp / 10;
        int expMini = _data.Exp % 10;

        CObjectPoolManager objPool = CObjectPoolManager.Instance;

        if (objPool == null)
        {
            CLog.Warn("오브젝트 풀 없음! 재화 드롭 취소");
            return;
        }
        
        // 돈 드랍
        for (int i = 0; i < money; i++)
        {
            GameObject moneyObject = objPool.GetPoolObject(_money);
            if (moneyObject != null)
            {
                moneyObject.transform.position = transform.position;
                CEnemyProjectile_DropItem item = moneyObject.GetComponent<CEnemyProjectile_DropItem>();
                item?.SetWhatis(CEnemyProjectile_DropItem.EIm.Money, 10, transform.position);
            }
        }
        if (moneyMini > 0)
        {
            GameObject moneyMiniObject = objPool.GetPoolObject(_moneyMini);
            if (moneyMiniObject != null)
            {
                moneyMiniObject.transform.position = transform.position;
                CEnemyProjectile_DropItem item = moneyMiniObject.GetComponent<CEnemyProjectile_DropItem>();
                item?.SetWhatis(CEnemyProjectile_DropItem.EIm.Money, moneyMini, transform.position);
            }
        }

        // 경험치 드랍
        for (int i = 0; i < exp; i++)
        {
            GameObject expObject = objPool.GetPoolObject(_exp);
            if (expObject != null)
            {
                expObject.transform.position = transform.position;
                CEnemyProjectile_DropItem item = expObject.GetComponent<CEnemyProjectile_DropItem>();
                item?.SetWhatis(CEnemyProjectile_DropItem.EIm.Exp, 10, transform.position);
            }
        }
        if (expMini > 0)
        {
            GameObject expMiniObject = objPool.GetPoolObject(_expMini);
            if (expMiniObject != null)
            {
                expMiniObject.transform.position = transform.position;
                CEnemyProjectile_DropItem item = expMiniObject.GetComponent<CEnemyProjectile_DropItem>();
                item?.SetWhatis(CEnemyProjectile_DropItem.EIm.Exp, expMini, transform.position);
            }
        }
    }

    public void HpBarUpdate()
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

    // 꺼야할 오브젝트 등록
    public void GameObjectsListAdd(GameObject go)
    {
        if (go == null)
        {
            return;
        }

        _gameObjects.Add(go);
    }

    // 생성한 오브젝트 끄기
    public void GameObjectsListClear()
    {
        if (_gameObjects.Count > 0)
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                if (_gameObjects[i] != null &&_gameObjects[i].activeSelf)
                {
                    _gameObjects[i].SetActive(false);
                }
            }
            _gameObjects.Clear();
        }
    }

    public void SpriteFlipX(bool flip)
    {
        bool isLeft = _sprite.transform.localScale.x < 0f;
        // 이미 같은곳을 바라보는중
        if (isLeft == flip)
        {
            return;
        }

        Vector3 scale = _sprite.transform.localScale;
        scale.x *= -1f;
        _sprite.transform.localScale = scale;
    }

    public void SuperArmorGet()
    {
        _superArmor = _data.SuperArmor;
        _sprite.material.SetFloat(_outLineId, 1f);
    }

    // 목숨이 1 이상일때 리셋
    public bool TryExtraLife()
    {
        if (_extraLife > 0)
        {
            int life = _extraLife - 1;
            EnemyInit();
            _extraLife = life;
            return true;
        }
        return false;
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
                _attackUni.Cancel();
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
            _sprite.color = Color.gray;
            // 7초뒤 비활성화 예약
            _hurtUni.Run(async ct =>
            {
                await UniTask.Delay(TimeSpan.FromSeconds(7), cancellationToken: ct);
                CEnemySpawner spawner = GetComponentInParent<CEnemySpawner>();
                if (spawner != null)
                {
                    spawner.EnemyDie();
                }
                CLog.Warn("예외 발생! 강제 비활성화");
                gameObject.SetActive(false);
            });
            DropItem();
            _attackUni.Cancel();
            StateChanged(_airbornState);
            return;
        }

        HultFeedback();

        // 순찰중에 맞은 경우 플레이어를 강제로 찾기
        if (_isPatrol)
        {
            PatrolSearch(_patrolRadius * 10f);
        }
    }

    public void TakeDamage(int damage)
    {
        TakeDamage(new SDamageInfo(damage, Vector2.zero));
    }
}
