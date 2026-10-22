using Cysharp.Threading.Tasks;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region CSaveSlotUI 필드
[Serializable]
public class CSaveSlotUI
{
    [SerializeField] private Button _startGameStartButton;
    [SerializeField] private TextMeshProUGUI _startGameStartButtonText;
    [SerializeField] private TextMeshProUGUI _palyerNameText;
    [SerializeField] private TextMeshProUGUI _playerNameSOText;
    [SerializeField] private TextMeshProUGUI _playerProgressText;
    [SerializeField] private TextMeshProUGUI _playerProgressSOText;
    [SerializeField] private TextMeshProUGUI _playerRetryCountText;
    [SerializeField] private TextMeshProUGUI _playerRetryCountSOText;
    [SerializeField] private TextMeshProUGUI _playerPlayTimeText;
    [SerializeField] private TextMeshProUGUI _playerPlayTimeSOText;

    public Button StartGameStartButton
    {
        get => _startGameStartButton;
        set => _startGameStartButton = value;
    }

    public TextMeshProUGUI StartGameStartButtonText
    {
        get => _startGameStartButtonText;
        set => _startGameStartButtonText = value;
    }

    public TextMeshProUGUI PlayerNameText
    {
        get => _palyerNameText;
        set => _palyerNameText = value;
    }

    public TextMeshProUGUI PlayerNameSOText
    {
        get => _playerNameSOText;
        set => _playerNameSOText = value;
    }

    public TextMeshProUGUI PlayerProgressText
    {
        get => _playerProgressText;
        set => _playerProgressText = value;
    }

    public TextMeshProUGUI PlayerProgressSOText
    {
        get => _playerProgressSOText;
        set => _playerProgressSOText = value;
    }

    public TextMeshProUGUI PlayerRetryCountText
    {
        get => _playerRetryCountText;
        set => _playerRetryCountText = value;
    }

    public TextMeshProUGUI PlayerRetryCountSOText
    {
        get => _playerRetryCountSOText;
        set => _playerRetryCountSOText = value;
    }

    public TextMeshProUGUI PlayerPlayTimeText
    {
        get => _playerPlayTimeText;
        set => _playerPlayTimeText = value;
    }

    public TextMeshProUGUI PlayerPlayTimeSOText
    {
        get => _playerPlayTimeSOText;
        set => _playerPlayTimeSOText = value;
    }
}
#endregion


public class CTitleUI : MonoBehaviour
{
    [SerializeField] private CLocalizationManager _localization;

    [Header("메뉴버튼")]
    [SerializeField] private GameObject _startButtonObject;
    [SerializeField] private GameObject _settingButtonObject;
    [SerializeField] private GameObject _discordButtonObject;
    [SerializeField] private GameObject _creditButtonObject;
    [SerializeField] private GameObject _exitButtonObject;

    [Header("게임 시작 UI")]
    [SerializeField] private GameObject _startGameUI;
    [SerializeField] private CSaveSlotUI[] _saveSlots = new CSaveSlotUI[3];


    private CSceneManager _cSceneManager;
    private Button _startButton;
    private Button _settingButton;
    private Button _discordButton;
    private Button _creditButton;
    private Button _exitButton;


    // GameObject의 버튼 생상 변경과 기입처리
    private void Awake()
    {
        _startButton = SetButton(_startButtonObject);
        _settingButton = SetButton(_settingButtonObject);
        _discordButton = SetButton(_discordButtonObject);
        _creditButton = SetButton(_creditButtonObject);
        _exitButton = SetButton(_exitButtonObject);

        // 세이브 슬롯 불러오기 

        // 세이브 슬롯이 없다면 빈 키
        // 있다면 해당 키 

        _saveSlots[0].StartGameStartButtonText.text = _localization.GetText("scene.enmpty.text");
    }

    private void Start()
    {
        _cSceneManager = CManagerDontDestroy.Instance.SceneManager;

        if (_cSceneManager == null)
        {
            // ※ : TODO : 잡아주는 과정 추가 필요.
            CLog.Warn("_cSceneManager : Null");
        }
    }

    private void Update()
    {
        if(_cSceneManager.IsChanging)
        {
            gameObject.SetActive(false);
        }
    }

    private void OnEnable()
    {
        _startButton.onClick.AddListener(GameStart);
        _settingButton.onClick.AddListener(SettingGame);
        _discordButton.onClick.AddListener(DiscordOpen);
        _creditButton.onClick.AddListener(CreditOpen);
        _exitButton.onClick.AddListener(GameExit);
    }

    private void OnDisable()
    {
        _startButton.onClick.RemoveListener(GameStart);
        _settingButton.onClick.RemoveListener(SettingGame);
        _discordButton.onClick.RemoveListener(DiscordOpen);
        _creditButton.onClick.RemoveListener(CreditOpen);
        _exitButton.onClick.RemoveListener(GameExit);
    }

    // 마우스 이동에 따른 글자색 변화
    private Button SetButton(GameObject buttonObject)
    {
        Button button = buttonObject.GetComponent<Button>();
        TextMeshProUGUI text = buttonObject.GetComponent<TextMeshProUGUI>();

        // 버튼의 상태가 변할시 바꿀 대상 지정(텍스트)
        button.targetGraphic = text;

        // 해당 동작의 변경 방식을 선언
        button.transition = Selectable.Transition.ColorTint;
        text.raycastTarget = true;

        // 버튼의 상태별 사용할 색상
        // 설정을 복사해서 전달
        ColorBlock textcolors = button.colors;
        textcolors.normalColor = Color.white;
        textcolors.highlightedColor = Color.yellow;
        textcolors.pressedColor = Color.yellow;
        textcolors.selectedColor = Color.yellow;


        button.colors = textcolors;

        return button;
    }




    private void GameStart()
    {





        _cSceneManager.ChangeScreenAsync(EScene.Town).Forget();
    }

    private void SettingGame()
    {
        // 저장 데이터 불러오기
    }

    private void DiscordOpen()
    {
        // 옵션 UI 열기
    }

    private void CreditOpen()
    {
        // 크레딧 UI 열기
    }

    private void GameExit()
    {
        // 게임 종료
    }


}