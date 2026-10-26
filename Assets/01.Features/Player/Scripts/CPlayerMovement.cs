
using UnityEngine;

public enum EPlayerMoveState
{
    Idle,
    Walk,
    Run,
    Dash
}

[RequireComponent(typeof(Rigidbody2D))]
public class CPlayerMovement : MonoBehaviour, IInputCommand<Vector2>, IInputCommand<float>
{
    #region 인스펙터

    [Header("참조")]
    // Test에서는 사용 ※
    [SerializeField] private CInputCommandHub _inputHub;
    [SerializeField] private Rigidbody2D _rb2D;
    [SerializeField] private CPlayerController _playerController;

    [Header("Test")]
    [SerializeField] private bool _isInCombat;

    #endregion

    #region 내부변수

    // 런타임 데이터
    private CPlayerGamingDataSO _playerData;
    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CPlayerRunSession _session;

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

    // 데이터 확인
    private bool _warnedMissingData;

    #endregion

    #region 프로퍼티

    public EPlayerMoveState MoveState => _moveState;

    public Vector2 MoveDirection => _moveDirection;
    public Vector2 FacingDirection => _facingDirection;
    public bool IsInCombat => _isInCombat;
    public float DashRemainingTime => _dashTimer;
    public float DashCoolRemaining => _dashCooldownTimer;

    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        if (_rb2D == null)
        {
            CLog.Error("[Movement] Rigidbody2D 참조 누락");
        }

        if (_playerController == null)
        {
            CLog.Log("[CPlayerMovement] session 연결 완료");
            _playerController = FindFirstObjectByType<CPlayerController>();
            
        }

        _session = FindFirstObjectByType<CPlayerRunSession>();

    }

    private void OnEnable()
    {       
        _inputTask.Run(async token =>
        {
            _inputHub = await CManagerDontDestroy.InputCommandHubAsync(token);
            RegisterInput();
            CLog.Log("[CPlayerMovement] OnEnable 작업 완료");
        });
        
    }   

    private void FixedUpdate()
    {
        // _inputHub 안잡혔을때 잡힐때까지 대기
        if (_inputHub == null)
        {
            return;
        }   

        if (_rb2D == null)
        {
            return;
        }

        if (!TryBindPlayerData())
        {
            return;
        }

        UpdateDashCoolDown();
        TryStartDash();
        UpdateMovement();
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        UnRegisterInput();

        _moveDirection = Vector2.zero;
        _spaceHeld = false;
        _dashRequested = false;

        _dashTimer = 0f;
        _dashCooldownTimer = 0f;

        _moveState = EPlayerMoveState.Idle;

        // 참조 해제
        _playerData = null;
        _warnedMissingData = false;
    }

    #endregion

    #region PlayerData

    private bool TryBindPlayerData()
    {
        CPlayerGamingDataSO currentData = _session.PlayerData;

        if(currentData == null || !currentData.IsInitialized)
        {
            if(_playerData != null)
            {
                _playerData = null;

                // 이동 상태 초기화
                _moveState = EPlayerMoveState.Idle;
                _moveDirection = Vector2.zero;
                _dashRequested = false;
                _dashTimer = 0f;
                _dashCooldownTimer = 0f;
            }
        }

        // Controller가 관리하는 현재 런타임 SO 참조
        _playerData = _session.PlayerData;

        if (_playerData == null)
        {
            if (!_warnedMissingData)
            {
                CLog.Warn("[Movement] PlayerData 연결 대기 중");
                _warnedMissingData = true;
            }

            if (_warnedMissingData)
            {
                CLog.Log("[Movement] PlayerData 연결 완료");
                _warnedMissingData = false;
            }

            return false;
        }

        // 새로운 런타임 데이터가 연결된 경우
        if (!ReferenceEquals(_playerData, currentData))
        {
            _playerData = currentData;
            CLog.Log("[Movement] PlayerData 연결 완료");
        }

        _warnedMissingData = false;

        return true;
    }

    #endregion

    #region Input Command

    private void RegisterInput()
    {
        _inputHub.Register(EKeyAction.Move, (IInputCommand<Vector2>)this);
        _inputHub.Register(EKeyAction.Dash, (IInputCommand<float>)this);
    }

    private void UnRegisterInput()
    {
        if (_inputHub == null)
        {
            return;
        }

        _inputHub.Unregister(EKeyAction.Move, (IInputCommand<Vector2>)this);
        _inputHub.Unregister(EKeyAction.Dash, (IInputCommand<float>)this);
    }

    // WASD
    public void Execute(Vector2 direction)
    {
        _moveDirection = Vector2.ClampMagnitude(direction, 1f);
    }

    // Space (누름: 1 / 뗌: 0)
    public void Execute(float value)
    {
        bool pressed = value > 0.5f;

        if (_isInCombat && pressed && !_spaceHeld)
        {
            _dashRequested = true;
        }

        _spaceHeld = pressed;
    }

    #endregion

    #region Dash

    private void UpdateDashCoolDown()
    {
        if (_dashCooldownTimer <= 0f)
        {
            return;
        }

        _dashCooldownTimer = Mathf.Max(0f, _dashCooldownTimer - Time.fixedDeltaTime);
    }

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

        if (_dashTimer > 0f || _dashCooldownTimer > 0f)
        {
            return;
        }

        SPlayerStats stats = _playerData.Stats;

        if (stats.DashSpeed <= 0f || stats.DashDuration <= 0f)
        {
            return;
        }

        if (!_playerData.TryConsumeDash())
        {
            return;
        }

        // 대쉬 방향 고정
        _dashDirection = _moveDirection.sqrMagnitude > 0.00001f ? _moveDirection.normalized : _facingDirection;

        _facingDirection = _dashDirection;

        _dashTimer = stats.DashDuration;
        _dashCooldownTimer = stats.DashCooldown;
    }

    #endregion

    #region Movement

    private void UpdateMovement()
    {
        SPlayerStats stats = _playerData.Stats;

        Vector2 velocity = Vector2.zero;

        // 대쉬
        if (_dashTimer > 0f)
        {
            _moveState = EPlayerMoveState.Dash;

            velocity = _dashDirection * stats.DashSpeed;

            _dashTimer = Mathf.Max(0f, _dashTimer - Time.fixedDeltaTime);
        }
        // 정지
        else if (_moveDirection.sqrMagnitude < 0.0001f)
        {
            _moveState = EPlayerMoveState.Idle;
        }
        // 걷기 / 달리기
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

        // 바라보는 방향
        if (_moveState != EPlayerMoveState.Dash && _moveDirection.sqrMagnitude > 0.0001f)
        {
            _facingDirection = _moveDirection.normalized;
        }

        // Rigidbody 이동
        _rb2D.MovePosition(_rb2D.position + velocity * Time.fixedDeltaTime);
    }

    #endregion

    #region Combat

    public void SetCombatState(bool isCombat)
    {
        if (_isInCombat == isCombat)
        {
            return;
        }

        _isInCombat = isCombat;

        if (!_isInCombat)
        {
            _dashRequested = false;
            _dashTimer = 0f;
        }
    }

    #endregion


}



