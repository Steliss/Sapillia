using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class CCostumeSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region 인스펙터
    [Header("코스튬 데이터")]
    [SerializeField] private CCostumeDataSO _costume;

    [Header("UI")]
    [SerializeField] private Button _button;
    [SerializeField] private Image _icon;
    [SerializeField] private TMP_Text _nameText;

    [Header("강조 표시")]
    [SerializeField] private Image _hoverBackground;
    [SerializeField] private Image _selectedFrame;

    #endregion

    #region 내부변수
    private CCostumeSelectUI _owner;
    private bool _isHovered;

    #endregion

    #region 프로퍼티
    public CCostumeDataSO CostumeData => _costume;

    #endregion

    #region Unity
    private void OnEnable()
    {
        if (_button != null)
        {
            _button.onClick.AddListener(HandleClick);
        }

        _isHovered = false;

        if (_hoverBackground != null)
        {
            _hoverBackground.enabled = false;
        }

        RefreshVisual(_owner != null ? _owner.CurrentCostume : null);
    }

    private void OnDisable()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(HandleClick);
        }

        _isHovered = false;
    }

    #endregion

    #region Initialize
    public void Initialize(CCostumeSelectUI owner)
    {
        _owner = owner;

        if (_costume == null)
        {
            return;
        }

        if (_icon != null)
        {
            _icon.sprite = _costume.Icon;
        }

        if (_nameText != null)
        {
            _nameText.text = _costume.CostumeName;
        }
    }

    #endregion

    #region Pointer
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (_costume == null)
        {
            return;
        }

        _isHovered = true;

        if (_hoverBackground != null)
        {
            _hoverBackground.enabled = true;
        }

        // 오른쪽 상세정보 미리보기
        _owner?.PreviewCostume(_costume);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _isHovered = false;

        if (_hoverBackground != null)
        {
            _hoverBackground.enabled = false;
        }

        _owner?.EndPreview(_costume);
    }

    #endregion

    #region Select
    private void HandleClick()
    {
        if (_costume == null)
        {
            return;
        }

        _owner?.SelectCostume(_costume);
    }

    public void RefreshVisual(CCostumeDataSO selected)
    {
        if (_selectedFrame != null)
        {
            _selectedFrame.enabled = _costume == selected;
        }

        if (_hoverBackground != null)
        {
            _hoverBackground.enabled = _isHovered;
        }
    }

    #endregion
}
