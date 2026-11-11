
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CWeaponSelectUI : MonoBehaviour
{
    #region 인스펙터

    [Header("무기 매니저")]
    [SerializeField] private CWeaponManager _weaponManager;
    [SerializeField] private CPlayerController _playerController;

    [Header("UI")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private Button _closeButton;

    [Header("무기 강화 항목")]
    [SerializeField] private CWeaponOptionUI[] _options;

    [Header("확인 팝업")]
    [SerializeField] private GameObject _confirmRoot;
    [SerializeField] private TMP_Text _confirmText;
    [SerializeField] private Button _yesButton;
    [SerializeField] private Button _noButton;

    [Header("강화 후 UI 닫기")]
    [SerializeField] private bool _closeOnUpgrade = true;

    #endregion

    #region 내부변수
    private bool _isOpen;
    private readonly List<CWeaponUpgradeOptionSO> _cachedOptions = new List<CWeaponUpgradeOptionSO>();
    private CWeaponUpgradeOptionSO _selectedOption;
    #endregion

    #region 프로퍼티
    public bool IsOpen => _isOpen;

    #endregion

    #region Unity
    private void Awake()
    {
        // 버튼마다 UI 관리자 연결
        if (_options != null)
        {
            foreach (CWeaponOptionUI option in _options)
            {
                if (option != null)
                {
                    option.Initialize(this);
                    option.SetVisible(false);
                }
            }
        }

        // 처음에는 UI 숨기기
        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }

        if (_confirmRoot != null)
        {
            _confirmRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (_weaponManager != null)
        {
            _weaponManager.OnWeaponChanged += HandleWeaponChanged;
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Close);
        }

        if (_yesButton != null)
        {
            _yesButton.onClick.AddListener(ConfirmUpgrade);
        }

        if (_noButton != null)
        {
            _noButton.onClick.AddListener(CloseConfirm);
        }
        
    }

    private void OnDisable()
    {
        if (_weaponManager != null)
        {
            _weaponManager.OnWeaponChanged -= HandleWeaponChanged;
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(Close);
        }

        if (_yesButton != null)
        {
            _yesButton.onClick.RemoveListener(ConfirmUpgrade);
        }

        if (_noButton != null)
        {
            _noButton.onClick.RemoveListener(CloseConfirm);
        }

    }

    #endregion

    #region Open / Close
    public void Open()
    {
        if (_panelRoot == null || _weaponManager == null || _playerController == null)
        {
            return;
        }

        // 플레이어 데이터 준비 여부 확인
        if (!TryGetPlayerData(out CPlayerGamingDataSO playerData))
        {
            CLog.Warn("[WeaponUI] PlayerData 준비되지 않음");
            return;
        }

        _isOpen = true;
        _panelRoot.SetActive(true);

        RefreshOptions();

        CLog.Log
            (
                $"[WeaponUI] 강화 UI 열기 / " +
                $"{playerData.WeaponType} Tier " +
                $"{playerData.EquippedWeaponTier}"
             );
    }

    public void Close()
    {
        _isOpen = false;
        _selectedOption = null;

        if (_confirmRoot != null)
        {
            _confirmRoot.SetActive(false);
        }

        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    #endregion

    #region PlayerData
    private bool TryGetPlayerData(out CPlayerGamingDataSO playerData)
    {
        playerData = null;

        if (_playerController == null)
        {
            return false;
        }

        playerData = _playerController.PlayerData;

        return playerData != null && playerData.IsInitialized;
    }

    #endregion

    #region Upgrade
    public void SelectWeapon(EWeaponType type, int tier)
    {
        if (!_isOpen || _weaponManager == null)
        {
            return;
        }

        if (!TryGetPlayerData(out CPlayerGamingDataSO playerData))
        {
            return;
        }

        // 현재 플레이어가 장착 중인 무기
        EWeaponType currentType = playerData.WeaponType;
        int currentTier = playerData.EquippedWeaponTier;

        // 다음 티어 계산
        int nextTier = currentTier + 1;

        // 동일한 무기 종류인지 검사
        if (type != currentType)
        {
            CLog.Warn("[WeaponUI] 다른 종류의 무기는 강화 불가능");
            return;
        }

        // 정확히 다음 티어인지 검사
        if (tier != nextTier)
        {
            CLog.Warn("[WeaponUI] 다음 티어만 강화 가능");
            return;
        }

        // 다음 티어 무기 존재 여부
        if (!_weaponManager.HasWeapon(type, tier))
        {
            CLog.Warn("[WeaponUI] 강화할 무기 데이터 없음");
            return;
        }

        // 기존 무기 교체 시스템 활용
        bool success = _weaponManager.TrySwapWeapon(type, tier);

        if (!success)
        {
            CLog.Warn($"[WeaponUI] 강화 실패 : {type} Tier {tier}");
            return;
        }

        CLog.Log($"[WeaponUI] 강화 완료 : {type} Tier {tier}");

        // 강화 성공 후 처리
        if (_closeOnUpgrade)
        {
            Close();
        }
        else
        {
            RefreshOptions();
        }
    }

    private void HandleWeaponChanged(EWeaponType type, int tier)
    {
        if (!_isOpen)
        {
            return;
        }

        RefreshOptions();
    }

    private void RefreshOptions()
    {
        // 모든 무기 슬롯 먼저 숨기기
        HideAllOptions();

        if (_weaponManager == null || !TryGetPlayerData(out CPlayerGamingDataSO playerData))
        {
            return;
        }

        EWeaponType currentType = playerData.WeaponType;
        int currentTier = playerData.EquippedWeaponTier;

        _weaponManager.GetAvailableUpgradeOptions(currentType, currentTier, _cachedOptions);

        // 전부 숨기기
        if (_options != null)
        {
            foreach (CWeaponOptionUI optionUI in _options)
            {
                if (optionUI != null)
                {
                    optionUI.SetVisible(false);
                }
            }
        }

        // 후보 채우기
        for (int i = 0; i < _options.Length; i++)
        {
            if (_options[i] == null)
            {
                continue;
            }

            if (i >= _cachedOptions.Count)
            {
                _options[i].SetVisible(false);
                continue;
            }

            _options[i].Bind(_cachedOptions[i]);
        }

        CLog.Log($"[WeaponUI] 표시 후보 수 : {_cachedOptions.Count}");
    }


    private void HideAllOptions()
    {
        if (_options != null)
        {
            foreach (CWeaponOptionUI option in _options)
            {
                if (option != null)
                {
                    option.SetVisible(false);
                }
            }
        }
        
    }
    #endregion

    #region 확인창
    public void OpenConfirm(CWeaponUpgradeOptionSO option)
    {
        if (option == null || _confirmRoot == null)
        {
            return;
        }

        _selectedOption = option;
        _confirmRoot.SetActive(true);

        if (_confirmText != null)
        {
            _confirmText.text = $"{option.DisplayName}로 강화하시겠습니까?";
        }
    }

    public void CloseConfirm()
    {
        _selectedOption = null;

        if (_confirmRoot != null)
        {
            _confirmRoot.SetActive(false);
        }
    }

    private void ConfirmUpgrade()
    {
        if (_selectedOption == null || _weaponManager == null)
        {
            return;
        }

        bool success = _weaponManager.TryApplyUpgrade(_selectedOption);

        if (!success)
        {
            CLog.Warn("[WeaponUI] 강화 실패");
            return;
        }

        CLog.Log($"[WeaponUI] 강화 성공 : {_selectedOption.DisplayName}");

        CloseConfirm();

        if (_closeOnUpgrade)
        {
            Close();
        }
        else
        {
            RefreshOptions();
        }
    }

    #endregion
}
