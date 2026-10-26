using UnityEngine;

public class CPlayerSpecialAttack : MonoBehaviour
{
    #region 인스펙터

    [Header("참조")]
    [SerializeField] private CPlayerController _playerController;
    [SerializeField] private CPlayerGuard _playerGuard;
    [SerializeField] private CWeaponMotion _weaponMotion;

    [Header("특수공격 데이터")]
    [SerializeField] private CWeaponAttackDataSO _specialAttackData;

    [Header("특수공격 설정")]
    [Min(1)]
    [SerializeField] private int _specialManaCost = 15;

    #endregion

    #region 내부변수
    // 현재 특수공격 진행 여부
    private bool _isPerforming;
    // 특수공격 후 좌클릭 해제 대기
    private bool _waitForLeftRelease;

    #endregion

    #region 프로퍼티
    public bool IsPerforming => _isPerforming;

    // 특수공격 중 또는 좌클릭 해제 대기 중
    // 기존 일반공격을 차단하는 데 사용
    public bool BlockNormalAttack => _isPerforming || _waitForLeftRelease;
    public int SpecialManaCost => _specialManaCost;

    #endregion

    #region Unity

    private void Update()
    {
        if (!_isPerforming)
        {
            return;
        }

        // 무기 공격이 진행 중이면 종료하지 않음
        if (_weaponMotion != null && _weaponMotion.IsAttacking)
        {
            return;
        }

        // 공격 모션과 Recovery까지 종료되면
        // 특수공격 종료 처리
        EndSpecialAttack();
    }

    private void OnDisable()
    {
        // 특수공격 중 컴포넌트가 비활성화되어도
        // 가드 잠금이 남지 않도록 처리
        if (_isPerforming && _playerGuard != null)
        {
            _playerGuard.EndSpecialAttack();
        }

        _isPerforming = false;
        _waitForLeftRelease = false;
    }

    #endregion

    #region Special Attack

    // 특수공격 실행 시도
    // 우클릭 유지 + 좌클릭 입력으로 호출
    public bool TryStart(int baseDamage, SPlayerStats stats)
    {
        // 특수공격 상태 확인
        if (!isActiveAndEnabled)
        {
            return false;
        }

        if (BlockNormalAttack)
        {
            return false;
        }

        // 우클릭 유지 여부 확인
        // GuardBreak 여부는 검사하지 않음
        if (_playerGuard == null || !_playerGuard.CanStartSpecialAttack)
        {
            return false;
        }

        // 필수 참조 확인
        if (_playerController == null || _weaponMotion == null || _specialAttackData == null)
        {
            CLog.Warn("[SpecialAttack] 참조 누락");
            return false;
        }

        // 플레이어 데이터 참조
        CPlayerGamingDataSO playerData = _playerController.PlayerData;

        if (playerData == null)
        {
            CLog.Warn("[SpecialAttack] PlayerData 누락");
            return false;
        }

        // 5. MP 확인
        if (playerData.CurrentMP < _specialManaCost)
        {
            CLog.Warn
                (
                    $"[SpecialAttack] MP 부족! " +
                    $"현재 MP : {playerData.CurrentMP} / " +
                    $"필요 MP : {_specialManaCost}"
                );

            return false;
        }

        // 무기 공격 가능 여부 확인
        if (!_weaponMotion.isActiveAndEnabled || !_weaponMotion.CanStart)
        {
            CLog.Log("[SpecialAttack] 현재 공격 실행 불가능");

            return false;
        }

        // MP 소비
        if (!playerData.TryUseMP(_specialManaCost))
        {
            CLog.Warn("[SpecialAttack] MP 부족");
            return false;
        }

        // 8. 기존 CWeaponMotion을 통해 특수공격 실행
        bool started = _weaponMotion.TryPlay(_specialAttackData, baseDamage, stats);

        // 공격 실행 실패 시 MP 반환
        if (!started)
        {
            playerData.RecoverMP(_specialManaCost);
            CLog.Warn("[SpecialAttack] 공격 실행 실패 - MP 반환");

            return false;
        }

        // 가드 해제 및 잠금
        _playerGuard.BeginSpecialAttack();

        // 특수공격 상태 변경
        _isPerforming = true;

        // 좌클릭 유지로 인한 일반공격 방지
        _waitForLeftRelease = true;

        CLog.Log("[SpecialAttack] 특수공격 시작");
        CLog.Log($"[SpecialAttack] MP -{_specialManaCost}");
        CLog.Log($"[SpecialAttack] 남은 MP : {playerData.CurrentMP}");

        return true;
    }

    // 특수공격 종료
    private void EndSpecialAttack()
    {
        if (!_isPerforming)
        {
            return;
        }

        _isPerforming = false;

        // 가드 잠금 해제
        // GuardBreak 상태는 그대로 유지
        if (_playerGuard != null)
        {
            _playerGuard.EndSpecialAttack();
        }

        CLog.Log("[SpecialAttack] 특수공격 종료");
    }

   // 좌클릭 해제 시 호출
 
    public void NotifyLeftReleased()
    {
        _waitForLeftRelease = false;
    }

    #endregion
}
