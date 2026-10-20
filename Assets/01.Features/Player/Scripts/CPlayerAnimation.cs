using UnityEngine;

public class CPlayerAnimation : MonoBehaviour
{
    #region 인스펙터

    [Header("References")]
    [SerializeField] private Animator _animator;
    [SerializeField] private CPlayerMovement _movement;

    [Header("Animation Speed")]
    [SerializeField] private float _walkAnimSpeed = 1f;
    [SerializeField] private float _runAnimSpeed = 1.5f;
    [SerializeField] private float _dashAnimSpeed = 1f;

    #endregion

    private static readonly int IsMovingParam = Animator.StringToHash("bIsMoving");
    private static readonly int MoveAnimSpeedParam = Animator.StringToHash("fMoveAnimSpeed");

    private void Update()
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
}