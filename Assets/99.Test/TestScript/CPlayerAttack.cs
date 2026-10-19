using UnityEngine;

public class CPlayerAttack : MonoBehaviour, IInputCommand
{
    #region 인스펙터

    [SerializeField] private CInputCommandHub _inputHub;
    [SerializeField] private Animator _weaponAnimator;

    [Header("Test")]
    [SerializeField] private float _attackInterval = 0.35f;

    #endregion

    #region 내부변수
    private float _nextAttackTime;
    private static readonly int AttackHash = Animator.StringToHash("tAttack");

    #endregion

    private void OnEnable()
    {
        if(_inputHub != null)
        {
            _inputHub.Register(EKeyAction.Attack, this);
        }
    }

    public void Execute()
    {
        TryAttack();
    }


    private void TryAttack()
    {
        if(_weaponAnimator == null)
        {
            return;
        }

        if(Time.time < _nextAttackTime)
        {
            return;
        }

        _nextAttackTime = Time.time * _attackInterval;

        _weaponAnimator.SetTrigger(AttackHash);
    }

  
}
