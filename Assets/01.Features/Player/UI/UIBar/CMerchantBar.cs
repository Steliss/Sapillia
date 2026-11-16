using TMPro;
using UnityEngine;

public class CMerchantBar : CUIBar
{
    [SerializeField] private TextMeshProUGUI _merchantText;
    [SerializeField] private TextMeshProUGUI _playerText;
    [SerializeField] private CMerchant _merchant;

    private CDataManger _data;
    private CPlayerGamingDataSO _playerData;

    private CUniTaskOnes _dataTask = new CUniTaskOnes();


    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _playerData = _data.PlayerData.GetPlayerData();
            _playerData.OnMoneyChanged += HandleMoneyChanged;
            HandleMoneyChanged(_playerData.Money); // 초기값
            CLog.Log("CMerchantBar _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _playerData.OnMoneyChanged -= HandleMoneyChanged;
    }


    private void HandleMoneyChanged(int currentMoney)
    {
        _merchantText.text = $"{_merchant.Money}";
        _playerText.text = $"{_playerData.Money}";
        SetValue(_merchant.Money, (_merchant.Money + _playerData.Money));
    }

}
