using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;





[System.Serializable]
public class CPlayerStatsUI
{
    [Header("캐릭터 UI창")]
    [SerializeField] private Button _playerButton;
    [SerializeField] private TextMeshProUGUI _titleText;
    [SerializeField] private TextMeshProUGUI _dataText;

    [Header("마우스 UI")]
    [SerializeField] private GameObject _background;
    [SerializeField] private TextMeshProUGUI _mouseTitleText;
    [SerializeField] private TextMeshProUGUI _mouseDescriptionText;
    [SerializeField] private TextMeshProUGUI _mousedataText;


    public Button PlayerButton
    {
        get => _playerButton;
        set => _playerButton = value;
    }

    public TextMeshProUGUI TitleText
    {
        get => _titleText;
        set => _titleText = value;
    }

    public TextMeshProUGUI DataText
    {
        get => _dataText;
        set => _dataText = value;
    }

    public GameObject Background
    {
        get => _background;
        set => _background = value;
    }

    public TextMeshProUGUI MouseTitleText
    {
        get => _mouseTitleText;
        set => _mouseTitleText = value;
    }

    public TextMeshProUGUI DescriptionText
    {
        get => _mouseDescriptionText;
        set => _mouseDescriptionText = value;
    }

    public TextMeshProUGUI MousedataText
    {
        get => _mousedataText;
        set => _mousedataText = value;
    }

}

// ※ : TODO : 이미지랑 UI 쪽 이미지 넣으려면 계층구조랑 싹 수정할 필요가 있음
public class CPlayerStatusUI : MonoBehaviour, IInputCommand, IESCCommand
{
    [Header("플레이어 캔버스")]
    [SerializeField] private GameObject _UICanvas;
    [SerializeField] private GameObject _specialStats;
    [SerializeField] private GameObject _commonStats;
    [SerializeField] private TextMeshProUGUI _commonStatsText;
    [SerializeField] private TextMeshProUGUI _specialStatsText;
    [SerializeField] private Button _commonStatsButton;
    [SerializeField] private Button _specialStatsButton;
    [SerializeField] private Button _fruitButton;

    [SerializeField] private TextMeshProUGUI _playerNameText;
    [SerializeField] private TextMeshProUGUI _playerLevelText;

    // ※ : TODO : 너도 수정대상이야
    [SerializeField] private TextMeshProUGUI _attribute_Physical;
    [SerializeField] private TextMeshProUGUI _attribute_Fire;
    [SerializeField] private TextMeshProUGUI _attribute_Ice;
    [SerializeField] private TextMeshProUGUI _attribute_Lightning;

    [Space(30)]
    [SerializeField] private CPlayerStatsUI _HPUI;
    [SerializeField] private CPlayerStatsUI _MPUI;
    [SerializeField] private CPlayerStatsUI _MP_ReginUI;
    [Space (30)]
    [SerializeField] private CPlayerStatsUI _defUI;
    [SerializeField] private CPlayerStatsUI _strUI;
    [SerializeField] private CPlayerStatsUI _evasionUI;
    [Space(30)]
    [SerializeField] private CPlayerStatsUI _critical_RateUI;
    [SerializeField] private CPlayerStatsUI _critical_DamageUI;
    [Space(30)]
    [SerializeField] private CPlayerStatsUI _attack_SpeedUI;
    [SerializeField] private CPlayerStatsUI _movement_SpeedUI;
    [SerializeField] private CPlayerStatsUI _run_SpeedMulitplierUI;
    [Space(30)]
    [SerializeField] private CPlayerStatsUI _bargainUI;
    [SerializeField] private CPlayerStatsUI _luckUI;
    [Space(30)]
    [SerializeField] private CPlayerStatsUI _requiredEXPUI;
    [SerializeField] private CPlayerStatsUI _requiredMoneyUI;
    [SerializeField] private CPlayerStatsUI _levelup_HealUI;
    [Space(30)]
    [SerializeField] private TextMeshProUGUI _damageTitleText;
    [SerializeField] private CPlayerStatsUI _damage_AllUI;
    [SerializeField] private CPlayerStatsUI _damage_FixedUI;
    [SerializeField] private CPlayerStatsUI _damage_BreakUI;
    [SerializeField] private CPlayerStatsUI _damage_ReflectUI;
    [Space(30)]
    [SerializeField] private TextMeshProUGUI _weaponTitleText;
    [SerializeField] private CPlayerStatsUI _damage_WeaponUI;
    [SerializeField] private CPlayerStatsUI _damage_NormalUI;
    [SerializeField] private CPlayerStatsUI _damage_DashUI;
    [SerializeField] private CPlayerStatsUI _damage_SpecialUI;

    private CDataManger _data;
    private CLocalizationManager _localization;
    private CInputCommandHub _input;
    private CPlayerGamingDataSO _playerData;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();
    private CUniTaskOnes _localizationTask = new CUniTaskOnes();

    private bool _isActive = false;

    public bool isActive => _isActive;

    

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
            // ReloadData();
            CLog.Log("CPlayerUI _dataTask OnEnable 작업 완료");
        });
        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            _localization.OnLanguageChanged += ReloadText;
            // ReloadText();
            CLog.Log("CPlayerUI _localizationTask OnEnable 작업 완료");
        });

        _commonStatsButton.onClick.AddListener(SetAsLastSiblingCommonStats);
        _specialStatsButton.onClick.AddListener(SetAsLastSiblingSpecialStats);
        _fruitButton.onClick.AddListener(FruitButton);
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _dataTask.Cancel();
        _localizationTask.Cancel();

        _input.Unregister(EKeyAction.PlayerStats, this);
        _input.Unregister(EKeyAction.ESC, this);

        _localization.OnLanguageChanged -= ReloadText;

        _commonStatsButton.onClick.RemoveListener(SetAsLastSiblingCommonStats);
        _specialStatsButton.onClick.RemoveListener(SetAsLastSiblingSpecialStats);
        _fruitButton.onClick.RemoveListener(FruitButton);

    }

    // 버튼 연결 이벤트 해제
    private void OnDestroy()
    {
        ClearButtonEvent(_HPUI.PlayerButton);
        ClearButtonEvent(_MPUI.PlayerButton);
        ClearButtonEvent(_MP_ReginUI.PlayerButton);

        ClearButtonEvent(_defUI.PlayerButton);
        ClearButtonEvent(_strUI.PlayerButton);
        ClearButtonEvent(_evasionUI.PlayerButton);

        ClearButtonEvent(_critical_RateUI.PlayerButton);
        ClearButtonEvent(_critical_DamageUI.PlayerButton);

        ClearButtonEvent(_attack_SpeedUI.PlayerButton);
        ClearButtonEvent(_movement_SpeedUI.PlayerButton);
        ClearButtonEvent(_run_SpeedMulitplierUI.PlayerButton);

        ClearButtonEvent(_bargainUI.PlayerButton);
        ClearButtonEvent(_luckUI.PlayerButton);

        ClearButtonEvent(_requiredEXPUI.PlayerButton);
        ClearButtonEvent(_requiredMoneyUI.PlayerButton);
        ClearButtonEvent(_levelup_HealUI.PlayerButton);

        ClearButtonEvent(_damage_AllUI.PlayerButton);
        ClearButtonEvent(_damage_FixedUI.PlayerButton);
        ClearButtonEvent(_damage_BreakUI.PlayerButton);
        ClearButtonEvent(_damage_ReflectUI.PlayerButton);

        ClearButtonEvent(_damage_WeaponUI.PlayerButton);
        ClearButtonEvent(_damage_NormalUI.PlayerButton);
        ClearButtonEvent(_damage_DashUI.PlayerButton);
        ClearButtonEvent(_damage_SpecialUI.PlayerButton);
    }


    private void Awake()
    {
        SetButton(_HPUI.PlayerButton, _HPUI.TitleText, _HPUI.Background); 
        SetButton(_MPUI.PlayerButton, _MPUI.TitleText, _MPUI.Background);
        SetButton(_MP_ReginUI.PlayerButton, _MP_ReginUI.TitleText, _MP_ReginUI.Background);

        SetButton(_defUI.PlayerButton, _defUI.TitleText, _defUI.Background);
        SetButton(_strUI.PlayerButton, _strUI.TitleText, _strUI.Background);
        SetButton(_evasionUI.PlayerButton, _evasionUI.TitleText, _evasionUI.Background);

        SetButton(_critical_RateUI.PlayerButton, _critical_RateUI.TitleText, _critical_RateUI.Background);
        SetButton(_critical_DamageUI.PlayerButton, _critical_DamageUI.TitleText, _critical_DamageUI.Background);

        SetButton(_attack_SpeedUI.PlayerButton, _attack_SpeedUI.TitleText, _attack_SpeedUI.Background);
        SetButton(_movement_SpeedUI.PlayerButton, _movement_SpeedUI.TitleText, _movement_SpeedUI.Background);
        SetButton(_run_SpeedMulitplierUI.PlayerButton, _run_SpeedMulitplierUI.TitleText, _run_SpeedMulitplierUI.Background);

        SetButton(_bargainUI.PlayerButton, _bargainUI.TitleText, _bargainUI.Background);
        SetButton(_luckUI.PlayerButton, _luckUI.TitleText, _luckUI.Background);

        SetButton(_requiredEXPUI.PlayerButton, _requiredEXPUI.TitleText, _requiredEXPUI.Background);
        SetButton(_requiredMoneyUI.PlayerButton, _requiredMoneyUI.TitleText, _requiredMoneyUI.Background);
        SetButton(_levelup_HealUI.PlayerButton, _levelup_HealUI.TitleText, _levelup_HealUI.Background);

        SetButton(_damage_AllUI.PlayerButton, _damage_AllUI.TitleText, _damage_AllUI.Background);
        SetButton(_damage_FixedUI.PlayerButton, _damage_FixedUI.TitleText, _damage_FixedUI.Background);
        SetButton(_damage_BreakUI.PlayerButton, _damage_BreakUI.TitleText, _damage_BreakUI.Background);
        SetButton(_damage_ReflectUI.PlayerButton, _damage_ReflectUI.TitleText, _damage_ReflectUI.Background);
        
        SetButton(_damage_WeaponUI.PlayerButton, _damage_WeaponUI.TitleText, _damage_WeaponUI.Background);
        SetButton(_damage_NormalUI.PlayerButton, _damage_NormalUI.TitleText, _damage_NormalUI.Background);
        SetButton(_damage_DashUI.PlayerButton, _damage_DashUI.TitleText, _damage_DashUI.Background);
        SetButton(_damage_SpecialUI.PlayerButton, _damage_SpecialUI.TitleText, _damage_SpecialUI.Background);
    }

    // 플레이어 스텟창 오픈
    public void Execute()
    {
        ReloadData();
        ReloadText();
        _isActive = true;
        _UICanvas.SetActive(true);
    }

    // 플레이어 스텟창 닫기
    public void ExecuteESC()
    {
        _UICanvas.SetActive(false);
        _isActive = false;
    }

    // SetButton 이벤트에 따른 이벤트 해제
    private void ClearButtonEvent(Button button)
    {
        if (button == null)
        {
            return;
        }

        EventTrigger trigger = button.GetComponent<EventTrigger>();

        trigger.triggers.Clear();
    }

    // 마우스 이동에 따른 설명 UI 오픈
    private void SetButton(Button button, TextMeshProUGUI text, GameObject UI)
    {
        if (button == null || text == null || UI == null)
        {
            CLog.Warn("[CPlayerUI] 버튼 또는 UI 참조 누락");
            return;
        }

        button.targetGraphic = text;
        button.transition = Selectable.Transition.ColorTint;

        text.color = Color.white;
        text.raycastTarget = true;

        Color textColor = new Color32(200, 200, 200, 235);

        ColorBlock textColors = button.colors;
        textColors.normalColor = textColor;
        textColors.highlightedColor = Color.white;
        textColors.selectedColor = Color.white;

        button.colors = textColors;

        // 설명창 초기 상태
        UI.SetActive(false);

        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }

        // 마우스 진입: 설명창 열기
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener(eventData =>
        {
            UI.SetActive(true);
        });

        // 마우스 이탈: 설명창 닫기
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener(eventData =>
        {
            UI.SetActive(false);
        });

        // 이벤트에 대한 해제 이벤트가 추가되면 고민이 필요
        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    // 언어 변경 세팅
    private void ReloadText()
    {
        if (_playerData == null || !_playerData.IsInitialized)
        {
            CLog.Warn("[CPlayerUI] 플레이어 데이터 누락");
            return;
        }

        _commonStatsText.text = _localization.GetText("player.commonstats.Text");
        _specialStatsText.text = _localization.GetText("player.specialstats.Text");

        _MP_ReginUI.TitleText.text = _localization.GetText("player.mpregintext.Text");

        _defUI.TitleText.text = _localization.GetText("player.deftext.Text");
        _strUI.TitleText.text = _localization.GetText("player.strtext.Text");
        _evasionUI.TitleText.text = _localization.GetText("player.evasiontext.Text");

        _critical_RateUI.TitleText.text = _localization.GetText("player.criticalratetext.Text");
        _critical_DamageUI.TitleText.text = _localization.GetText("player.criticaldamagetext.Text");

        _attack_SpeedUI.TitleText.text = _localization.GetText("player.attackspeedtext.Text");
        _movement_SpeedUI.TitleText.text = _localization.GetText("player.movementspeedtext.Text");
        _run_SpeedMulitplierUI.TitleText.text = _localization.GetText("player.runspeedmulitpliertext.Text");

        _bargainUI.TitleText.text = _localization.GetText("player.bargaintext.Text");
        _luckUI.TitleText.text = _localization.GetText("player.lucktext.Text");

        _requiredEXPUI.TitleText.text = _localization.GetText("player.requiredexptext.Text");
        _requiredMoneyUI.TitleText.text = _localization.GetText("player.requiredmoneytext.Text");
        _levelup_HealUI.TitleText.text = _localization.GetText("player.leveluphealtext.Text");

        _damageTitleText.text = _localization.GetText("player.damagetitletext.Text");
        _damage_AllUI.TitleText.text = _localization.GetText("player.damagealltext.Text");
        _damage_FixedUI.TitleText.text = _localization.GetText("player.damagefixedtext.Text");
        _damage_BreakUI.TitleText.text = _localization.GetText("player.damagebreaktext.Text");
        _damage_ReflectUI.TitleText.text = _localization.GetText("player.damagereflecttext.Text");
        
        _weaponTitleText.text = _localization.GetText("player.weapontitletext.Text");
        _damage_WeaponUI.TitleText.text = _localization.GetText("player.damageweapontext.Text");
        _damage_NormalUI.TitleText.text = _localization.GetText("player.damagenormaltext.Text");
        _damage_DashUI.TitleText.text = _localization.GetText("player.damagedashtext.Text");
        _damage_SpecialUI.TitleText.text = _localization.GetText("player.damagespecialtext.Text");

    }

    // 데이터 변경값 적용
    private void ReloadData()
    {
        if (_playerData == null || !_playerData.IsInitialized)
        {
            CLog.Warn("[CPlayerUI] 플레이어 데이터 누락");
            return;
        }

        SPlayerStats stats = _playerData.Stats;

        // 플레이어 이름 / 레벨
        _playerNameText.text = _data.CurrentFlow.PlayerName;
        _playerLevelText.text = $"Lv {_playerData.Level}";

        // 속성 능력치
        _attribute_Physical.text = stats.Attribute_Physical.ToString();
        _attribute_Fire.text = stats.Attribute_Fire.ToString();
        _attribute_Ice.text = stats.Attribute_Ice.ToString();
        _attribute_Lightning.text = stats.Attribute_Lightning.ToString();

        // HP / MP
        _HPUI.DataText.text = $"{_playerData.CurrentHP} / {stats.MaxHP}";
        _MPUI.DataText.text = $"{_playerData.CurrentMP} / {stats.MaxMP}";

        // 공통 능력치
        _MP_ReginUI.DataText.text = stats.MP_Regin.ToString("0");

        _defUI.DataText.text = stats.Def.ToString();
        _strUI.DataText.text = stats.Str.ToString();
        _evasionUI.DataText.text = stats.Evasion.ToString();

        _critical_RateUI.DataText.text = stats.Critical_Rate.ToString("0.##");
        _critical_DamageUI.DataText.text = stats.Critical_Damage.ToString("0.##");

        _attack_SpeedUI.DataText.text = stats.Attack_Speed.ToString("0.##");
        _movement_SpeedUI.DataText.text = stats.Movement_Speed.ToString("0.##");
        _run_SpeedMulitplierUI.DataText.text = stats.Run_SpeedMulitplier.ToString("0.##");

        _bargainUI.DataText.text = stats.Bargain.ToString();
        _luckUI.DataText.text = stats.Luck.ToString();

        _requiredEXPUI.DataText.text = _playerData.RequiredEXP.ToString();
        _requiredMoneyUI.DataText.text = stats.PickUpMoney_Rate.ToString("0.##");
        _levelup_HealUI.DataText.text = stats.Levelup_Heal.ToString("0.##");

        _damage_AllUI.DataText.text = stats.Damage_All.ToString("0.##");
        _damage_FixedUI.DataText.text = stats.Damage_Fixed.ToString();
        _damage_BreakUI.DataText.text = stats.Damage_Break.ToString("0.##");
        _damage_ReflectUI.DataText.text = stats.Damage_Reflect.ToString("0.##");
        
        _damage_WeaponUI.DataText.text = stats.Damage_Weapon.ToString("0.##");
        _damage_NormalUI.DataText.text = stats.Damage_Normal.ToString("0.##");
        _damage_DashUI.DataText.text = stats.Damage_Dash.ToString("0.##");
        _damage_SpecialUI.DataText.text = stats.Damage_Special.ToString("0.##");

        // 마우스 부분
        _HPUI.MouseTitleText.text = _localization.GetText("player.hp.mousetitle.Text");
        _HPUI.DescriptionText.text = _localization.GetText("player.hp.mousedescription.Text");
        _MPUI.MouseTitleText.text = _localization.GetText("player.mp.mousetitle.Text");
        _MPUI.DescriptionText.text = _localization.GetText("player.mp.mousedescription.Text");
        _MP_ReginUI.MouseTitleText.text = _localization.GetText("player.mpregin.mousetitle.Text");
        _MP_ReginUI.DescriptionText.text = _localization.GetText("player.mpregin.mousedescription.Text");

        _defUI.MouseTitleText.text = _localization.GetText("player.def.mousetitle.Text");
        _defUI.DescriptionText.text = _localization.GetText("player.def.mousedescription.Text");
        _strUI.MouseTitleText.text = _localization.GetText("player.str.mousetitle.Text");
        _strUI.DescriptionText.text = _localization.GetText("player.str.mousedescription.Text");
        _evasionUI.MouseTitleText.text = _localization.GetText("player.evasion.mousetitle.Text");
        _evasionUI.DescriptionText.text = _localization.GetText("player.evasion.mousedescription.Text");

        _critical_RateUI.MouseTitleText.text = _localization.GetText("player.criticalrate.mousetitle.Text");
        _critical_RateUI.DescriptionText.text = _localization.GetText("player.criticalrate.mousedescription.Text");
        _critical_DamageUI.MouseTitleText.text = _localization.GetText("player.criticaldamage.mousetitle.Text");
        _critical_DamageUI.DescriptionText.text = _localization.GetText("player.criticaldamage.mousedescription.Text");

        _attack_SpeedUI.MouseTitleText.text = _localization.GetText("player.attackspeed.mousetitle.Text");
        _attack_SpeedUI.DescriptionText.text = _localization.GetText("player.attackspeed.mousedescription.Text");
        _movement_SpeedUI.MouseTitleText.text = _localization.GetText("player.movementspeed.mousetitle.Text");
        _movement_SpeedUI.DescriptionText.text = _localization.GetText("player.movementspeed.mousedescription.Text");
        _run_SpeedMulitplierUI.MouseTitleText.text = _localization.GetText("player.runspeedmulitplier.mousetitle.Text");
        _run_SpeedMulitplierUI.DescriptionText.text = _localization.GetText("player.runspeedmulitplier.mousedescription.Text");

        _bargainUI.MouseTitleText.text = _localization.GetText("player.bargain.mousetitle.Text");
        _bargainUI.DescriptionText.text = _localization.GetText("player.bargain.mousedescription.Text");
        _luckUI.MouseTitleText.text = _localization.GetText("player.luck.mousetitle.Text");
        _luckUI.DescriptionText.text = _localization.GetText("player.luck.mousedescription.Text");

        _requiredEXPUI.MouseTitleText.text = _localization.GetText("player.requiredexp.mousetitle.Text");
        _requiredEXPUI.DescriptionText.text = _localization.GetText("player.requiredexp.mousedescription.Text");
        _requiredMoneyUI.MouseTitleText.text = _localization.GetText("player.requiredmoney.mousetitle.Text");
        _requiredMoneyUI.DescriptionText.text = _localization.GetText("player.requiredmoney.mousedescription.Text");
        _levelup_HealUI.MouseTitleText.text = _localization.GetText("player.levelupheal.mousetitle.Text");
        _levelup_HealUI.DescriptionText.text = _localization.GetText("player.levelupheal.mousedescription.Text");

        _damage_AllUI.MouseTitleText.text = _localization.GetText("player.damageall.mousetitle.Text");
        _damage_AllUI.DescriptionText.text = _localization.GetText("player.damageall.mousedescription.Text");
        _damage_FixedUI.MouseTitleText.text = _localization.GetText("player.damagefixed.mousetitle.Text");
        _damage_FixedUI.DescriptionText.text = _localization.GetText("player.damagefixed.mousedescription.Text");
        _damage_BreakUI.MouseTitleText.text = _localization.GetText("player.damagebreak.mousetitle.Text");
        _damage_BreakUI.DescriptionText.text = _localization.GetText("player.damagebreak.mousedescription.Text");
        _damage_ReflectUI.MouseTitleText.text = _localization.GetText("player.damagereflect.mousetitle.Text");
        _damage_ReflectUI.DescriptionText.text = _localization.GetText("player.damagereflect.mousedescription.Text");

        _damage_WeaponUI.MouseTitleText.text = _localization.GetText("player.damageweapon.mousetitle.Text");
        _damage_WeaponUI.DescriptionText.text = _localization.GetText("player.damageweapon.mousedescription.Text");
        _damage_NormalUI.MouseTitleText.text = _localization.GetText("player.damagenormal.mousetitle.Text");
        _damage_NormalUI.DescriptionText.text = _localization.GetText("player.damagenormal.mousedescription.Text");
        _damage_DashUI.MouseTitleText.text = _localization.GetText("player.damagedash.mousetitle.Text");
        _damage_DashUI.DescriptionText.text = _localization.GetText("player.damagedash.mousedescription.Text");
        _damage_SpecialUI.MouseTitleText.text = _localization.GetText("player.damagespecial.mousetitle.Text");
        _damage_SpecialUI.DescriptionText.text = _localization.GetText("player.damagespecial.mousedescription.Text");
    }

    // 버튼 누를시 해당 창이 앞에 오게 
    private void SetAsLastSiblingCommonStats()
    {
        _commonStats.transform.SetAsLastSibling();
    }

    private void SetAsLastSiblingSpecialStats()
    {
        _specialStats.transform.SetAsLastSibling();
    }

    // 과일 버튼 관련
    private void FruitButton()
    {

    }
}
