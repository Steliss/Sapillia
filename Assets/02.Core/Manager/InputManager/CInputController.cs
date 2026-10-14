using UnityEngine;


public class CInputController : MonoBehaviour
{

    #region 인스펙터
    [Header("커맨드")]
    [SerializeField] private CInputCommandHub _commandHub;

    [Header("이동")]
    [SerializeField] private float _moveSpeed = 6f;
    [SerializeField] private float _rotateSharpness = 15f;

    [Header("점프")]
    [SerializeField] private float _jumpPower = 6f;
    [SerializeField] private float _groundCheckDistance = 0.2f;
    [SerializeField] private LayerMask _groundLayer;
    #endregion

    #region 내부변수
    private Animator _playerAnimation;
    private Rigidbody _rb;

    private Vector2 _moveInput;
    private bool _jumpRequested;

    private IInputCommand<Vector2> _moveCommand;
    private IInputCommand _jumpCommand;
    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _playerAnimation = GetComponent<Animator>();

        // 커맨드에 실행할 함수 연결
        _moveCommand = new CValueCommand<Vector2>(SetMove);
        _jumpCommand = new CActionCommand(Jump);
    }

    private void OnEnable()
    {
        RegisterCommands();
    }

    private void OnDisable()
    {
        UnregisterCommands();

        _moveInput = Vector2.zero;
        _jumpRequested = false;
    }

    private void Update()
    {
        if (_playerAnimation != null)
        {
            _playerAnimation.SetFloat("fMove", _moveInput.magnitude, 0.15f, Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        Vector3 moveDir = BuildMoveDirection(_moveInput);

        Move(moveDir);
        TryJump();
        TickRotate(moveDir);
    }

    #region 커맨드 연결
    private void RegisterCommands()
    {
        if (_commandHub == null)
        {
            return;
        }

        _commandHub.Register(EInput.Move, _moveCommand);
        _commandHub.Register(EInput.Space, _jumpCommand);
    }

    private void UnregisterCommands()
    {
        if (_commandHub == null)
        {
            return;
        }

        _commandHub.Unregister(EInput.Move, _moveCommand);
        _commandHub.Unregister(EInput.Space, _jumpCommand);
    }

    // 씬에서 새로 생성된 플레이어에 Hub 연결
    public void SetCommandHub(CInputCommandHub commandHub)
    {
        UnregisterCommands();

        _moveInput = Vector2.zero;
        _jumpRequested = false;

        _commandHub = commandHub;

        if (isActiveAndEnabled)
        {
            RegisterCommands();
        }
    }
    #endregion


    #region 커맨드가 호출하는 함수
    public void SetMove(Vector2 direction)
    {
        _moveInput = Vector2.ClampMagnitude(direction, 1f);
    }

    public void Jump()
    {
        _jumpRequested = true;
    }
    #endregion


    #region 실제 이동 처리
    private void Move(Vector3 moveDir)
    {
        _rb.velocity = new Vector3(moveDir.x * _moveSpeed, _rb.velocity.y, moveDir.z * _moveSpeed);
    }

    private void TryJump()
    {
        if (!_jumpRequested)
        {
            return;
        }

        _jumpRequested = false;

        if (!CheckGround())
        {
            return;
        }

        if (_playerAnimation != null)
        {
            _playerAnimation.SetTrigger("tJump");
        }

        _rb.AddForce(Vector3.up * _jumpPower, ForceMode.Impulse);
    }

    private bool CheckGround()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        return Physics.Raycast(origin, Vector3.down, _groundCheckDistance, _groundLayer, QueryTriggerInteraction.Ignore);
    }

    private Vector3 BuildMoveDirection(Vector2 input)
    {
        return new Vector3(input.x, 0f, input.y);
    }

    private void TickRotate(Vector3 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.0001f)
        {
            return;
        }

        Quaternion target = Quaternion.LookRotation(moveDir, Vector3.up);
        Quaternion rotation = Quaternion.Slerp(_rb.rotation, target, 1f - Mathf.Exp(-_rotateSharpness * Time.fixedDeltaTime));

        _rb.MoveRotation(rotation);

    }
    #endregion

    private void OnDrawGizmos()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;

        Gizmos.color = CheckGround() ? Color.green : Color.red;
        Gizmos.DrawLine(origin, origin + Vector3.down * _groundCheckDistance);
    }
}