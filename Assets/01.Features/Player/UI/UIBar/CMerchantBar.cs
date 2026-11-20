using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CMerchantBar : CUIBar, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private TextMeshProUGUI _merchantText;
    [SerializeField] private TextMeshProUGUI _playerText;
    [SerializeField] private CMerchant _merchant;
    [SerializeField] private Button _infoButton;
    [SerializeField] private GameObject _infoObj;
    [SerializeField] private TextMeshProUGUI _infoText;

    private CDataManger _data;
    private CPlayerGamingDataSO _playerData;
    private CLocalizationManager _localization;

    private CUniTaskOnes _dataTask = new CUniTaskOnes();
    private CUniTaskOnes _localizationTask = new();


    private bool _isInitialize = false;

    private EReady _ready = EReady.None;
    private EReady RequiredReady = EReady.Localization | EReady.Data;


    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _playerData = _data.PlayerData.GetPlayerData();
            _playerData.OnMoneyChanged += HandleMoneyChanged;
            HandleMoneyChanged(_playerData.Money); // 초기값
            SetReady(EReady.Data);
            CLog.Log("CMerchantBar _dataTask OnEnable 작업 완료");
        });

        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            SetReady(EReady.Localization);
            CLog.Log("CMerchantUI OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _playerData.OnMoneyChanged -= HandleMoneyChanged;
    }



    public void OnPointerEnter(PointerEventData eventData)
    {
        _infoObj.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _infoObj.SetActive(false);
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

        _infoText.text = _localization.GetText("scene.infotext.text", _playerData.Stats.Bargain, _merchant.Bargain);

        _isInitialize = true;
    }


    private void HandleMoneyChanged(int currentMoney)
    {
        _merchantText.text = $"{_merchant.CurrentMerchant.Money}";
        _playerText.text = $"{_playerData.Money}";
        SetValue(_merchant.CurrentMerchant.Money, (_merchant.CurrentMerchant.Money + _playerData.Money));
    }

}
