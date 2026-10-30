using TMPro;
using UnityEngine;

public class PlayerMPBar : CUIBar
{
    [SerializeField] private TextMeshProUGUI _text;

    private CDataManger _data;
    private CPlayerGamingDataSO _playerData;

    private CUniTaskOnes _dataTask = new CUniTaskOnes();


    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _playerData = _data.PlayerData.GetPlayerData();
            _playerData.OnMPChanged += HandleMPChanged;
            HandleMPChanged(_playerData.CurrentMP, _playerData.Stats.MaxMP); // 초기값
            CLog.Log("PlayerHPBar _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _playerData.OnMPChanged -= HandleMPChanged;
    }


    private void HandleMPChanged(int currentHP, int maxHP)
    {
        _text.text = $"{currentHP:0} / {maxHP:0}";
        SetValue(currentHP, maxHP);
    }
}