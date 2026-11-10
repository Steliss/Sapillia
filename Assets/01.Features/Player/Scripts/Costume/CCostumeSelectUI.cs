using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CCostumeSelectUI : MonoBehaviour
{
    #region 인스펙터
    [Header("참조")]
    [SerializeField] private CCostumeManager _costumeManager;

    [Header("UI 패널")]
    [SerializeField] private GameObject _panelRoot;
    [SerializeField] private Button _closeButton;

    [Header("코스튬 목록")]
    [SerializeField] private CCostumeSlotUI[] _slots;

    [Header("상세정보 UI")]
    [SerializeField] private Image _detailIcon;
    [SerializeField] private TMP_Text _detailName;
    [SerializeField] private TMP_Text _detailDescription;
    [SerializeField] private TMP_Text _detailBonus;
    [SerializeField] private GameObject _equippedLabel;

    #endregion

    #region 내부변수
    private bool _isOpen;
    private CCostumeDataSO _hoveredCostume;

    #endregion

    #region 프로퍼티
    public bool IsOpen => _isOpen;
    public CCostumeDataSO CurrentCostume => _costumeManager != null ? _costumeManager.CurrentCostume : null;

    #endregion

    #region Unity

    private void Awake()
    {
        // 슬롯에 UI 관리자 참조 전달
        if (_slots != null)
        {
            foreach (CCostumeSlotUI slot in _slots)
            {
                if (slot != null)
                {
                    slot.Initialize(this);
                }
            }
        }

        // 시작 시 UI 닫기
        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (_costumeManager != null)
        {
            _costumeManager.OnCostumeChanged += HandleCostumeChanged;
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.AddListener(Close);
        }
    }

    private void OnDisable()
    {
        if (_costumeManager != null)
        {
            _costumeManager.OnCostumeChanged -= HandleCostumeChanged;
        }

        if (_closeButton != null)
        {
            _closeButton.onClick.RemoveListener(Close);
        }
    }

    #endregion

    #region Open / Close

    public void Open()
    {
        if (_isOpen || _panelRoot == null || _costumeManager == null)
        {
            return;
        }

        _isOpen = true;
        _hoveredCostume = null;

        _panelRoot.SetActive(true);

        RefreshSlots();

        // 현재 장착된 코스튬 표시
        ShowDetails(_costumeManager.CurrentCostume);
    }

    public void Close()
    {
        if (!_isOpen)
        {
            return;
        }

        _isOpen = false;
        _hoveredCostume = null;

        if (_panelRoot != null)
        {
            _panelRoot.SetActive(false);
        }
    }

    #endregion

    #region Costume Preview

    // 마우스를 올리면 미리보기
    public void PreviewCostume(CCostumeDataSO costume)
    {
        if (!_isOpen || costume == null)
        {
            return;
        }

        _hoveredCostume = costume;

        ShowDetails(costume);
    }

    // 마우스가 벗어나면 현재 장착 코스튬 표시
    public void EndPreview(CCostumeDataSO costume)
    {
        if (!_isOpen || _hoveredCostume != costume)
        {
            return;
        }

        _hoveredCostume = null;

        ShowDetails(_costumeManager.CurrentCostume);
    }

    #endregion

    #region Costume Select

    public void SelectCostume(CCostumeDataSO costume)
    {
        if (!_isOpen || _costumeManager == null || costume == null)
        {
            return;
        }

        _costumeManager.SelectCostume(costume);
    }

    private void HandleCostumeChanged(CCostumeDataSO costume)
    {
        RefreshSlots();

        // 호버 중이면 미리보기 유지
        CCostumeDataSO displayed = _hoveredCostume != null ? _hoveredCostume : costume;

        if (_isOpen)
        {
            ShowDetails(displayed);
        }
    }

    private void RefreshSlots()
    {
        if (_slots == null)
        {
            return;
        }

        foreach (CCostumeSlotUI slot in _slots)
        {
            if (slot != null)
            {
                slot.RefreshVisual(CurrentCostume);
            }
        }
    }

    #endregion

    #region Costume Details

    private void ShowDetails(CCostumeDataSO costume)
    {
        if (costume == null)
        {
            return;
        }

        if (_detailIcon != null)
        {
            _detailIcon.sprite = costume.Icon;
            _detailIcon.enabled = costume.Icon != null;
        }

        if (_detailName != null)
        {
            _detailName.text = costume.CostumeName;
        }

        if (_detailDescription != null)
        {
            _detailDescription.text = costume.Description;
        }

        if (_detailBonus != null)
        {
            _detailBonus.text = BuildBonusText(costume.Bonus);
        }

        // 현재 실제로 장착한 코스튬인지 표시
        if (_equippedLabel != null)
        {
            _equippedLabel.SetActive(costume == CurrentCostume);
        }
    }

    private string BuildBonusText(SCostumeStatBonus bonus)
    {
        StringBuilder sb = new StringBuilder();

        AppendBonus(sb, "방어력", bonus.def);
        AppendBonus(sb, "물리 속성", bonus.Physical);
        AppendBonus(sb, "화염 속성", bonus.Fire);
        AppendBonus(sb, "얼음 속성", bonus.Ice);
        AppendBonus(sb, "번개 속성", bonus.Lightning);
        AppendBonus(sb, "경험치 획득", bonus.EXP, "%");

        return sb.Length > 0 ? sb.ToString().TrimEnd() : "추가 능력치 없음";
    }

    // 수정해야함.
    private void AppendBonus(StringBuilder sb, string statName, float value, string unit = "")
    {
        if (Mathf.Approximately(value, 0f))
        {
            return;
        }

        string sign = value > 0f ? "+" : "";
        string color = value > 0f ? "#70D878" : "#FF6464";

        sb.AppendLine($"<color={color}>{sign}{value:0.##}{unit} " + $"{statName}</color>");
    }

    #endregion
}
