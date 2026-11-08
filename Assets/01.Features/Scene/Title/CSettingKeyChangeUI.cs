using System;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[Serializable]
public class CSettingKeySlot
{
    [SerializeField] private GameObject _slot;
    [SerializeField] private EKeyAction _action;
    [SerializeField] private TextMeshProUGUI _titleText;

    [Header("기본키")]
    [SerializeField] private Button _primaryButton;
    [SerializeField] private TextMeshProUGUI _primaryText;

    [Header("보조키")]
    [SerializeField] private Button _secondaryButton;
    [SerializeField] private TextMeshProUGUI _secondaryText;

    public EKeyAction Action => _action;
    public TextMeshProUGUI TitleText => _titleText;

    public Button PrimaryButton => _primaryButton;
    public TextMeshProUGUI PrimaryText => _primaryText;

    public Button SecondaryButton => _secondaryButton;
    public TextMeshProUGUI SecondaryText => _secondaryText;

    public GameObject Slot
    {
        get => _slot;
        set => _slot = value;
    }

    public void ClickPrimary()
    {
        OnKeyClicked?.Invoke(_action, EKeySlot.Primary);
    }

    public void ClickSecondary()
    {
        OnKeyClicked?.Invoke(_action, EKeySlot.Secondary);
    }

    public event Action<EKeyAction, EKeySlot> OnKeyClicked;
}

public class CSettingKeyChangeUI : MonoBehaviour
{
    [SerializeField] private CLocalizationManager _localization;
    [SerializeField] private CDataManger _data;
    [SerializeField] private CKeyBind _keyBind;
    [SerializeField] private GameObject _waitObject;
    [SerializeField] private Button _resetButton;

    [SerializeField] private Sprite _changeSprite;
    [SerializeField] private CSettingKeySlot _displayMode;

    [Header("키 설정 행")]
    [SerializeField] private CSettingKeySlot[] _keySlots = new CSettingKeySlot[22];


    private void Awake()
    {
        foreach (CSettingKeySlot slot in _keySlots)
        {
            SetSlotButton(slot.Slot);
        }
    }

    private void OnEnable()
    {

        _waitObject.SetActive(false);
        _keyBind.OnRebindEnded += HandleRebindEnded;

        foreach (CSettingKeySlot slot in _keySlots)
        {
            slot.OnKeyClicked += BeginKeyChange;

            slot.PrimaryButton.onClick.AddListener(slot.ClickPrimary);
            slot.SecondaryButton.onClick.AddListener(slot.ClickSecondary);
        }

        _localization.OnLanguageChanged += ReloadText;

        ReloadText();
        ReloadKeyText();

        _resetButton.onClick.AddListener(ResetSetting);
    }

    private void OnDisable()
    {
        _localization.OnLanguageChanged -= ReloadText;

        _keyBind.OnRebindEnded -= HandleRebindEnded;
        _keyBind.CancelRebind();
        _waitObject.SetActive(false);

        foreach (CSettingKeySlot slot in _keySlots)
        {
            slot.PrimaryButton.onClick.RemoveListener(slot.ClickPrimary);
            slot.SecondaryButton.onClick.RemoveListener(slot.ClickSecondary);

            slot.OnKeyClicked -= BeginKeyChange;
        }


        _resetButton.onClick.RemoveListener(ResetSetting);
    }


    // 슬롯 이미지 교체
    private void SetSlotButton(GameObject slot)
    {
        Button button = slot.GetComponent<Button>();
        Image image = slot.GetComponent<Image>();

        button.targetGraphic = image;
        button.transition = Selectable.Transition.SpriteSwap;

        image.raycastTarget = true;

        SpriteState sprites = button.spriteState;
        sprites.highlightedSprite = _changeSprite;
        sprites.pressedSprite = _changeSprite;
        sprites.selectedSprite = image.sprite;
        sprites.disabledSprite = image.sprite;

        button.spriteState = sprites;

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
    }


    private void ReloadText()
    {
        _keySlots[0].TitleText.text = _localization.GetText("scene.keyslot1.text");
        _keySlots[1].TitleText.text = _localization.GetText("scene.keyslot2.text");
        _keySlots[2].TitleText.text = _localization.GetText("scene.keyslot3.text");
        _keySlots[3].TitleText.text = _localization.GetText("scene.keyslot4.text");
        _keySlots[4].TitleText.text = _localization.GetText("scene.keyslot5.text");
        _keySlots[5].TitleText.text = _localization.GetText("scene.keyslot6.text");
        _keySlots[6].TitleText.text = _localization.GetText("scene.keyslot7.text");
        _keySlots[7].TitleText.text = _localization.GetText("scene.keyslot8.text");
        _keySlots[8].TitleText.text = _localization.GetText("scene.keyslot9.text");
        _keySlots[9].TitleText.text = _localization.GetText("scene.keyslot10.text");
        _keySlots[10].TitleText.text = _localization.GetText("scene.keyslot11.text");
        _keySlots[11].TitleText.text = _localization.GetText("scene.keyslot12.text");
        _keySlots[12].TitleText.text = _localization.GetText("scene.keyslot13.text");
        _keySlots[13].TitleText.text = _localization.GetText("scene.keyslot14.text");
        _keySlots[14].TitleText.text = _localization.GetText("scene.keyslot15.text");
        _keySlots[15].TitleText.text = _localization.GetText("scene.keyslot16.text");
        _keySlots[16].TitleText.text = _localization.GetText("scene.keyslot17.text");
        _keySlots[17].TitleText.text = _localization.GetText("scene.keyslot18.text");
        _keySlots[18].TitleText.text = _localization.GetText("scene.keyslot19.text");
        _keySlots[19].TitleText.text = _localization.GetText("scene.keyslot20.text");
        _keySlots[20].TitleText.text = _localization.GetText("scene.keyslot21.text");
        _keySlots[21].TitleText.text = _localization.GetText("scene.keyslot22.text");
    }

    // 기본키 / 보조키 표시
    private void ReloadKeyText()
    {
        foreach (CSettingKeySlot slot in _keySlots)
        {
            CKeyBindingData binding = _data.CurrentSetting.GetKeyBinding(slot.Action);

            slot.PrimaryText.text = GetText(binding._primary);
            slot.SecondaryText.text = GetText(binding._secondary);
        }
    }

    // 키에 연결된 string 가져오기
    private string GetText(string keyPath)
    {
        if (string.IsNullOrEmpty(keyPath))
        {
            return "—";
        }

        return InputControlPath.ToHumanReadableString(keyPath, InputControlPath.HumanReadableStringOptions.OmitDevice);
    }


    // 클릭한 행의 기본키 / 보조키 변경 시작
    private void BeginKeyChange(EKeyAction action, EKeySlot keySlot)
    {
        if (_keyBind.IsRebinding)
        {
            return;
        }

        _waitObject.SetActive(true);

        try
        {
            _keyBind.RebindKey(action, keySlot == EKeySlot.Primary);
        }
        catch
        {
            _waitObject.SetActive(false);
            throw;
        }
    }

    // 변경 완료 또는 취소 시 호출
    private void HandleRebindEnded()
    {
        _waitObject.SetActive(false);
        ReloadKeyText();
    }

    private void ResetSetting()
    {
        _keyBind.CancelRebind();

        if (!_data.ResetSettings())
        {
            return;
        }

        _keyBind.ApplySavedBindind();
        ReloadKeyText();
    }
}