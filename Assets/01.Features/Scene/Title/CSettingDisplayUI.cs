using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CSettingDisplayUI : MonoBehaviour
{
    [SerializeField] private CLocalizationManager _localization;
    [SerializeField] private CDataManger _data;

    [Header("변경 이미지")]
    [SerializeField] private Sprite _changeSprite;
    [SerializeField] private Button _approveButton;
    [SerializeField] private TextMeshProUGUI _approveText;

    [SerializeField] private CSettingButtonSlot _displayMode;
    [SerializeField] private CSettingButtonSlot _resolution;

    [Header("선택 가능한 해상도")]
    [SerializeField]
    private Vector2Int[] _resolutions =
    {
        new Vector2Int(640, 480),
        new Vector2Int(1280, 720),
        new Vector2Int(1600, 900),
        new Vector2Int(1920, 1080)
    };

    private CSettings _settings;

    private int _resolutionIndex;

    private void Awake()
    {
        SetSlotButton(_displayMode.Slot);
        SetSlotButton(_resolution.Slot);
    }

    private void OnEnable()
    {
        _settings = _data.CurrentSetting;

        InitializeSelection();

        _localization.OnLanguageChanged += ReloadText;
        ReloadText();

        _displayMode.ButtonUp.onClick.AddListener(DisplayModeUp);
        _displayMode.ButtonDown.onClick.AddListener(DisplayModeDown);

        _resolution.ButtonUp.onClick.AddListener(ResolutionUp);
        _resolution.ButtonDown.onClick.AddListener(ResolutionDown);

        _approveButton.onClick.AddListener(ApproveDisplay);
    }

    private void OnDisable()
    {
        _localization.OnLanguageChanged -= ReloadText;

        _displayMode.ButtonUp.onClick.RemoveListener(DisplayModeUp);
        _displayMode.ButtonDown.onClick.RemoveListener(DisplayModeDown);

        _resolution.ButtonUp.onClick.RemoveListener(ResolutionUp);
        _resolution.ButtonDown.onClick.RemoveListener(ResolutionDown);

        _approveButton.onClick.RemoveListener(ApproveDisplay);
    }

    // 현재 설정에 해당하는 해상도 선택
    private void InitializeSelection()
    {
        Vector2Int currentResolution = new Vector2Int(_settings.Width, _settings.Height);
        _resolutionIndex = Array.IndexOf(_resolutions, currentResolution);
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

    // 언어 변경 및 초기 텍스트 설정
    private void ReloadText()
    {
        _displayMode.TitleText.text = _localization.GetText("scene.optiondisplaymodetitle.text");
        _resolution.TitleText.text = _localization.GetText("scene.optionresolutiontitle.text");
        _approveText.text = _localization.GetText("scene.optionapprove.text");

        _resolution.ContentsText.text = $"{_settings.Width} × {_settings.Height}";

        switch (_settings.ScreenMode)
        {
            case FullScreenMode.Windowed:
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.windowed.text");
                break;

            case FullScreenMode.FullScreenWindow:
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.borderless.text");
                break;

            case FullScreenMode.ExclusiveFullScreen:
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.exclusive.text");
                break;

            case FullScreenMode.MaximizedWindow:
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.maximized.text");
                break;
        }
    }

    private void ResolutionUp()
    {
        // 인덱스 순회
        _resolutionIndex = (_resolutionIndex + 1) % _resolutions.Length;

        ChangeResolution();
    }

    private void ResolutionDown()
    {
        // 인덱스 순회
        _resolutionIndex = (_resolutions.Length + _resolutionIndex - 1) % _resolutions.Length;

        ChangeResolution();
    }

    // 선택한 해상도를 설정 데이터에 기록
    private void ChangeResolution()
    {
        Vector2Int resolution = _resolutions[_resolutionIndex];

        _settings.SetDisplay(resolution.x, resolution.y, _settings.ScreenMode);

        _resolution.ContentsText.text = $"{_settings.Width} × {_settings.Height}";
    }

    private void DisplayModeUp()
    {
        FullScreenMode mode;

        switch (_settings.ScreenMode)
        {
            case FullScreenMode.Windowed:
                mode = FullScreenMode.FullScreenWindow;
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.borderless.text");
                break;

            case FullScreenMode.FullScreenWindow:
                mode = FullScreenMode.ExclusiveFullScreen;
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.exclusive.text");
                break;

            case FullScreenMode.ExclusiveFullScreen:
                mode = FullScreenMode.MaximizedWindow;
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.maximized.text");
                break;

            case FullScreenMode.MaximizedWindow:
                mode = FullScreenMode.Windowed;
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.windowed.text");
                break;

            default:
                return;
        }

        _settings.SetDisplay(_settings.Width, _settings.Height, mode);
    }

    private void DisplayModeDown()
    {
        FullScreenMode mode;

        switch (_settings.ScreenMode)
        {
            case FullScreenMode.Windowed:
                mode = FullScreenMode.MaximizedWindow;
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.maximized.text");
                break;
            case FullScreenMode.FullScreenWindow:
                mode = FullScreenMode.Windowed;
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.windowed.text");
                break;

            case FullScreenMode.ExclusiveFullScreen:
                mode = FullScreenMode.FullScreenWindow;
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.borderless.text");
                break;

            case FullScreenMode.MaximizedWindow:
                mode = FullScreenMode.ExclusiveFullScreen;
                _displayMode.ContentsText.text = _localization.GetText("scene.displaymode.exclusive.text");
                break;

            default:
                return;
        }

        _settings.SetDisplay(_settings.Width, _settings.Height, mode);
    }


    // 기록해 둔 설정값으로 실제 화면 변경
    private void ApproveDisplay()
    {
        _settings.SetDisplay();
    }
}