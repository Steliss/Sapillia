using System;
using UnityEngine;

public enum Costume
{
    Pink,
    Armored,
    White
    
}


public class CCostumeManager : MonoBehaviour
{

    #region 기본 코스튬
    [Header("Costume Data")]
    [SerializeField] private CCostumeDataSO _defaultCostume;    
    [SerializeField] private CCostumeDataSO _pinkCostume;
    [SerializeField] private CCostumeDataSO _armoredCostume;
    [SerializeField] private CCostumeDataSO _whiteCostume;
    #endregion

    #region 
    private CCostumeDataSO _currentCostume;

    #endregion

    #region 현재코스튬
    public CCostumeDataSO CurrentCostume => _currentCostume;

    #endregion

    #region 이벤트
    public event Action<CCostumeDataSO> OnCostumeChanged;

    #endregion
    #region 코스튬 변경
    // 열거체로 코스튬 선택
    public bool SelectCostume(Costume type)
    {       
        switch (type)
        {
            case Costume.White:
                return SelectCostume(_whiteCostume);

            case Costume.Pink:
                return SelectCostume(_pinkCostume);

            case Costume.Armored:
                return SelectCostume(_armoredCostume);
            
        }

        return SelectCostume(_pinkCostume);
    }

    // 코스튬 변경
    public bool SelectCostume(CCostumeDataSO costume)
    {
        if (costume == null)
        {
            CLog.Warn("[CostumeManager] 코스튬 데이터 없음");
            return false;
        }

        // 이미 선택한 코스튬
        if (_currentCostume == costume)
        {
            return true;
        }

        // 코스튬 변경
        _currentCostume = costume;

        // 변경 이벤트 호출
        OnCostumeChanged?.Invoke(_currentCostume);

        CLog.Log($"[CostumeManager] 코스튬 변경 : " + $"{_currentCostume.CostumeName}");

        return true;
    }

    /// <summary>
    /// UI 버튼에 연결할 함수
    /// </summary>

    // 버튼 연결 → Basic 코스튬으로 변경
    public void SelectBasic()
    {
        SelectCostume(Costume.Pink);
    }
    // 버튼 연결 → Armored 코스튬으로 변경
    public void SelectArmored()
    {
        SelectCostume(Costume.Armored);        
    }
    // 버튼 연결 → White 코스튬으로 변경
    public void SelectWhite()
    {
        SelectCostume(Costume.White);
    }



    #endregion


    #region Unity
    private void Awake()
    {
        _currentCostume = _defaultCostume;
    }

    private void Start()
    {
        // 초기 코스튬 변경 알림
        if (_currentCostume != null)
        {
            OnCostumeChanged?.Invoke(_currentCostume);
        }
    }


    #endregion



}
