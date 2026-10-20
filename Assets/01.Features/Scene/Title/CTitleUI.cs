using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CTitleUI : MonoBehaviour
{
    [Header("버튼")]
    [SerializeField] private GameObject _startButtonObject;
    [SerializeField] private GameObject _settingButtonObject;
    [SerializeField] private GameObject _discordButtonObject;
    [SerializeField] private GameObject _creditButtonObject;
    [SerializeField] private GameObject _exitButtonObject;


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