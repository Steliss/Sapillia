using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class CSettingSoundSlot
{
    [SerializeField] private GameObject _slot;
    [SerializeField] private Slider _slider;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _valueText;

    public GameObject Slot => _slot;
    public Slider Slider => _slider;
    public TextMeshProUGUI TitleText => _titleText;
    public TextMeshProUGUI ValueText => _valueText;
}


public class CSettingSoundUI : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CSoundManager _sound;
    [SerializeField] private CDataManger _data;
    [SerializeField] private CLocalizationManager _localization;

    [Header("이미지")]
    [SerializeField] private Sprite _changeSprite;

    [Header("볼륨 슬롯")]
    [SerializeField] private CSettingSoundSlot _masterSlider;
    [SerializeField] private CSettingSoundSlot _bgmSlider;
    [SerializeField] private CSettingSoundSlot _effectSlider;
    [SerializeField] private CSettingSoundSlot _environmentSlider;


    private void Awake()
    {
        InitializeSlot(_masterSlider);
        InitializeSlot(_bgmSlider);
        InitializeSlot(_effectSlider);
        InitializeSlot(_environmentSlider);
    }

    private void OnEnable()
    {
        ReloadData();
        ReloadText();

        _masterSlider.Slider.onValueChanged.AddListener(ChangeMaster);
        _bgmSlider.Slider.onValueChanged.AddListener(ChangeBGM);
        _effectSlider.Slider.onValueChanged.AddListener(ChangeEffect);
        _environmentSlider.Slider.onValueChanged.AddListener(ChangeEnvironment);
    }

    private void OnDisable()
    {
        _masterSlider.Slider.onValueChanged.RemoveListener(ChangeMaster);
        _bgmSlider.Slider.onValueChanged.RemoveListener(ChangeBGM);
        _effectSlider.Slider.onValueChanged.RemoveListener(ChangeEffect);
        _environmentSlider.Slider.onValueChanged.RemoveListener(ChangeEnvironment);
    }

    // 슬롯 이미지와 슬라이더 범위 설정
    private void InitializeSlot(CSettingSoundSlot slot)
    {
        SetSlotButton(slot.Slot);

        slot.Slider.wholeNumbers = false;
        slot.Slider.minValue = 0f;
        slot.Slider.maxValue = 1f;
    }

    private void ReloadText()
    {
        _masterSlider.TitleText.text = _localization.GetText("scene.mastersound.text");
        _bgmSlider.TitleText.text = _localization.GetText("scene.bgmsound.text");
        _effectSlider.TitleText.text = _localization.GetText("scene.effectsound.text");
        _environmentSlider.TitleText.text = _localization.GetText("scene.environmentsound.text");
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

    // 저장된 데이터 가져와서 표시
    public void ReloadData()
    {
        CSettings setting = _data.CurrentSetting;

        SetSlotValue(_masterSlider, setting.MasterSound);
        SetSlotValue(_bgmSlider, setting.BGMSound);
        SetSlotValue(_effectSlider, setting.EffectSound);
        SetSlotValue(_environmentSlider, setting.EnvironmentSound);
    }

    // 사운드 설정
    private void SetSlotValue(CSettingSoundSlot slot, float value)
    {
        value = Mathf.Clamp01(value);

        slot.Slider.SetValueWithoutNotify(value);
        RefreshValueText(slot, value);
    }

    // % 표시
    private void RefreshValueText(CSettingSoundSlot slot, float value)
    {
        slot.ValueText.text = $"{Mathf.RoundToInt(value * 100f)}%";
    }

    private void ChangeMaster(float value)
    {
        value = Mathf.Clamp01(value);

        _data.CurrentSetting.SetMasterSound(value);
        RefreshValueText(_masterSlider, value);
        ApplyVolume();
    }

    private void ChangeBGM(float value)
    {
        value = Mathf.Clamp01(value);

        _data.CurrentSetting.SetBGMSound(value);
        RefreshValueText(_bgmSlider, value);
        ApplyVolume();
    }

    private void ChangeEffect(float value)
    {
        value = Mathf.Clamp01(value);

        _data.CurrentSetting.SetEffectSound(value);
        RefreshValueText(_effectSlider, value);
        ApplyVolume();
    }

    private void ChangeEnvironment(float value)
    {
        value = Mathf.Clamp01(value);

        _data.CurrentSetting.SetEnvironmentSound(value);
        RefreshValueText(_environmentSlider, value);
        ApplyVolume();
    }

    // 설정값을 실제 소리에 즉시 적용
    private void ApplyVolume()
    {
        _sound.ApplyVolume(_data.CurrentSetting);
        SaveSettings();
    }

    // 설정 저장 // 변경시마다 저장하는데 저장 요소는 따로 옮길지에 대한 생각 필요 
    public void SaveSettings()
    {
        if (_data == null)
        {
            return;
        }

        _data.SaveSettings();
    }
}