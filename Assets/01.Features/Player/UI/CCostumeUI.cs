using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 테스트용도 UI
public class CCostumeUI : MonoBehaviour
{
    #region 인스펙터

    [SerializeField] private CCostumeManager _costumeManager;
    [SerializeField] private CPlayerCostume _playerCostume;
    [SerializeField] private GameObject _costumePanel;

    #endregion

    private void Awake()
    {
        if (_costumeManager == null)
        {
            _costumeManager = FindFirstObjectByType<CCostumeManager>();
        }

        if (_costumePanel != null)
        {
            _costumePanel.SetActive(false);
        }
    }



    private void OnEnable()    
    {
        if( _costumeManager == null)
        {
            CLog.Error("CostumeManager 누락");
            enabled = false;
            return;
        }

        _costumeManager.OnCostumeChanged += HandleCostumeChanged;
    }

    private void OnDisable()
    {
        if (_costumeManager != null)
        {
            _costumeManager.OnCostumeChanged -= HandleCostumeChanged;
        }
    }
    

    public void Open()
    {
        if (_costumePanel != null)
        {
            _costumePanel.SetActive(true);
        }
    }

    public void Close()
    {
        if (_costumePanel != null)
        {
            _costumePanel.SetActive(false);
        }
    }

    public void SelectCostume(CCostumeDataSO costume)
    {
        if (_costumePanel == null || !_costumePanel.activeInHierarchy)
        {
            return;
        }

        if (_costumeManager == null)
        {
            return;
        }

        _costumeManager.SelectCostume(costume);
    }

    private void HandleCostumeChanged(CCostumeDataSO costume)
    {
        if (_playerCostume != null)
        {
            _playerCostume.ApplyCostume(costume);
        }
    }
}
