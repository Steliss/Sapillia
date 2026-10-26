using System;
using TMPro;
using UnityEditor.Localization.Editor;
using UnityEngine;
using UnityEngine.UI;


// 옵션 슬롯 한 칸
[Serializable]
public class CSettingButtonSlot
{
    [SerializeField] private GameObject _slot;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _contentsText;
    [SerializeField] private Button _buttonUp;
    [SerializeField] private Button _buttonDown;

    public GameObject Slot
    {
        get => _slot;
        set => _slot = value;
    }
    public TextMeshProUGUI TitleText
    {
        get => _titleText;
        set => _titleText = value;
    }
    public TextMeshProUGUI ContentsText
    {
        get => _contentsText;
        set => _contentsText = value;
    }
    public Button ButtonUp
    {
        get => _buttonUp;
        set => _buttonUp = value;
    }
    public Button ButtonDown
    {
        get => _buttonDown;
        set => _buttonDown = value;
    }

}


public class CSettingUI : MonoBehaviour
{
    [SerializeField] private CLocalizationManager _localization;
    [SerializeField] private CDataManger _dataManger;

    [Header("변경 이미지")]
    [SerializeField] private Sprite _changeSprite;

    [Header("옵션 슬롯")]
    [SerializeField] private CSettingButtonSlot _language;
    [SerializeField] private CSettingButtonSlot _bossSkip;
    [SerializeField] private CSettingButtonSlot _cameraShake;
    [SerializeField] private CSettingButtonSlot _effect;



    private void OnEnable()
    {
        _language.ButtonUp.onClick.AddListener(LanguageUp);
        _language.ButtonDown.onClick.AddListener(LanguageDown);

        _bossSkip.ButtonUp.onClick.AddListener(BossSkip);
        _bossSkip.ButtonDown.onClick.AddListener(BossSkip);

        _cameraShake.ButtonUp.onClick.AddListener(CameraShakeUp);
        _cameraShake.ButtonDown.onClick.AddListener(CameraShakeDown);

        _effect.ButtonUp.onClick.AddListener(EffectUp);
        _effect.ButtonDown.onClick.AddListener(EffectDown);
    }

    private void OnDisable()
    {
        _language.ButtonUp.onClick.RemoveListener(LanguageUp);
        _language.ButtonDown.onClick.RemoveListener(LanguageDown);

        _bossSkip.ButtonUp.onClick.RemoveListener(BossSkip);
        _bossSkip.ButtonDown.onClick.RemoveListener(BossSkip);

        _cameraShake.ButtonUp.onClick.RemoveListener(CameraShakeUp);
        _cameraShake.ButtonDown.onClick.RemoveListener(CameraShakeDown);

        _effect.ButtonUp.onClick.RemoveListener(EffectUp);
        _effect.ButtonDown.onClick.RemoveListener(EffectDown);
    }

    private void Awake()
    {
        SetSlotButton(_language.Slot);
        SetSlotButton(_bossSkip.Slot);
        SetSlotButton(_cameraShake.Slot);
        SetSlotButton(_effect.Slot);

        // 각 텍스트 초기값 넣어야함
    }


    // 슬롯이미지 교체
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

        // 배경 강조용 버튼은 키보드 탐색에서 제외
        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;
    }

    #region 게임 플레이 옵션 관련

    private void LanguageUp()
    {
        int index = (int)_dataManger.CurrentSetting.CurrentLanhuage;
        int count = Enum.GetValues(typeof(ELanguage)).Length;

        index--;

        if (index < 0)
        {
            index = count - 1;
        }

        _localization.ChangeLanguage((ELanguage)index);
        // 텍스트 전부 재 로딩 해야하는데.. 
        CLog.Log($"{_dataManger.CurrentSetting.CurrentLanhuage}");
    }

    private void LanguageDown()
    {
        int index = (int)_dataManger.CurrentSetting.CurrentLanhuage;
        int count = Enum.GetValues(typeof(ELanguage)).Length;

        index++;

        if (index >= count)
        {
            index = 0;
        }

        _localization.ChangeLanguage((ELanguage)index);
        CLog.Log($"{_dataManger.CurrentSetting.CurrentLanhuage}");
    }

    // 보스 연출 스킾 여부 
    private void BossSkip()
    {
        _dataManger.CurrentSetting.SetSkipBoss(!_dataManger.CurrentSetting.SkipBoss); 
        _bossSkip.ContentsText.text = _localization.GetText(
         _dataManger.CurrentSetting.SkipBoss
        ? "scene.optionset.text"
        : "scene.optionoff.text"
        );
    }

    // ※ : TODO : 카메라랑 이펙트 관련 기능이 없어서 일단 넘어감
    private void CameraShakeUp()
    {
    }

    private void CameraShakeDown()
    {
    }

    private void EffectUp()
    {
    }

    private void EffectDown()
    {
    }

    #endregion
}