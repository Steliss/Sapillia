using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CESCUI : MonoBehaviour, IInputCommand, IESCCommand
{
    [SerializeField] private CPlayerStatusUI playerStatsUI;
    [SerializeField] private CInventoryUI inventoryUI;

    [SerializeField] private GameObject _backGround;
    [SerializeField] private Button _continueButton;
    [SerializeField] private TextMeshProUGUI _continueText;
    [SerializeField] private Button _giveUPButton;
    [SerializeField] private TextMeshProUGUI _giveUPText;
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
    private bool _isInitialize = false;

    private EReady _ready = EReady.None;
    private EReady RequiredReady = EReady.Localization | EReady.Data;

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
            SetReady(EReady.Localization);
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
            SetReady(EReady.Data);
            CLog.Log("CESCUI _sceneTask OnEnable 작업 완료");
        });
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            CLog.Log("CESCUI _dataTask OnEnable 작업 완료");
        });

        _continueButton.onClick.AddListener(Continue);
        _giveUPButton.onClick.AddListener(GiveUP);
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
        _giveUPButton.onClick.RemoveListener(GiveUP);
        _settingButton.onClick.RemoveListener(Setting);
        _exitButton.onClick.RemoveListener(Exit);
    }

    public void ExecuteESC()
    {
        if (_settingUI.IsActive)
        {
            _settingUI.ActiveUI(false);
            return;
        }

        if (inventoryUI.IsActive)
        {
            inventoryUI.ActiveUI(false);
            return;
        }

        if (playerStatsUI.IsActive)
        {
            playerStatsUI.ActiveUI(false);
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
        _giveUPText.text = _localization.GetText("scene.giveuptext.text");
        _settingText.text = _localization.GetText("scene.settingtext.text");
        _extiText.text = _localization.GetText("scene.extitext.text");
    }

    private void SetReady(EReady ready)
    {
        _ready |= ready;

        if (_isInitialize)
        {
            return;
        }

        if ((_ready & RequiredReady) != RequiredReady)
        {
            return;
        }

        _isInitialize = true;

        if(_data.CurrentFlow.CurrentScene == EScene.Town)
        {
            _giveUPText.text = _localization.GetText("scene.enmpty.text");
        }
    }

    #region 버튼 명령
    private void Continue()
    {
        _backGround.SetActive(false);
    }

    private void GiveUP()
    {
        if(_data.CurrentFlow.CurrentScene == EScene.Town)
        {
            return;
        }

        // 포기 관련
        if(_data.ResetPlayer())
        {
            // 패배 관련 띄우기 여부 

            _Scene.ChangeScreenAsync(EScene.Town).Forget();
        }
        else
        {
            CLog.Warn("CESCUI 플레이어 초기화 오류");
        }
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
