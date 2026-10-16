using System;
using UnityEngine;


public class CCostumeManager : MonoBehaviour
{

    #region 기본 코스튬
    [Header("Default Costume")]
    [SerializeField] private CCostumeDataSO _defaultCostume;
    #endregion

    #region 
    private CCostumeDataSO _currentCostume;

    public event Action<CCostumeDataSO> OnCostumeChanged;
    #endregion

    #region 현재코스튬
    public CCostumeDataSO CurrentCostume => _currentCostume;


    #endregion

    // 코스튬 변경
    public bool SelectCostume(CCostumeDataSO costume)
    {
        if(costume == null)
        {
            return false;
        }

        if(_currentCostume == costume)
        {
            return true;
        }
                
        _currentCostume = costume;
        OnCostumeChanged?.Invoke(_currentCostume);

        return true;
    }

    private void Awake()
    {
        _currentCostume = _defaultCostume;
    }
}
