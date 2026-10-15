using UnityEngine;


public enum EPlayerMoveState
{
    Idle,
    Walk,
    Run,
    Dash
}

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CPlayerData))]
public class CPlayerMovement : MonoBehaviour, IInputCommand<Vector2>, IInputCommand<float>
{
    #region 인스펙터
    [Header("InputCommandHub")]
    [SerializeField] private CInputCommandHub _inputHub;
    [SerializeField] private Rigidbody2D _rb2D;
    [SerializeField] private CPlayerData _playerData;

    [Header("Test")]
    [SerializeField] private bool _isInCombat;

    #endregion

    #region 내부변수
    private Rigidbody2D _rb;
    private CPlayerData _playerdata;

    // 이동 관련
    private EPlayerMoveState _moveState = EPlayerMoveState.Idle;
    private Vector2 _moveDirection;
    private Vector2 _facingDirection = Vector2.down;
    private Vector2 _dashDirection;

    // 대쉬 관련
    private float _dashTimer;
    private float _dashCooldownTimer;

    private bool _spaceHeld;
    private bool _dashRequested;
    #endregion

    #region 프로퍼티
    public EPlayerMoveState MoveState => _moveState;

    public Vector2 MoveDirection => _moveDirection;
    public Vector2 FacingDirection => _facingDirection;

    public bool IsInCombat => _isInCombat;

    public float DashRemainingTime => _dashTimer;
    public float DashCoolRemaining => _dashCooldownTimer;

    #endregion



    private void Awake()
    {
        _rb = _rb2D;
        _playerdata = _playerData;
    }

    private void OnEnable()
    {
        if (_inputHub == null)
        {
            CLog.Error("CInputCommandHub 연결 오류");
            return;
        }

        RegisterInput();
    }

    private void OnDisable()
    {
        UnRegisterInput();

        _moveDirection = Vector2.zero;
        _spaceHeld = false;
        _dashRequested = false;

        _dashTimer = 0f;
        _dashCooldownTimer = 0f;

        _moveState = EPlayerMoveState.Idle;
    }

    private void FixedUpdate()
    {

    }



    void Start()
    {

    }

    void Update()
    {

    }

    // 커맨드 등록
    private void RegisterInput()
    {
        _inputHub.Register(EInput.Move, (IInputCommand<Vector2>)this);
        _inputHub.Register(EInput.Space, (IInputCommand<float>)this);
    }

    // 커맨드 해제
    private void UnRegisterInput()
    {
        if (_inputHub == null)
        {
            return;
        }

        _inputHub.Unregister(EInput.Move, (IInputCommand<Vector2>)this);
        _inputHub.Unregister(EInput.Space, (IInputCommand<float>)this);
    }

    // WASD 입력 시
    public void Excute(Vector2 direction)
    {
        _moveDirection = Vector2.ClampMagnitude(direction, 1f);
    }

    // Space 입력 시 (누름 : 1 / 땜 : 0)
    public void Excute(float value)
    {
        bool pressed = value > 0.5f;

        // 전투 상태에서는 새롭게 누를때만 대쉬
        if (_isInCombat && pressed && !_spaceHeld)
        {
            _dashRequested = true;
        }

        _spaceHeld = pressed;
    }

    // 대쉬 쿨타임
    private void UpdateDashCoolDown()
    {
        if(_dashCooldownTimer < 0f)
        {
            return;
        }
        
        _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - Time.fixedDeltaTime);
    }

    // 대쉬 시도
    private void TryStartDash()
    {
        if (!_dashRequested)
        {
            return;
        }

        _dashRequested = false;

        if (!_isInCombat)
        {
            return;
        }

        // 대쉬중이거나 쿨타임 진행중일 시 대쉬 불가능
        if(_dashTimer > 0f || _dashCooldownTimer > 0f)
        {
            return;
        }

        SPlayerStats stats = _playerdata.Stats;

        if (stats.DashSpeed <= 0f || stats.DashDuration <= 0f)
        {
            return;
        }

        // 대쉬 잔여 횟수 확인하고 없으면 탈출
        if (!_playerdata.TryConsumDash())
        {
            return;
        }
        
        // 대쉬 방향 설정
        _dashDirection = _moveDirection.sqrMagnitude > 0.00001f ? _moveDirection.normalized : _facingDirection;

        _facingDirection = _dashDirection;
        
        _dashTimer = stats.DashDuration;
        _dashCooldownTimer = stats.DashDuration;
    }

    private void UpdateMovement()
    {
        SPlayerStats stats = _playerdata.Stats;

        Vector2 velocity = Vector2.zero;

        // 대쉬 상태
        if (_dashTimer > 0f)
        {
            _moveState = EPlayerMoveState.Dash;

            velocity = _dashDirection * stats.DashSpeed;

            _dashTimer = Mathf.Max(0f, _dashTimer - Time.fixedDeltaTime);
        }
        // 정지 상태
        else if(_moveDirection.sqrMagnitude < 0.0001f)
        {
            _moveState = EPlayerMoveState.Idle;
        }
        // 걷기 / 뛰기
        else
        {
            bool canRun = !_isInCombat && _spaceHeld;

            _moveState = canRun ? EPlayerMoveState.Run : EPlayerMoveState.Walk;

            float speed = stats.Movement_Speed;

            if (canRun)
            {
                speed *= stats.Run_SpeedMulitplier;
            }

            velocity = _moveDirection * speed;
        }

        if(_moveState != EPlayerMoveState.Dash && _moveDirection.sqrMagnitude > 0.0001f)
        {
            _facingDirection = _moveDirection.normalized;
            // 나중에 마우스 방향으로 변경예정
        }

        _rb.MovePosition(_rb.position + velocity * Time.fixedDeltaTime);
    }

    // Combat 전투
    public void SetCombatState(bool isCombat)
    {
        if (_isInCombat == isCombat)
        {
            return;
        }

        _isInCombat = isCombat;

        // 비전투 진입 시 대쉬 취소
        if (!_isInCombat)
        {
            _dashRequested = false;
            _dashTimer = 0f;
        }
    }
   


}
