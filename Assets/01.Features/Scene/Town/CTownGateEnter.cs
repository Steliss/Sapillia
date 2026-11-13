using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CTownGateEnter : CTriggerPlayer, IInputCommand
{
    [SerializeField] private GameObject _interractObject;
    [SerializeField] private TextMeshPro _text;


    private CLocalizationManager _localization;
    private CSceneManager _sceneManager;
    private CInputCommandHub _input;
    private CDataManger _data;

    private CUniTaskOnes _localizationTask = new CUniTaskOnes();
    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _sceneTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private bool _isActive = false;

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.Interaction, this);
            CLog.Log("CTownGateEnter _inputTask OnEnable 작업 완료");
        });
        _sceneTask.Run(async token =>
        {
            _sceneManager = await CManagerDontDestroy.SceneManagerAsync(token);
            CLog.Log("CTownGateEnter _sceneTask OnEnable 작업 완료");
        });
        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            _localization.OnLanguageChanged += ReloadText;
            ReloadText();
            CLog.Log("CTownGateEnter _localizationTask OnEnable 작업 완료");
        });
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            CLog.Log("CTownGateEnter DataMangerAsync OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask?.Cancel();
        _sceneTask?.Cancel();
        _localizationTask?.Cancel();
        _dataTask?.Cancel();

        _input.Unregister(EKeyAction.Interaction, this);
    }


    private void ReloadText()
    {
        _text.text = _localization.GetText("scene.interractEnter.text");
    }


    protected override void OnPlayerEnter()
    {
        _isActive = true;
        _interractObject.SetActive(true);
    }

    protected override void OnPlayerExit()
    {
        _isActive = false;
        _interractObject.SetActive(false);
    }


    public void Execute()
    {
        if (_isActive)
        {
            _data.CurrentFlow.AddProgress(1);
            _sceneManager.ChangeScreenAsync(EScene.stage1).Forget();
        }
    }
}
