using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CWeaponOptionUI : MonoBehaviour
{
    #region 인스펙터     

    [Header("UI")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _iconImage;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private TMP_Text _descriptionText;

    [Header("석궁 전용 능력치")]
    [SerializeField] private TMP_Text _statText;

    #endregion

    #region 내부변수
    private CWeaponSelectUI _owner;
    private CWeaponUpgradeOptionSO _option;

    #endregion
        
    public void SetVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }

    #region Unity
    private void OnEnable()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(HandleClick);
        }
    }

    private void OnDisable()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(HandleClick);
        }
    }
    #endregion

    #region Initialize

    public void Initialize(CWeaponSelectUI owner)
    {
        _owner = owner;      
    }

    #endregion

    public void Bind(CWeaponUpgradeOptionSO option)
    {
        _option = option;

        if (_option == null)
        {
            SetVisible(false);
            return;
        }
        
        if (_iconImage != null)
        {
            _iconImage.sprite = _option.Icon;
            _iconImage.enabled = _option.Icon != null;
        }

        if (_nameText != null)
        {
            _nameText.text = _option.DisplayName;
        }

        if (_descriptionText != null)
        {
            _descriptionText.text = _option.Description;
        }

        // 석궁일 때만 상세 능력치 표시
        bool showStats = option.WeaponType == EWeaponType.Crossbow && option.CrossbowData != null;

        if (_statText != null)
        {
            _statText.gameObject.SetActive(showStats);

            if (showStats)
            {
                _statText.text = BuildCrossbowText(_option.CrossbowData);
            }
        }

        SetVisible(true);
    }  

    private string BuildCrossbowText(CCrossbowDataSO data)
    {
        if (data == null)
        {
            return string.Empty;
        }

        return
            $"탄창 용량         {data.AmmoCap}\n" +
            $"탄창 개수         {data.AmmoCount}\n" +
            $"발사 속도         {data.AttackInterval:F2}s\n" +
            $"재장전            {data.ReloadDuration}";
    }

    #region Select
    private void HandleClick()
    {
        if (_owner == null || _option == null)
        {
            return;
        }

        // 클릭하면 바로 장착하지 않고 확인 팝업 열기
        _owner.OpenConfirm(_option);
    }



    #endregion
}
