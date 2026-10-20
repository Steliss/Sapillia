using UnityEngine;

public class CPlayerAttack : MonoBehaviour, IInputCommand<float>
{
    #region 인스펙터

    [Header("입력 참조")]
    [SerializeField] private CInputCommandHub _inputHub;
    [SerializeField] private CPlayerController _playerController;

    [Header("무기")]
    [SerializeField] private CWeaponDataSO _weaponData;
    [SerializeField] private CWeaponMotion _weaponMotion;

    [Header("연속공격")]
    [Min(0f)]
    [SerializeField] private float _comboResetTime = 1.0f;

    [Range(0f, 1f)]
    [SerializeField] private float _inputBufferStart = 0.5f;

    #endregion

    #region 내부변수
    private int _nextComboIndex;
    // 콤보 유지 시간
    private float _comboExpireTime = float.NegativeInfinity;
    // 좌클릭 유지 여부
    private bool _isAttackHeld;
    // 다음 공격 예약
    private bool _attackQueued;
    #endregion

    private void OnEnable()
    {
        if(_inputHub != null)
        {
            _inputHub.Register(EKeyAction.Attack, (IInputCommand<float>)this);
        }
    }

    private void OnDisable()
    {
        if(_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Attack, (IInputCommand<float>)this);
        }

        _isAttackHeld = false;
        _attackQueued = false;

        _comboExpireTime = float.NegativeInfinity;
        _nextComboIndex = 0;
    }
    private void Update()
    {
        if (_weaponMotion == null)
        {
            CLog.Warn($"{this} 무기 비어있음");
            return;
        }

        // 현재 공격 끝까지 수행
        if (!_weaponMotion.CanStart)
        {
            return;
        }

        // 좌클릭 유지 또는 예약된 공격
        if (_isAttackHeld || _attackQueued)
        {
            if (StartNextAttack())
            {
                _attackQueued = false;
            }

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
                
        if (_weaponMotion == null)
        {
            CLog.Warn($"{this}.Excute 무기 비어있음");
            return;
        }

        // 좌클릭 땜
        if (!pressed)
        {
            _isAttackHeld = false;
            return;
        }

        if (_isAttackHeld)
        {
            return;
        }

        _isAttackHeld = true;        

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

    private bool StartNextAttack()
    {
       
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

        if (!_weaponMotion.TryPlay(attack, _weaponData.BaseDamage, stats))
        {
            return false;
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




}
