using Cysharp.Threading.Tasks;
using System;
using System.IO;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#region CSaveSlotUI 필드
[Serializable]
public class CSaveSlotUI
{
    [SerializeField] private Button _startGameStartButton;
    [SerializeField] private Image _startButtonBackImage;
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
    public Image StartButtonBackImage
    {
        get => _startButtonBackImage;
        set => _startButtonBackImage = value;
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


public class CTitleUI : MonoBehaviour, IInputCommand
{
    [SerializeField] private CLocalizationManager _localization;
    [SerializeField] private CDataManger _dataManger;

    [Header("메뉴버튼")]
    [SerializeField] private GameObject _startButtonObject;
    [SerializeField] private GameObject _settingButtonObject;
    [SerializeField] private GameObject _discordButtonObject;
    [SerializeField] private GameObject _creditButtonObject;
    [SerializeField] private GameObject _exitButtonObject;

    [Header("게임 시작 UI")]
    [SerializeField] private GameObject _startGameUI;
    [SerializeField] private CSaveSlotUI[] _saveSlots = new CSaveSlotUI[3];

    [Header("설정 UI")]
    [SerializeField] private GameObject _settingUI;


    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _sceneTask = new CUniTaskOnes();
    private CSceneManager _cSceneManager;
    private CInputCommandHub _input;
    private Button _startButton;
    private Button _settingButton;
    private Button _discordButton;
    private Button _creditButton;
    private Button _exitButton;


    private void OnEnable()
    {
        _startButton.onClick.AddListener(OpenCharacterSlot);
        _settingButton.onClick.AddListener(SettingGame);
        _discordButton.onClick.AddListener(DiscordOpen);
        _creditButton.onClick.AddListener(CreditOpen);
        _exitButton.onClick.AddListener(GameExit);

        _saveSlots[0].StartGameStartButton.onClick.AddListener(GameStart);

        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.ESC, this);
            CLog.Log("_inputTask OnEnable 작업 완료");
        });
        _sceneTask.Run(async token =>
        {
            _cSceneManager = await CManagerDontDestroy.SceneManagerAsync(token);
            CLog.Log("_sceneTask OnEnable 작업 완료");
        });

    }

    private void OnDisable()
    {
        _startButton.onClick.RemoveListener(OpenCharacterSlot);
        _settingButton.onClick.RemoveListener(SettingGame);
        _discordButton.onClick.RemoveListener(DiscordOpen);
        _creditButton.onClick.RemoveListener(CreditOpen);
        _exitButton.onClick.RemoveListener(GameExit);

        _inputTask.Cancel();
        _sceneTask.Cancel();

        _input.Unregister(EKeyAction.ESC, this);
    }

    private void Awake()
    {
        // GameObject의 버튼 생상 변경과 기입처리
        _startButton = SetButton(_startButtonObject);
        _settingButton = SetButton(_settingButtonObject);
        _discordButton = SetButton(_discordButtonObject);
        _creditButton = SetButton(_creditButtonObject);
        _exitButton = SetButton(_exitButtonObject);

        _saveSlots[0].StartGameStartButton = SetButton2(_saveSlots[0].StartGameStartButton, _saveSlots[0].StartButtonBackImage);
        _saveSlots[1].StartGameStartButton = SetButton2(_saveSlots[1].StartGameStartButton, _saveSlots[1].StartButtonBackImage);
        _saveSlots[2].StartGameStartButton = SetButton2(_saveSlots[2].StartGameStartButton, _saveSlots[2].StartButtonBackImage);
    }

    private void Start()
    {
        string path = Path.Combine(Application.persistentDataPath, "Game_0.json");
        LoadGame(0, path);
        path = Path.Combine(Application.persistentDataPath, "Game_1.json");
        LoadGame(1, path);
        path = Path.Combine(Application.persistentDataPath, "Game_2.json");
        LoadGame(2, path);
    }

    private void Update()
    {
        if(_cSceneManager == null || _input == null)
        {
            return;
        }

        if(_cSceneManager.IsChanging)
        {
            gameObject.SetActive(false);
        }

        // ※ test
        if (Input.GetKeyDown(KeyCode.M))
        {
            CLog.Log("Test");
            _dataManger.LoadGame(0);
        }

    }


    public void Execute()
    {
        _startGameUI.SetActive(false);
        //CLog.Log("CTitleUI ESC 실행");
    }

    public void LoadGame(int slot, string path)
    {
        if (!File.Exists(path))
        {
            _saveSlots[slot].StartGameStartButtonText.text = _localization.GetText("scene.menustart.text");
            _saveSlots[slot].PlayerNameText.text = _localization.GetText("scene.enmpty.text");
            _saveSlots[slot].PlayerNameSOText.text = _localization.GetText("scene.enmpty.text");
            _saveSlots[slot].PlayerProgressText.text = _localization.GetText("scene.enmpty.text");
            _saveSlots[slot].PlayerProgressSOText.text = _localization.GetText("scene.enmpty.text");
            _saveSlots[slot].PlayerRetryCountText.text = _localization.GetText("scene.enmpty.text");
            _saveSlots[slot].PlayerRetryCountSOText.text = _localization.GetText("scene.enmpty.text");
            _saveSlots[slot].PlayerPlayTimeText.text = _localization.GetText("scene.enmpty.text");
            _saveSlots[slot].PlayerPlayTimeSOText.text = _localization.GetText("scene.enmpty.text");
        }
        else 
        {
            if (_dataManger.LoadGame(0))
            {
                _saveSlots[slot].StartGameStartButtonText.text = _localization.GetText("scene.menustart.text");
                _saveSlots[slot].PlayerNameText.text = _localization.GetText("scene.playername.text");
                _saveSlots[slot].PlayerNameSOText.text = _localization.GetText($"{_dataManger.CurrentFlow.PlayerName}");
                _saveSlots[slot].PlayerProgressText.text = _localization.GetText("scene.playerprogress.text");
                _saveSlots[slot].PlayerProgressSOText.text = _localization.GetText($"{_dataManger.CurrentFlow.PlayerProgress}");
                _saveSlots[slot].PlayerRetryCountText.text = _localization.GetText("scene.playerretrycount.text");
                _saveSlots[slot].PlayerRetryCountSOText.text = _localization.GetText($"{_dataManger.CurrentFlow.PlayerRetrycount}");
                _saveSlots[slot].PlayerPlayTimeText.text = _localization.GetText("scene.playerplaytime.text");
                _saveSlots[slot].PlayerPlayTimeSOText.text = _localization.GetText($"{_dataManger.CurrentFlow.Playerplaytime}");
            }
            else
            {
                CLog.Error("CTitleUI : 파일 불러오기 에러");
            }
        }
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

    // 마우스 이동에 버튼 테두리 변화
    private Button SetButton2(Button button, Image image)
    {
        // 상태에 따라 색상을 바꿀 테두리
        button.targetGraphic = image;
        button.transition = Selectable.Transition.ColorTint;

        Color color = image.color;
        color.a = 1f;
        image.color = color;

        ColorBlock colors = button.colors;
        colors.normalColor = new Color(1f, 1f, 1f, 0f);
        colors.highlightedColor = new Color(1f, 1f, 1f, 1f);
        colors.pressedColor = new Color(1f, 1f, 1f, 1f);
        colors.selectedColor = new Color(1f, 1f, 1f, 0f);

        button.colors = colors;

        return button;
    }

    // 캐릭터 슬롯 열기
    private void OpenCharacterSlot()
    {
        _startGameUI.SetActive(true);
    }



    private void GameStart()
    {





        _cSceneManager.ChangeScreenAsync(EScene.Town).Forget();
        //SceneManager.LoadScene("Town");
    }

    private void SettingGame()
    {

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