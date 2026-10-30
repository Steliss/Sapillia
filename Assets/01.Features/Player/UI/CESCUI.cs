using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CESCUI : MonoBehaviour, IInputCommand, IESCCommand
{
    [SerializeField] private CPlayerStatusUI playerStatsUI;
    // [SerializeField] private 인벤토리 UI

    [SerializeField] private GameObject _backGround;
    [SerializeField] private Button _continueButton;
    [SerializeField] private TextMeshProUGUI _continueText;
    [SerializeField] private Button _diaryButton;
    [SerializeField] private TextMeshProUGUI _diaryText;
    [SerializeField] private Button _settingButton;
    [SerializeField] private TextMeshProUGUI _settingText;
    [SerializeField] private Button _exitButton;
    [SerializeField] private TextMeshProUGUI _extiText;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _localizationTask = new CUniTaskOnes();
    private CUniTaskOnes _settingTask = new CUniTaskOnes();
    private CUniTaskOnes _sceneTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private CInputCommandHub _input;
    private CSettingUI _settingUI;
    private CLocalizationManager _localization;
    private CSceneManager _Scene;
    private CDataManger _data;


    private bool _isActive = false;

    public bool IsActive => _isActive;

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.ESC, this);
            CLog.Log("CESCUI _inputTask OnEnable 작업 완료");
        });
        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            _localization.OnLanguageChanged += ReloadText;
            ReloadText();
            CLog.Log("CESCUI _localizationTask OnEnable 작업 완료");
        });
        _settingTask.Run(async token =>
        {
            _settingUI = await CManagerDontDestroy.SettingsAsync(token);
            CLog.Log("CESCUI _settingTask OnEnable 작업 완료");
        });
        _sceneTask.Run(async token =>
        {
            _Scene = await CManagerDontDestroy.SceneManagerAsync(token);
            CLog.Log("CESCUI _sceneTask OnEnable 작업 완료");
        });
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            CLog.Log("CESCUI _dataTask OnEnable 작업 완료");
        });

        _continueButton.onClick.AddListener(Continue);
        _diaryButton.onClick.AddListener(Diary);
        _settingButton.onClick.AddListener(Setting);
        _exitButton.onClick.AddListener(Exit);
    }

    private void OnDisable()
    {
        _localization.OnLanguageChanged -= ReloadText;
        _input.Unregister(EKeyAction.ESC, this);

        _inputTask?.Cancel();
        _settingTask?.Cancel();
        _localizationTask?.Cancel();
        _sceneTask?.Cancel();
        _dataTask?.Cancel();

        _continueButton.onClick.RemoveListener(Continue);
        _diaryButton.onClick.RemoveListener(Diary);
        _settingButton.onClick.RemoveListener(Setting);
        _exitButton.onClick.RemoveListener(Exit);
    }

    public void ExecuteESC()
    {
        CLog.Warn("test");

        if (playerStatsUI.IsActive)
        {
            CLog.Warn("test1");
            return;
        }

        if (_settingUI.IsActive)
        {
            _settingUI.ActiveUI(false);
            return;
        }

        _isActive = !_isActive;
        _backGround.SetActive(_isActive);

    }

    public void Execute()
    {

    }

    private void ReloadText()
    {
        _continueText.text = _localization.GetText("scene.continuetext.text");
        _diaryText.text = _localization.GetText("scene.diarytext.text");
        _settingText.text = _localization.GetText("scene.settingtext.text");
        _extiText.text = _localization.GetText("scene.extitext.text");
    }


    #region 버튼 명령
    private void Continue()
    {
        _backGround.SetActive(false);
    }

    private void Diary()
    {
        // 구현 필요 
    }

    private void Setting()
    {
        _settingUI.ActiveUI(true);
    }

    private void Exit()
    {
        // 저장 테스트 필요
        _data.SaveGame(_data.CurrentFlow.SaveSlot);

        _backGround.SetActive(false);
        _Scene.ChangeScreenAsync(EScene.Menu).Forget();

    }

    #endregion
}
