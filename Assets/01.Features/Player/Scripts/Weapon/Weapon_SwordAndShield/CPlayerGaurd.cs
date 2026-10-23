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

    private float _guardStartTime;
    private float _guardBreakEndTime;

    #endregion

    #region 프로퍼티

    public EPlayerGuardState GuardState => _guardState;
    public bool IsGuarding => _guardState == EPlayerGuardState.Guard;
    public bool IsGuardBroken => _guardState == EPlayerGuardState.GuardBreak;
    public bool IsGuardRequested => _buttonHeld;
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
        _perfectGuardUsed = false;

        _guardState = EPlayerGuardState.Idle;
        _guardBreakEndTime = 0f;
    }

    private void Update()
    {
        // _inputHub 안잡혔을때 잡힐때까지 대기
        if (_inputHub == null)
        {
            return;
        }

        // 테스트용: 실제 우클릭 입력 확인
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
        if (_buttonHeld && !IsGuarding)
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

            EndGuard();
            CLog.Log("방어 해제");
            return;
        }

        // GuardBreak 중에는 방어 입력 무시
        if (IsGuardBroken)
        {
            return;
        }

        // 중복 입력 방지
        if (_buttonHeld)
        {
            return;
        }

        _buttonHeld = true;

        TryStartGuard();
    }

    #endregion

    #region Guard State

    private void TryStartGuard()
    {
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

    #region Guard Damage

    public int ResolveDamage(SDamageInfo damageInfo)
    {
        int damage = damageInfo.Damage;

        if (damage <= 0)
        {
            CLog.Log($"[Guard]{damage}만큼 데미지 입음 1");
            return 0;
        }

        // 1. 방어 중이 아니거나 GuardBreak 상태
        if (!IsGuarding)
        {
            CLog.Log($"[Guard]{damage}만큼 데미지 입음 2");
            return damage;
        }

        // 2. 방향 정보가 없는 공격
        if (!damageInfo.HasDirection)
        {
            CLog.Log($"[Guard]{damage}만큼 데미지 입음 3");
            return damage;
        }

        if (_aim == null)
        {
            CLog.Log($"[Guard]{damage}만큼 데미지 입음 4");
            return damage;
        }

        // 3. 방패 방향 판정
        Vector2 shieldDirection = _aim.AimDirection;

        if (shieldDirection.sqrMagnitude < 0.0001f)
        {
            CLog.Log($"[Guard]{damage}만큼 데미지 입음 5");
            return damage;
        }

        // 실제 공격 진행 방향의 반대가 공격이 오는 쪽
        Vector2 directionToAttack = -damageInfo.IncomingDirection;

        float dot = Vector2.Dot(shieldDirection.normalized, directionToAttack);

        // 방패 기준 전방 -90도 ~ +90도
        if (dot < 0f)
        {
            CLog.Log($"[Guard]{damage}만큼 데미지 입음 6");
            return damage;
        }

        // 4. 플레이어 데이터 확인
        if (_playerController == null || _playerController.PlayerData == null)
        {
            CLog.Log($"[Guard]{damage}만큼 데미지 입음 7");
            return damage;
        }

        CPlayerGamingDataSO playerData = _playerController.PlayerData;

        // 5. 퍼펙트 가드 확인
        bool isPerfect = IsPerfectGuardWindow;

        int manaCost = isPerfect ? PerfectGuardManaCost : _guardManaCost;

        // 6. MP 부족 시 GuardBreak
        if (playerData.CurrentMP < manaCost)
        {
            if (playerData.CurrentMP > 0)
            {
                playerData.TryUseMP(playerData.CurrentMP);
            }

            EnterGuardBreak();

            // 이번 공격은 방어 실패
            CLog.Log($"{damage}만큼 데미지 입음");
            return damage;
        }

        // 7. MP 소비
        if (!playerData.TryUseMP(manaCost))
        {
            EnterGuardBreak();
            return damage;
        }

        // 8. 방어 성공
        if (isPerfect)
        {
            CLog.Log($"Perpect Guard");
            _perfectGuardUsed = true;
            OnGuardSuccess?.Invoke(EGuardResult.PerfectGuard);
        }
        else
        {
            CLog.Log("Guard");
            OnGuardSuccess?.Invoke(EGuardResult.Guard);
        }

        // 9. MP가 정확히 0이 됐다면 GuardBreak
        if (playerData.CurrentMP <= 0)
        {
            EnterGuardBreak();
        }

        // 성공한 방어는 피해 0
        return 0;
    }

    public int ResolveDamage(int damage, Vector2 attackOrigin, out EGuardResult result)
    {
        result = EGuardResult.None;

        if (damage <= 0)
        {
            return 0;
        }

        // 방어 중이 아니면 전체 피해
        if (!IsGuarding)
        {
            return damage;
        }

        // 방패 범위 확인
        if (!IsInGuardRange(attackOrigin))
        {
            // 후방 공격 시 방어X
            // 가드 판정 X
            return damage;
        }

        if (_playerController == null || _playerController.PlayerData == null)
        {
            return damage;
        }

        CPlayerGamingDataSO playerData = _playerController.PlayerData;

        // 퍼펙트 가드 여부 확인
        bool isPerfect = IsPerfectGuardWindow;

        // 방어 종류에 따른 MP 비용
        int manaCost = isPerfect ? PerfectGuardManaCost : _guardManaCost;

        // MP 소비 불가능 → GuardBreak
        if (playerData.CurrentMP < manaCost)
        {
            // 남은 MP를 0으로 소모
            if (playerData.CurrentMP > 0)
            {
                playerData.TryUseMP(playerData.CurrentMP);
            }

            EnterGuardBreak();

            // 이번 공격은 방어 실패
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
        }
        else
        {
            result = EGuardResult.Guard;
        }

        OnGuardSuccess?.Invoke(result);

        // 방어 성공 후 MP가 0이 됐다면 GuardBreak
        if (playerData.CurrentMP <= 0)
        {
            EnterGuardBreak();
        }

        // 성공한 방어는 HP 피해 0
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

        // 기존 방어 요청 취소
        _buttonHeld = false;
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