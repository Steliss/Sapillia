using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.UI;



[System.Serializable]
public class CPlayerStatsText
{
    [SerializeField] private TextMeshProUGUI _MP_ReginText;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _defText;
    [SerializeField] private TextMeshProUGUI _strText;
    [SerializeField] private TextMeshProUGUI _evasionText;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _critical_RateText;
    [SerializeField] private TextMeshProUGUI _critical_DamageText;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _attack_SpeedText;
    [SerializeField] private TextMeshProUGUI _movement_SpeedText;
    [SerializeField] private TextMeshProUGUI _run_SpeedMulitplierText;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _bargainText;
    [SerializeField] private TextMeshProUGUI _luckText;

    [Space(10)]
    [SerializeField] private TextMeshProUGUI _requiredEXPText;
    [SerializeField] private TextMeshProUGUI _requiredMoneyText;
    [SerializeField] private TextMeshProUGUI _levelup_HealText;

    public TextMeshProUGUI MP_ReginText
    {
        get => _MP_ReginText;
        set => _MP_ReginText = value;
    }

    public TextMeshProUGUI DefText
    {
        get => _defText;
        set => _defText = value;
    }

    public TextMeshProUGUI StrText
    {
        get => _strText;
        set => _strText = value;
    }

    public TextMeshProUGUI EvasionText
    {
        get => _evasionText;
        set => _evasionText = value;
    }

    public TextMeshProUGUI Critical_RateText
    {
        get => _critical_RateText;
        set => _critical_RateText = value;
    }

    public TextMeshProUGUI Critical_DamageText
    {
        get => _critical_DamageText;
        set => _critical_DamageText = value;
    }

    public TextMeshProUGUI Attack_SpeedText
    {
        get => _attack_SpeedText;
        set => _attack_SpeedText = value;
    }

    public TextMeshProUGUI Movement_SpeedText
    {
        get => _movement_SpeedText;
        set => _movement_SpeedText = value;
    }

    public TextMeshProUGUI Run_SpeedMulitplierText
    {
        get => _run_SpeedMulitplierText;
        set => _run_SpeedMulitplierText = value;
    }

    public TextMeshProUGUI BargainText
    {
        get => _bargainText;
        set => _bargainText = value;
    }

    public TextMeshProUGUI LuckText
    {
        get => _luckText;
        set => _luckText = value;
    }

    public TextMeshProUGUI RequiredEXPText
    {
        get => _requiredEXPText;
        set => _requiredEXPText = value;
    }

    public TextMeshProUGUI RequiredMoneyText
    {
        get => _requiredMoneyText;
        set => _requiredMoneyText = value;
    }

    public TextMeshProUGUI Levelup_HealText
    {
        get => _levelup_HealText;
        set => _levelup_HealText = value;
    }
}

public class CPlayerUI : MonoBehaviour, IInputCommand
{
    [Header("플레이어 캔버스")]
    [SerializeField] private GameObject _UICanvas; 
    [SerializeField] private TextMeshProUGUI _commonStatsText;
    [SerializeField] private TextMeshProUGUI _specialStatsText;
    [SerializeField] private Button _commonStatsButton;
    [SerializeField] private Button _specialStatsButton;
    [SerializeField] private Button _fruitButton;

    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerLevelText;

    [SerializeField] private TextMeshProUGUI _attribute_Physical;
    [SerializeField] private TextMeshProUGUI _attribute_Fire;
    [SerializeField] private TextMeshProUGUI _attribute_Ice;
    [SerializeField] private TextMeshProUGUI _attribute_Lightning;


    [SerializeField] private CPlayerStatsText _commonText = new();
    [Space(30)]
    [SerializeField] private TextMeshProUGUI _HPData;
    [SerializeField] private TextMeshProUGUI _MpData;
    [SerializeField] private CPlayerStatsText _commonData = new();

    private CDataManger _data;
    private CLocalizationManager _localization;
    private CInputCommandHub _input;
    private CPlayerGamingDataSO _playerData;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();
    private CUniTaskOnes _localizationTask = new CUniTaskOnes();



    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.PlayerStats, this);
            _input.Register(EKeyAction.ESC, this);
            CLog.Log("CPlayerUI _inputTask OnEnable 작업 완료");
        });
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _playerData = _data.PlayerData.GetPlayerData();
            ReloadData();
            CLog.Log("CPlayerUI _dataTask OnEnable 작업 완료");
        });
        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            ReloadText();
            CLog.Log("CPlayerUI _localizationTask OnEnable 작업 완료");
        });

        _localization.OnLanguageChanged += ReloadText;
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _dataTask.Cancel();
        _localizationTask.Cancel();

        _input.Unregister(EKeyAction.PlayerStats, this);
        _input.Unregister(EKeyAction.ESC, this);

        _localization.OnLanguageChanged -= ReloadText;
    }


    // 플레이어 스텟창 오픈
    public void Execute()
    {
        // 같은 커맨드를 참고할때 음.. 
    }

    // 언어 변경 세팅
    private void ReloadText()
    {
        _commonStatsText.text = _localization.GetText("player.commonstats.Text");
        _specialStatsText.text = _localization.GetText("player.specialstats.Text");

        _commonText.MP_ReginText.text = _localization.GetText("player.mpregintext.Text");

        _commonText.DefText.text = _localization.GetText("player.deftext.Text");
        _commonText.StrText.text = _localization.GetText("player.strtext.Text");
        _commonText.EvasionText.text = _localization.GetText("player.evasiontext.Text");

        _commonText.Critical_RateText.text = _localization.GetText("player.criticalratetext.Text");
        _commonText.Critical_DamageText.text = _localization.GetText("player.criticaldamagetext.Text");

        _commonText.Attack_SpeedText.text = _localization.GetText("player.attackspeedtext.Text");
        _commonText.Movement_SpeedText.text = _localization.GetText("player.movementspeedtext.Text");
        _commonText.Run_SpeedMulitplierText.text = _localization.GetText("player.runspeedmulitpliertext.Text");

        _commonText.BargainText.text = _localization.GetText("player.bargaintext.Text");
        _commonText.LuckText.text = _localization.GetText("player.lucktext.Text");

        _commonText.RequiredEXPText.text = _localization.GetText("player.requiredexptext.Text");
        _commonText.RequiredMoneyText.text = _localization.GetText("player.requiredmoneytext.Text");
        _commonText.Levelup_HealText.text = _localization.GetText("player.leveluphealtext.Text");
    }

    // 데이터 변경값 적용
    private void ReloadData()
    {
        if (_playerData == null || !_playerData.IsInitialized)
        {
            CLog.Error("_playerData : Null");
            return;
        }

        // 플레이어 이름
        _playerNameText.text = _data.CurrentFlow.PlayerName;
        _playerLevelText.text = _playerData.Level.ToString();

        // 속성 능력치
        _attribute_Physical.text = _playerData.Stats.Attribute_Physical.ToString();
        _attribute_Fire.text = _playerData.Stats.Attribute_Fire.ToString();
        _attribute_Ice.text = _playerData.Stats.Attribute_Ice.ToString();
        _attribute_Lightning.text = _playerData.Stats.Attribute_Lightning.ToString();

        _HPData.text = $"{_playerData.CurrentHP} / {_playerData.Stats.MaxHP}";
        _MpData.text = $"{_playerData.CurrentMP} / {_playerData.Stats.MaxMP}";

        _commonData.MP_ReginText.text = _playerData.Stats.MP_Regin.ToString("0"); // 반올림
        _commonData.DefText.text = _playerData.Stats.Def.ToString();
        _commonData.StrText.text = _playerData.Stats.Str.ToString();
        _commonData.EvasionText.text = _playerData.Stats.Evasion.ToString();
        _commonData.Critical_RateText.text = _playerData.Stats.Critical_Rate.ToString("0.##");
        _commonData.Critical_DamageText.text = _playerData.Stats.Critical_Damage.ToString("0.##");
        _commonData.Attack_SpeedText.text = _playerData.Stats.Attack_Speed.ToString("0.##");
        _commonData.Movement_SpeedText.text = _playerData.Stats.Movement_Speed.ToString("0.##");
        _commonData.Run_SpeedMulitplierText.text = _playerData.Stats.Run_SpeedMulitplier.ToString("0.##");
        _commonData.BargainText.text = _playerData.Stats.Bargain.ToString();
        _commonData.LuckText.text = _playerData.Stats.Luck.ToString();
        _commonData.RequiredEXPText.text = _playerData.RequiredEXP.ToString();
        //_commonData.RequiredMoneyText.text = _playerData.Stats.RequiredMoney.ToString("0.##"); 
        _commonData.Levelup_HealText.text = _playerData.Stats.Levelup_Heal.ToString("0.##");


    }

}
