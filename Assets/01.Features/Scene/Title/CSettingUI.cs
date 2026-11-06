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

    [Header("옵션 슬롯")]
    [SerializeField] private TextMeshProUGUI _gamePlaySlotButtonText;
    [SerializeField] private Button _gamePlaySlotButton;
    [SerializeField] private GameObject _gamePlayTap;
    [Space(6)]
    [SerializeField] private TextMeshProUGUI _soundSlotButtonText;
    [SerializeField] private Button _soundSlotButton;
    [SerializeField] private GameObject _soundSlotTap;
    [Space(6)]
    [SerializeField] private TextMeshProUGUI _displaySlotButtonText;
    [SerializeField] private Button _displaySlotButton;
    [SerializeField] private GameObject _displayTap;
    [Space(6)]
    [SerializeField] private TextMeshProUGUI _keySettingSlotButtonText;
    [SerializeField] private Button _keySettingsSlotButton;
    [SerializeField] private GameObject _keySettingTap;

    private bool _isActive = false;

    public bool IsActive => _isActive;

    private void OnEnable()
    {
        _localization.OnLanguageChanged += ReloadText;

        _gamePlaySlotButton.onClick.AddListener(GamePlaySlotOpen);
        _soundSlotButton.onClick.AddListener(SoundSlotOpen);
        _displaySlotButton.onClick.AddListener(DisplaySlotOpen);
        _keySettingsSlotButton.onClick.AddListener(KeySettingSlotOpen);
    }

    private void OnDisable()
    {
        _localization.OnLanguageChanged -= ReloadText;

        _gamePlaySlotButton.onClick.RemoveListener(GamePlaySlotOpen);
        _soundSlotButton.onClick.RemoveListener(SoundSlotOpen);
        _displaySlotButton.onClick.RemoveListener(DisplaySlotOpen);
        _keySettingsSlotButton.onClick.RemoveListener(KeySettingSlotOpen);
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

    }

    private void Start()
    {
        ReloadText();
    }

    /// <summary>
    ///  설정 UI 캔버스 열기
    /// </summary>
    /// <param name="con"></param>
    public void ActiveUI(bool con = true)
    {
        _isActive = con;
        _UICanvas.SetActive(con);
    }


    //언어 변환 및 초기 언어 세팅 관련
    private void ReloadText()
    {
        _gamePlaySlotButtonText.text = _localization.GetText("scene.gameplayslotbutton.text");
        _soundSlotButtonText.text = _localization.GetText("scene.soundslotbutton.text");
        _displaySlotButtonText.text = _localization.GetText("scene.displayslotbutton.text");
        _keySettingSlotButtonText.text = _localization.GetText("scene.keysettingslotbutton.text");

    }

    private void GamePlaySlotOpen()
    {
        _gamePlayTap.transform.SetAsLastSibling();

        _gamePlaySlotButtonText.transform.SetAsLastSibling();
        _soundSlotButtonText.transform.SetAsLastSibling();
        _displaySlotButtonText.transform.SetAsLastSibling();
        _keySettingSlotButtonText.transform.SetAsLastSibling();
    }

    private void SoundSlotOpen()
    {
        _soundSlotTap.transform.SetAsLastSibling();

        _gamePlaySlotButtonText.transform.SetAsLastSibling();
        _soundSlotButtonText.transform.SetAsLastSibling();
        _displaySlotButtonText.transform.SetAsLastSibling();
        _keySettingSlotButtonText.transform.SetAsLastSibling();
    }

    private void DisplaySlotOpen()
    {
        _displayTap.transform.SetAsLastSibling();

        _gamePlaySlotButtonText.transform.SetAsLastSibling();
        _soundSlotButtonText.transform.SetAsLastSibling();
        _displaySlotButtonText.transform.SetAsLastSibling();
        _keySettingSlotButtonText.transform.SetAsLastSibling();
    }

    private void KeySettingSlotOpen()
    {
        _keySettingTap.transform.SetAsLastSibling();

        _gamePlaySlotButtonText.transform.SetAsLastSibling();
        _soundSlotButtonText.transform.SetAsLastSibling();
        _displaySlotButtonText.transform.SetAsLastSibling();
        _keySettingSlotButtonText.transform.SetAsLastSibling();
    }



}