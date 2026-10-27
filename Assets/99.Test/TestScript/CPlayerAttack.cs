using UnityEngine;

public class CPlayerAttack : MonoBehaviour, IInputCommand<float>
{
    #region 인스펙터

    [Header("입력 참조")]
    [SerializeField] private CInputCommandHub _inputHub;
    [SerializeField] private CPlayerController _playerController;

    [Header("애니메이션")]
    [SerializeField] private CPlayerAnimation _playerAnimation;

    [Header("무기")]
    [SerializeField] private CWeaponDataSO _weaponData;
    [SerializeField] private CWeaponMotion _weaponMotion;

    [Header("특수공격")]
    [SerializeField] private CPlayerGuard _playerGuard;
    [SerializeField] private CPlayerSpecialAttack _specialAttack;

    [Header("연속공격")]
    [Min(0f)]
    [SerializeField] private float _comboResetTime = 1.0f;

    [Range(0f, 1f)]
    [SerializeField] private float _inputBufferStart = 0.5f;

    #endregion

    #region 내부변수
    private CUniTaskOnes _inputTask = new CUniTaskOnes();

    private int _nextComboIndex;
    // 콤보 유지 시간
    private float _comboExpireTime = float.NegativeInfinity;
    // 좌클릭 유지 여부
    private bool _isAttackHeld;
    // 다음 공격 예약
    private bool _attackQueued;
    // 일반공격 / 특수공격 공통 입력 중복 방지
    private bool _leftButtonHeld;
    #endregion

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _inputHub = await CManagerDontDestroy.InputCommandHubAsync(token);
            _inputHub.Register(EKeyAction.Attack, (IInputCommand<float>)this);
            CLog.Log("CPlayerAttack OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask.Cancel();

        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Attack, (IInputCommand<float>)this);
        }

        _leftButtonHeld = false;
        _isAttackHeld = false;
        _attackQueued = false;

        ResetCombo();

        if (_specialAttack != null)
        {
            _specialAttack.NotifyLeftReleased();
        }

       // _comboExpireTime = float.NegativeInfinity;
        //_nextComboIndex = 0;
    }
    private void Update()
    {
        // _inputHub 안잡혔을때 잡힐때까지 대기
        if(_inputHub == null)
        {
            return;
        }

        if (_weaponMotion == null)
        {
            CLog.Warn($"{this} 무기 비어있음");
            return;
        }

       

        // 1. 특수공격 진행 중에는
        // 일반공격과 예약된 콤보 실행 차단
        if (_specialAttack != null && _specialAttack.BlockNormalAttack)
        {
            CLog.Log($"[CPlayerAttacks] 특수 공격 진행중이라 일반 공격 예약 해제");
            return;
        }
        
        // 2. 우클릭 유지 중에는 일반공격 시작 금지
        // 기존 예약 공격도 취소
        if (_playerGuard != null && _playerGuard.IsGuardRequested)
        {
            CLog.Log($"[CPlayerAttacks] 특수 공격 진행중이라 일반 공격 해제");
            _isAttackHeld = false;
            _attackQueued = false;
            return;
        }

         // 현재 공격 끝까지 수행
        if (!_weaponMotion.CanStart)
        {
            return;
        }
        
        // 3. 현재 공격 종료 대기
        if (!_weaponMotion.CanStart)
        {
            CLog.Log($"[CPlayerAttacks] 일반 공격 진행 중");
            return;
        }

        // 좌클릭 유지 또는 예약된 공격
        if (_isAttackHeld || _attackQueued)
        {
            if (StartNextAttack())
            {
                _attackQueued = false;
            }

            CLog.Log($"[CPlayerAttacks] 좌클릭 유지중");

            return;
        }


        // 오랫동안 공격하지 않았다면 콤보 초기화
        if (!_attackQueued && Time.time > _comboExpireTime)
        {
            _nextComboIndex = 0;
        }
    }

    public void Execute(float value)
    {
        bool pressed = value > 0.5f;


        // 좌클릭 땜
        if (!pressed)
        {
            _leftButtonHeld = false;
            _isAttackHeld = false;

            if (_specialAttack != null)
            {
                _specialAttack.NotifyLeftReleased();
            }

            return;
        }

        // 2. 같은 좌클릭의 중복 입력 방지
        if (_leftButtonHeld)
        {
            return;
        }

        _leftButtonHeld = true;


        // 3. 이미 특수공격 중이라면
        // 일반공격 입력으로 전달하지 않음
        if (_specialAttack != null && _specialAttack.BlockNormalAttack)
        {
            _isAttackHeld = false;
            _attackQueued = false;
            return;
        }

        // 4. 우클릭 중이라면 특수공격 우선
        if (_playerGuard != null && _playerGuard.IsGuardRequested)
        {
            TrySpecialAttack();
            return;
        }

        // 5. 우클릭이 없으면 기존 일반공격
        if (_weaponMotion == null)
        {
            CLog.Warn("[PlayerAttack] WeaponMotion 누락");
            return;
        }

        _isAttackHeld = true;

        // 즉시 일반공격 가능
        if (_weaponMotion.CanStart)
        {
            if (StartNextAttack())
            {
                _attackQueued = false;
            }
            
            return;
        }
        // 공격 중 후반부 입력은 다음 공격으로 예약
        if (_weaponMotion.IsAttacking && _weaponMotion.Progress >= _inputBufferStart)
        {
            _attackQueued = true;
        }
    }

    #region Special Attack

    private void TrySpecialAttack()
    {
        CLog.Log("[PlayerAttack] TrySpecialAttack 진입 성공");

        // 기존 일반공격 입력과 예약 제거
        _isAttackHeld = false;
        _attackQueued = false;

        // 1. 특수공격 참조 확인
        if (_specialAttack == null)
        {
            CLog.Warn("[PlayerAttack] SpecialAttack 참조 누락");
            return;
        }

        if (_playerController == null || _playerController.PlayerData == null)
        {
            CLog.Warn("[PlayerAttack] PlayerData 누락");
            return;
        }

        if (_weaponData == null)
        {
            CLog.Warn("[PlayerAttack] WeaponData 누락");
            return;
        }

        CLog.Log("[PlayerAttack] TryStart 호출 직전");

        // 2. 런타임 플레이어 데이터
        CPlayerGamingDataSO playerData = _playerController.PlayerData;

        // 3. 특수공격 실행 요청
        bool started = _specialAttack.TryStart(_weaponData.BaseDamage, playerData.Stats);

        if (!started)
        {
            CLog.Log("[PlayerAttack] 특수공격 발동 실패");
            return;
        }

        // 4. 특수공격 성공 시 기존 콤보 초기화
        ResetCombo();

        CLog.Log("[PlayerAttack] 특수공격 발동 성공");
    }

    #endregion

    #region Normal Attack

    private bool StartNextAttack()
    {

        // 특수공격 진행 중 일반공격 불가능
        if (_specialAttack != null && _specialAttack.BlockNormalAttack)
        {
            return false;
        }

        // 우클릭을 유지 중이면 일반공격 불가능
        if (_playerGuard != null && _playerGuard.IsGuardRequested)
        {
            return false;
        }


        // ※ 테스트용도
        if (_weaponData == null)
        {
            CLog.Warn("[PlayerAttack] WeaponData 누락");
            return false;
        }

        if (_weaponMotion == null)
        {
            CLog.Warn("[PlayerAttack] WeaponMotion 누락");
            return false;
        }

        if (_playerController == null)
        {
            CLog.Warn("[PlayerAttack] PlayerController 누락");
            return false;
        }
        
        if (_playerController.PlayerData == null)
        {
            CLog.Warn($"[PlayerAttack.PlayerData] PlayerController 누락");
            return false;
        }

        if (_weaponData.AttackCount <= 0)
        {
            CLog.Warn("[PlayerAttack] 공격 패턴 없음");
            return false;
        }

        // 콤보 유효 시간 확인 → 콤보 시간 지났으면 1타부터 시작
        if (Time.time > _comboExpireTime)
        {
            _nextComboIndex = 0;
        }

        CWeaponAttackDataSO attack = _weaponData.GetAttack(_nextComboIndex);

        if (attack == null)
        {
            CLog.Warn("[PlayerAttack] AttackData 누락");
            return false;
        }            

        SPlayerStats stats = _playerController.PlayerData.Stats;

        // 일반 공격 시작
        if (!_weaponMotion.TryPlay(attack, _weaponData.BaseDamage, stats))
        {
            return false;
        }

        // 실제 무기 공격이 시작됐을 때 애니메이션 재생
        if (_playerAnimation != null)
        {
            _playerAnimation.PlayAttack(_nextComboIndex + 1, stats.Attack_Speed);
        }

        // 다음 공격 인덱스
        _nextComboIndex = (_nextComboIndex + 1) % _weaponData.AttackCount;

        // 공격 속도 반영
        float speed = Mathf.Max(0.01f, stats.Attack_Speed);
        float duration = Mathf.Max(0.01f, attack.Duration / speed);
        float recovery = Mathf.Max(0f, attack.Recovery / speed);

        // 현재 공격이 끝난 후에도 콤보 유지 시간 적용
        _comboExpireTime = Time.time + duration + recovery + _comboResetTime;

        return true;
    }
    #endregion

    #region Combo

    private void ResetCombo()
    {
        _nextComboIndex = 0;
        _comboExpireTime = float.NegativeInfinity;
        _attackQueued = false;
    }

    #endregion


}
