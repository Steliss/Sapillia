using System;
using UnityEngine;
using UnityEngine.InputSystem;

public enum EGuardResult
{
    None,
    Guard,
    PerfectGuard
}

public enum EPlayerGuardState
{
    Idle,
    Guard,
    GuardBreak
}

public class CPlayerGuard : MonoBehaviour, IInputCommand<float>, IDamageDefense
{
    #region 인스펙터

    [Header("참조")]
    [SerializeField] private CInputCommandHub _inputHub;
    [SerializeField] private CPlayerController _playerController;
    [SerializeField] private CWeaponMotion _weaponMotion;

    [Header("가드 방향 조정")]
    [SerializeField] private Transform _playerTransform;
    [SerializeField] private CSwordAndShieldAim _aim;

    [Header("Guard Settings")]
    [Min(1)]
    [SerializeField] private int _guardManaCost = 10;

    [Min(0f)]
    [SerializeField] private float _perfectGuardWindow = 0.15f;

    [Header("Guard Break")]
    [Min(0f)]
    [SerializeField] private float _guardBreakDuration = 10f;

    #endregion

    #region 내부변수
    private CUniTaskOnes _inputTask = new CUniTaskOnes();

    private EPlayerGuardState _guardState = EPlayerGuardState.Idle;

    private bool _buttonHeld;
    private bool _perfectGuardUsed;

    // 가드 올릴 수 있는가를 판단
    private bool _guardInputArmed;
    // 특수공격 중 가드 차단
    private bool _specialAttackLocked;
    // 현재 우클릭에서 가드를 시작한 적이 있는지
    private bool _guardStartedThisHold;

    private float _guardStartTime;
    private float _guardBreakEndTime;

    #endregion

    #region 프로퍼티

    public EPlayerGuardState GuardState => _guardState;
    public bool IsGuarding => _guardState == EPlayerGuardState.Guard;
    public bool IsGuardBroken => _guardState == EPlayerGuardState.GuardBreak;
    public bool IsGuardRequested => _buttonHeld;
    public bool IsSpecialAttackLocked => _specialAttackLocked;
    public bool CanStartSpecialAttack => isActiveAndEnabled && _buttonHeld && !_specialAttackLocked;
    public int PerfectGuardManaCost => Mathf.CeilToInt(_guardManaCost * 0.5f);
    public float GuardBreakRemaining => IsGuardBroken ? Mathf.Max(0f, _guardBreakEndTime - Time.time) : 0f;
    public bool IsPerfectGuardWindow => IsGuarding && !_perfectGuardUsed && Time.time - _guardStartTime <= _perfectGuardWindow;
    #endregion

    #region 이벤트

    public event Action<EGuardResult> OnGuardSuccess;

    public event Action OnGuardBreakStart;
    public event Action OnGuardBreakEnd;

    #endregion

    #region 활성화, 비활성화, 업뎃

    private void OnEnable()
    {
        CLog.Log("[Guard] OnEnable 작동 완료");

        _inputTask.Run(async token =>
        {
            _inputHub = await CManagerDontDestroy.InputCommandHubAsync(token);
            _inputHub.Register(EKeyAction.SpecialAttack, (IInputCommand<float>)this);
            CLog.Log("CPlayerGuard OnEnable 작업 완료");
        });
        if (_inputHub != null)
        {
        }
    }

    private void OnDisable()
    {
        _inputTask.Cancel();

        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.SpecialAttack, (IInputCommand<float>)this);
        }

        _buttonHeld = false;
        _guardInputArmed = false;
        _specialAttackLocked = false;
        _guardStartedThisHold = false;
        _perfectGuardUsed = false;

        if (!IsGuardBroken)
        {
            _guardState = EPlayerGuardState.Idle;
        }
    }

    private void Update()
    {
        // _inputHub 안잡혔을때 잡힐때까지 대기
        if (_inputHub == null)
        {
            return;
        }

        // 테스트용: 실제 우클릭 입력 확인 ※
        if (Mouse.current != null && Mouse.current.rightButton.wasPressedThisFrame)
        {
            CLog.Log("[Guard] 실제 마우스 우클릭 감지");
        }

        // GuardBreak 상태 처리
        if (IsGuardBroken)
        {
            UpdateGuardBreak();
            return;
        }

        // 검 공격 종료를 기다리던 방어 요청
        if (_buttonHeld && !IsGuarding && _guardInputArmed)
        {
            TryStartGuard();
        }
    }

    #endregion

    #region Input

    public void Execute(float value)
    {
        
        bool pressed = value > 0.5f;

        // 우클릭 해제
        if (!pressed)
        {
            _buttonHeld = false;
            _guardInputArmed = false;
            _guardStartedThisHold = false;
            _perfectGuardUsed = false;

            EndGuard();
            CLog.Log("방어 해제");
            return;
        }        

        // 중복 입력 방지
        if (_buttonHeld)
        {
            return;
        }

        _buttonHeld = true;

        if (_guardInputArmed)
        {
            TryStartGuard();
        }
    }

    #endregion

    #region Guard State

    private void TryStartGuard()
    {
        // 특수 공격 도중 가드 금지
        if (_specialAttackLocked)
        {
            return;
        }

        // 가드 허용 여부
        if (!_guardInputArmed)
        {
            return;
        }

        if (IsGuarding || IsGuardBroken)
        {
            return;
        }

        // 검 공격 중에는 방어 시작 대기
        if (_weaponMotion != null && _weaponMotion.IsAttacking)
        {
            return;
        }

        _guardState = EPlayerGuardState.Guard;

        /*
        // 퍼펙트 가드 판정을 초기화
        if (!_guardStartedThisHold)
        {
            _guardStartTime = Time.time;
            _perfectGuardUsed = false;
            _guardStartedThisHold = true;
        }
        */

        // 퍼펙트 가드 판정 시작
        _guardStartTime = Time.time;
        _perfectGuardUsed = false;

    }

    private void EndGuard()
    {
        // GuardBreak는 우클릭 해제로 해제할 수 없음
        if (!IsGuarding)
        {
            return;
        }

        _guardState = EPlayerGuardState.Idle;
        _perfectGuardUsed = false;
    }

    #endregion

    #region 검과 방패 특수 공격
    public void BeginSpecialAttack()
    {
        _specialAttackLocked = true;

        // 실제 가드 중이라면 방패 내림
        if (IsGuarding)
        {
            _guardState = EPlayerGuardState.Idle;
        }

        // 가드 브레이크 시 현상태 유지

        CLog.Log("[CPlayerGuard] 특수공격 시작 가드 X");
    }

    // 특수공격 종료 시 호출
    public void EndSpecialAttack()
    {
        _specialAttackLocked = false;
                
        CLog.Log("[Guard] 특수공격 종료 - 가드 잠금 해제");
    }
    #endregion

    #region Guard Damage

    public int ResolveDamage(SDamageInfo damageInfo)
    {
        return ResolveDamageInternal(damageInfo, out _);
    }

    public int ResolveDamage(int damage, Vector2 attackOrigin, out EGuardResult result)
    {
        result = EGuardResult.None;

        if (damage <= 0)
        {
            return 0;
        }

        if (_playerTransform == null)
        {
            return damage;
        }

        Vector2 incomingDirection = (Vector2)_playerTransform.position - attackOrigin;

        SDamageInfo damageInfo = new SDamageInfo(damage, incomingDirection);

        return ResolveDamageInternal(damageInfo, out result);
    }

    private int ResolveDamageInternal(SDamageInfo damageInfo, out EGuardResult result)
    {
        result = EGuardResult.None;
        int damage = damageInfo.Damage;

        if (damage <= 0)
        {
            return 0;
        }

        // 가드 중이 아니면 전체 피해
        if (!IsGuarding)
        {
            return damage;
        }

        // 공격 방향 정보 확인
        if (!damageInfo.HasDirection || _aim == null)
        {
            return damage;
        }

        Vector2 shieldDirection = _aim.AimDirection;

        if (shieldDirection.sqrMagnitude < 0.0001f)
        {
            return damage;
        }

        Vector2 directionToAttack = -damageInfo.IncomingDirection;

        if (directionToAttack.sqrMagnitude < 0.0001f)
        {
            return damage;
        }

        // 방패 전방 180도 판정
        float dot = Vector2.Dot(shieldDirection.normalized, directionToAttack.normalized);

        // 후방 공격
        if (dot < 0f)
        {
            CLog.Log("[Guard] 후방 공격 - 방어 실패");
            return damage;
        }

        // 플레이어 데이터 확인
        if (_playerController == null || _playerController.PlayerData == null)
        {
            return damage;
        }

        CPlayerGamingDataSO playerData = _playerController.PlayerData;

        // 퍼펙트 가드 확인
        bool isPerfect = IsPerfectGuardWindow;

        int manaCost = isPerfect ? PerfectGuardManaCost : _guardManaCost;

        // MP 부족 시 GuardBreak
        if (playerData.CurrentMP < manaCost)
        {
            if (playerData.CurrentMP > 0)
            {
                playerData.TryUseMP(playerData.CurrentMP);
            }

            EnterGuardBreak();

            CLog.Warn("[Guard] MP 부족 - 방어 실패");
            return damage;
        }

        // MP 소비
        if (!playerData.TryUseMP(manaCost))
        {
            EnterGuardBreak();
            return damage;
        }

        // 방어 성공
        if (isPerfect)
        {
            _perfectGuardUsed = true;
            result = EGuardResult.PerfectGuard;

            CLog.Log($"[Guard] Perfect Guard / MP -{manaCost}");
        }
        else
        {
            result = EGuardResult.Guard;

            CLog.Log($"[Guard] Normal Guard / MP -{manaCost}");
        }

        OnGuardSuccess?.Invoke(result);

        // 방어 후 MP가 0이면 GuardBreak
        if (playerData.CurrentMP <= 0)
        {
            EnterGuardBreak();
        }

        // 방어 성공 → HP 피해 0
        return 0;
    }

    #endregion



    #region Guard Break

    private void EnterGuardBreak()
    {
        if (IsGuardBroken)
        {
            return;
        }

        _guardState = EPlayerGuardState.GuardBreak;

        // 우클릭 상태는 유지
        // 가드를 올리는 권한만 해제
        _guardInputArmed = false;
        _perfectGuardUsed = false;

        // 10초간 방어 불가
        _guardBreakEndTime = Time.time + _guardBreakDuration;

        CLog.Warn($"[PlayerGuard] GuardBreak! " + $"{_guardBreakDuration}초 동안 방어 불가");

        OnGuardBreakStart?.Invoke();
    }

    private void UpdateGuardBreak()
    {
        if (Time.time < _guardBreakEndTime)
        {
            return;
        }

        // GuardBreak 종료
        _guardState = EPlayerGuardState.Idle;
        _guardBreakEndTime = 0f;

        CLog.Log("[PlayerGuard] GuardBreak 종료");

        OnGuardBreakEnd?.Invoke();

        // 자동 방어하지 않음
        // 우클릭을 새로 눌러야 방어 가능
    }

    #endregion

    #region Guard Direction

    private bool IsInGuardRange(Vector2 attackOrigin)
    {
        if (_playerTransform == null || _aim == null)
        {
            return false;
        }

        // 방패가 바라보는 방향
        Vector2 shieldDirection = _aim.AimDirection;

        // 플레이어에서 공격이 시작된 위치로 향하는 방향
        Vector2 attackDirection = attackOrigin - (Vector2)_playerTransform.position;

        if (shieldDirection.sqrMagnitude < 0.0001f || attackDirection.sqrMagnitude < 0.0001f)
        {
            return false;
        }

        float dot = Vector2.Dot(shieldDirection.normalized, attackDirection.normalized);

        // -90도 ~ +90도
        return dot >= 0f;
    }

    #endregion
}