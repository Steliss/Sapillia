using UnityEngine;

public class CPlayerAnimation : MonoBehaviour
{
    #region 인스펙터

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CPlayerMovement _movement;
    [SerializeField] private CWeaponMotion _weaponMotion;

    [Header("Animation Speed")]
    [SerializeField] private float _walkAnimSpeed = 1f;
    [SerializeField] private float _runAnimSpeed = 1.5f;
    [SerializeField] private float _dashAnimSpeed = 1f;

    #endregion

    #region 파라미터
    private static readonly int IsMovingParam = Animator.StringToHash("bIsMoving");
    private static readonly int MoveAnimSpeedParam = Animator.StringToHash("fMoveAnimSpeed");
    private static readonly int FacingBackParam = Animator.StringToHash("bFacingBack");
    private static readonly int IsAttackingParam = Animator.StringToHash("bIsAttacking");
    private static readonly int AttackAnimSpeedParam = Animator.StringToHash("fAttackAnimSpeed");

    #endregion

    #region 내부변수
    // 현재 재생 중인 공격 번호 (1~3)
    private int _currentAttackIndex;

    // 공격 애니메이션의 앞/뒤 방향
    private bool _attackFacingBack;

    #endregion

    private void LateUpdate()
    {
        if(_animator == null)
        {
            return;
        }

        UpdateMovementAnimation();
        UpdateAttackAnimation();
    }

    // 이동 애니메이션 처리
    private void UpdateMovementAnimation()
    {
        if (_animator == null || _movement == null)
        {
            return;
        }

        EPlayerMoveState state = _movement.MoveState;
        bool isMoving = state != EPlayerMoveState.Idle;

        float animationSpeed = state switch
        {
            EPlayerMoveState.Run => _runAnimSpeed,
            EPlayerMoveState.Dash => _dashAnimSpeed,
            _ => _walkAnimSpeed
        };

        _animator.SetBool(IsMovingParam, isMoving);
        _animator.SetFloat(MoveAnimSpeedParam, animationSpeed);
    }

    // 공격 애니메이션 처리
    private void UpdateAttackAnimation()
    {
        if(_weaponMotion == null)
        {
            return;
        }

        bool isAttacking = _weaponMotion.IsAttacking;

        _animator.SetBool(IsAttackingParam, isAttacking);

        // 공격 종료 시
        if (!isAttacking)
        {
            _currentAttackIndex = 0;
            return;
        }

        if(_currentAttackIndex <= 0)
        {
            return;
        }

        // 앞 뒤 방향 확인
        bool facingBack = _animator.GetBool(FacingBackParam);

        if(facingBack  == _attackFacingBack)
        {
            return;
        }

        string previousState = GetAttackState(_currentAttackIndex,  _attackFacingBack);

        float normalizedTime = 0f;

        AnimatorStateInfo stateInfo = _animator.GetCurrentAnimatorStateInfo(0);

        if (stateInfo.IsName(previousState))
        {
            normalizedTime = Mathf.Clamp01(stateInfo.normalizedTime);
        }

        _attackFacingBack = facingBack;

        _animator.Play(GetAttackState(_currentAttackIndex, _attackFacingBack), 0, normalizedTime);
    }

    private string GetAttackState(int attackIndex, bool facingBack)
    {
        string direction = facingBack ? "Back" : "Front";

        return $"Base Layer.Attack_{attackIndex}_{direction}";
    }

    // CPlayerAttack에서 실제 공격 성공 시 호출
    public void PlayAttack(int attackIndex, float attackSpeed)
    {
        if (_animator == null)
        {
            return;
        }

        _currentAttackIndex = attackIndex;

        // 현재 마우스 방향에 따른 앞/뒷모습
        _attackFacingBack = _animator.GetBool(FacingBackParam);

        // 공격 애니메이션 재생 속도
        _animator.SetFloat(AttackAnimSpeedParam, Mathf.Max(0.01f, attackSpeed));
        _animator.SetBool(IsAttackingParam, true);

        // 해당 콤보 애니메이션 시작
        _animator.Play(GetAttackState(_currentAttackIndex, _attackFacingBack), 0, 0f);
    }
}