using UnityEngine;

public class PlayerEXPBar : CUIBar
{
    private CDataManger _data;
    private CPlayerGamingDataSO _playerData;

    private CUniTaskOnes _dataTask = new CUniTaskOnes();


    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _playerData = _data.PlayerData.GetPlayerData();
            _playerData.OnEXPChanged += HandleEXPChanged;
            HandleEXPChanged(_playerData.CurrentEXP, _playerData.RequiredEXP); // 초기값
            CLog.Log("PlayerHPBar _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _playerData.OnEXPChanged -= HandleEXPChanged;
    }

    private void HandleEXPChanged(int currentHP, int maxHP)
    {
        SetValue(currentHP, maxHP);
    }
}