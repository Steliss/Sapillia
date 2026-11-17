using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CGameFlow : MonoBehaviour
{
    // 게임 패배 및 승리 판정 
    // 게임 패배시 패배에 맞는 오브젝트 

    [SerializeField] private GameObject _canvas;
    [SerializeField] private Button _retryButton;
    [SerializeField] private TextMeshProUGUI _retryText;

    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private TextMeshProUGUI _liveTimeText;
    [SerializeField] private TextMeshProUGUI _liveTitmeTime;
    [SerializeField] private TextMeshProUGUI _locationText;
    [SerializeField] private TextMeshProUGUI _locationlocation;
    [SerializeField] private TextMeshProUGUI _earnSappireText;
    [SerializeField] private TextMeshProUGUI _earnSappireCount;

    [SerializeField] private TextMeshProUGUI _SappireText;
    [SerializeField] private TextMeshProUGUI _SappireCount;
    [SerializeField] private TextMeshProUGUI _SappireEarn;

    [SerializeField] private GameObject _defeatObject;
    //[SerializeField] private GameObject _winObject;


    private CLocalizationManager _localization;
    private CDataManger _data;
    private CPlayerGamingDataSO _playerData;
    private CSceneManager _scene;

    private CUniTaskOnes _dataTask = new CUniTaskOnes();
    private CUniTaskOnes _localizationTask = new CUniTaskOnes();
    private CUniTaskOnes _sceneTask = new CUniTaskOnes();

    private bool _stopActive;

    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _playerData = _data.PlayerData.GetPlayerData();
            _playerData.OnHPChanged += DefeatGame;
            CLog.Log("CGameFlow _dataTask OnEnable 작업 완료");
        });
        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            CLog.Log("CGameFlow _localizationTask OnEnable 작업 완료");
        });
        _sceneTask.Run(async token =>
        {
            _scene = await CManagerDontDestroy.SceneManagerAsync(token);
            CLog.Log("CGameFlow _sceneTask OnEnable 작업 완료");
        });

        _retryButton.onClick.AddListener(RetryButton);
    }

    private void OnDisable()
    {
        _dataTask?.Cancel();
        _localizationTask?.Cancel();

        _retryButton.onClick.RemoveListener(RetryButton);
        _playerData.OnHPChanged -= DefeatGame;
    }

    private void DefeatGame(int currentHP, int maxHP)
    {
        if (currentHP > 0)
        {
            return;
        }

        ReloadText(false);
        _defeatObject.SetActive(true);
    }

    private void WinGame()
    {
        ReloadText(true);
        //_winObject.SetActive(true);
    }

    private void ReloadText(bool win)
    {
        if (_stopActive == false)
        {
            Time.timeScale = 0f;
        }

        _stopActive = true;

        _canvas.SetActive(true);

        _retryText.text = _localization.GetText("scene.retrytext.text");
        _title.text = _localization.GetText(win ? "scene.wintext.text" : "scene.defeattext.text");

        _liveTimeText.text = _localization.GetText("scene.livetime.text");
        //_liveTitmeTime.text = _data.CurrentFlow.; 
        _locationText.text = _localization.GetText("scene.location.text"); 


        if (_data.CurrentFlow.CurrentScene == EScene.stage1)
        {
            _locationlocation.text = _localization.GetText("scene.stage1.mapname");
        }
        else if (_data.CurrentFlow.CurrentScene == EScene.stage2)
        {
            _locationlocation.text = _localization.GetText("scene.stage2.mapname");
        }
        else
        {
            _locationlocation.text = _localization.GetText("scene.enmpty.text");
        }

        _earnSappireText.text = _localization.GetText("scene.earnsappire.text"); ;
        _earnSappireCount.text = _data.CurrentFlow.CurrentGameSapphire.ToString();

        _SappireText.text = _localization.GetText("scene.sappire.text");
        _SappireCount.text = _data.CurrentFlow.Sapphire.ToString();
        _SappireEarn.text = _data.CurrentFlow.CurrentGameSapphire.ToString();
    }

    private void RetryButton()
    {
        Time.timeScale = 1f;
        _data.ResetPlayer();
        _scene.ChangeScreenAsync(EScene.Town).Forget();
    }
}
