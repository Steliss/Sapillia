using UnityEngine;

public class CPlayerAttack : MonoBehaviour, IInputCommand
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

    [Header("Test")]
    [SerializeField] private float _attackInterval = 0.35f;

    #endregion

    #region 내부변수
    private int _nextComboIndex;
    private float _lastAttackTime = float.NegativeInfinity;
    private bool _attackQueued;
    #endregion

    private void OnEnable()
    {
        if(_inputHub != null)
        {
            _inputHub.Register(EKeyAction.Attack, this);
        }
    }

    private void OnDisable()
    {
        if(_inputHub != null)
        {
            _inputHub.Unregister(EInput.MouseL, (IInputCommand)this);
        }

        _attackQueued = false;
        _nextComboIndex = 0;
    }
    private void Update()
    {
        if (_weaponMotion == null)
        {
            CLog.Warn($"{this} 무기 비어있음");
            return;
        }

        // 예약된 다음 공격 실행
        if (_attackQueued && _weaponMotion.CanStart)
        {
            _attackQueued = false;
            StartNextAttack();
        }

        // 오랫동안 공격하지 않았다면 콤보 초기화
        if (!_attackQueued && _weaponMotion.CanStart && Time.time - _lastAttackTime > _comboResetTime)
        {
            _nextComboIndex = 0;
        }
    }

    public void Execute()
    {
        if (_weaponMotion == null)
        {
            CLog.Warn($"{this}.Excute 무기 비어있음");
            return;
        }

        if (_weaponMotion.CanStart)
        {
            StartNextAttack();
            return;
        }
        // 공격 중 후반부 입력은 다음 공격으로 예약
        if (_weaponMotion.Progress >= _inputBufferStart)
        {
            _attackQueued = true;
        }
    }

    private void StartNextAttack()
    {
        /*
        if (_weaponData == null || _playerController == null || _playerController.PlayerData == null)
        {
            CLog.Warn($"{_weaponData}.StartNextAttack 문제 있음");
            
            CLog.Warn($"{_playerController.PlayerData}.StartNextAttack 문제 있음");
            return;
        }*/

        // ※ 테스트용도
        if (_weaponData == null)
        {
            CLog.Warn("[PlayerAttack] WeaponData 누락");
            return;
        }

        if (_weaponMotion == null)
        {
            CLog.Warn("[PlayerAttack] WeaponMotion 누락");
            return;
        }

        if (_playerController == null)
        {
            CLog.Warn("[PlayerAttack] PlayerController 누락");
            return;
        }        

        if (_playerController.PlayerData == null)
        {
            CLog.Warn($"[PlayerAttack.PlayerData] PlayerController 누락");
        }

        if (_weaponData.AttackCount <= 0)
        {
            CLog.Warn("[PlayerAttack] 공격 패턴 없음");
            return;
        }

        // 콤보 유효 시간 확인
        if (Time.time - _lastAttackTime > _comboResetTime)
        {
            _nextComboIndex = 0;
        }

        CWeaponAttackDataSO attack = _weaponData.GetAttack(_nextComboIndex);

        if (attack == null)
        {
            CLog.Warn("[PlayerAttack] AttackData 누락");
            return;
        }            

        SPlayerStats stats = _playerController.PlayerData.Stats;

        if (!_weaponMotion.TryPlay(attack, _weaponData.BaseDamage, stats))
        {
            return;
        }            

        // 다음 공격 인덱스
        _nextComboIndex = (_nextComboIndex + 1) % _weaponData.AttackCount;
        _lastAttackTime = Time.time;
    }




}
