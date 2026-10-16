using UnityEngine;

public class CPlayerCostume : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private Animator _animator;


    #endregion

    #region 내부변수
    // 런타임중에 애니메이터 가져오기
    private RuntimeAnimatorController _baseController;
    private CCostumeDataSO _currentCostume;

    #endregion

    #region 프로퍼티
    public CCostumeDataSO CurrentCostume => _currentCostume;
    #endregion


    private void Awake()
    {
        if (_animator == null)
        {
            _animator = GetComponentInChildren<Animator>(true);
        }

        if (_animator == null)
        {
            CLog.Error("Animator 누락");
            enabled = false;
            return;
        }

        _baseController = _animator.runtimeAnimatorController;
    }

    public void ApplyCostume(CCostumeDataSO costume)
    {
        if(_animator == null)
        {
            return;
        }

        RuntimeAnimatorController controller = costume != null && costume.AnimatorOverride != null ? costume.AnimatorOverride : _baseController;

        if(_currentCostume == costume && _animator.runtimeAnimatorController ==  controller)
        {
            return;
        }

        _currentCostume = costume;

        _animator.runtimeAnimatorController = controller;
    }

   
}
