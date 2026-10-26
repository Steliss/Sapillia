using System;
using TMPro;
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

    [SerializeField] private GameObject _UICanvas;

    [Header("변경 이미지")]
    [SerializeField] private Sprite _changeSprite;

    [Header("옵션 슬롯")]
    [SerializeField] private GameObject _gamePlaySlotButton;
    [SerializeField] private TextMeshProUGUI _gamePlaySlotButtonText;
    [SerializeField] private CSettingButtonSlot _language;
    [SerializeField] private CSettingButtonSlot _bossSkip;
    [SerializeField] private CSettingButtonSlot _cameraShake;
    [SerializeField] private CSettingButtonSlot _effect;



    private void OnEnable()
    {
        _localization.OnLanguageChanged += ReloadText;

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
        _localization.OnLanguageChanged -= ReloadText;

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
        if(_localization ==  null)
        {
            CLog.Log("_localization : Null");
        }

        if (_dataManger == null)
        {
            CLog.Log("_dataManger : Null");
        }

        SetSlotButton(_language.Slot);
        SetSlotButton(_bossSkip.Slot);
        SetSlotButton(_cameraShake.Slot);
        SetSlotButton(_effect.Slot);

    }

    private void Start()
    {
        ReloadText();
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

    /// <summary>
    ///  설정 UI 캔버스 열기
    /// </summary>
    /// <param name="con"></param>
    public void ActiveUI(bool con = true)
    {
        _UICanvas.SetActive(con);
    }



    #region 게임 플레이 옵션 관련

    //언어 변환 및 초기 언어 세팅 관련
    private void ReloadText()
    {
        _gamePlaySlotButtonText.text = _localization.GetText("scene.gameplayslotbutton.text");
        _bossSkip.ContentsText.text = _localization.GetText(
         _dataManger.CurrentSetting.SkipBoss
        ? "scene.optionset.text"
        : "scene.optionoff.text"
        );
        _language.ContentsText.text = _localization.GetText("scene.optionlanguage.text");
    }



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
        _language.ContentsText.text = _localization.GetText("scene.optionlanguage.text");
        ReloadText();
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
        _language.ContentsText.text = _localization.GetText("scene.optionlanguage.text");
        ReloadText();
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