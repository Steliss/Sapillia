using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CMerchantBag : CTriggerPlayer, IInputCommand, IESCCommand
{
    [SerializeField] private int _cost = 200;

    [SerializeField] private CLogUI _log;
    [SerializeField] private CInventoryUI _ui;
    [SerializeField] private GameObject _buyCanvas;
    [SerializeField] private Button _buttonYes;
    [SerializeField] private Button _buttonNo;
    [SerializeField] private TextMeshProUGUI _infoDesc;
    [SerializeField] private TextMeshProUGUI _yesText;
    [SerializeField] private TextMeshProUGUI _noText;

    private CInputCommandHub _input;
    private CDataManger _data;
    private CInventoryData _inventory;
    private CLocalizationManager _localization;

    private CUniTaskOnes _inputTask = new();
    private CUniTaskOnes _dataTask = new();
    private CUniTaskOnes _localizationTask = new ();


    private bool _isNear;
    //private bool _isActive = false;


    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.Interaction, this);

            CLog.Log("CMerchant _inputTask OnEnable 작업 완료");
        });
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _inventory = _data.InventoryData.GetInventoryData();

            CLog.Log("CMerchant _dataTask OnEnable 작업 완료");
        });
        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            ReloadText();
            CLog.Log("CMerchant _inputTask OnEnable 작업 완료");
        });

        _buttonYes.onClick.AddListener(ButtonYes);
        _buttonNo.onClick.AddListener(ButtonNo);
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _dataTask.Cancel();
        _localizationTask.Cancel();

        _input?.Unregister(EKeyAction.Interaction, this);

        _buttonYes.onClick.RemoveListener(ButtonYes);
        _buttonNo.onClick.RemoveListener(ButtonNo);

    }

    protected override void OnPlayerEnter()
    {
        _isNear = true;
    }

    protected override void OnPlayerExit()
    {
        _isNear = false;
    }

    public void Execute()
    {
        if (_isNear)
        {
            ReloadText();
            ActiveUI(true);
        }
    }

    public void ExecuteESC()
    {
            ActiveUI(false);
    }

    public void ActiveUI(bool active)
    {
        _buyCanvas.SetActive(active);

        if (_input != null)
        {
            if (active)
            {
                _input.RegisterOpenedUI(this);
            }
            else
            {
                _input.UnregisterOpenedUI(this);
            }
        }
    }

    private void ReloadText()
    {
        _infoDesc.text = _localization.GetText("scene.bagbuy.des", _cost);
        _yesText.text = _localization.GetText("scene.yes.text", "#90EE90");
        _noText.text = _localization.GetText("scene.no.text", "#FF7F7F");
    }

    private void ButtonYes()
    {
        CPlayerGamingDataSO playerdata = _data.PlayerData.GetPlayerData();

        if(playerdata.TrySpendMoney(_cost))
        {
            _inventory.ExpandInventory(1);
            _ui.AddSlots();
            _buyCanvas.SetActive(false);
        }
        else
        {
            _log.AddSystem("scene.cantbuy.des");
            _buyCanvas.SetActive(false);
        }
    }

    private void ButtonNo()
    {
        ActiveUI(false);
    }

}
