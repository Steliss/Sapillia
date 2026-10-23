using System;
using UnityEngine;

[Serializable]
public class CSettings
{
    [Header("GamePlay")]
    [SerializeField] private bool _skipBoss = false;

    [Range(0f, 1f)]
    [SerializeField] private float _cameraShakeMultiplier = 1f;

    [SerializeField] private bool _displayEffect = true;
    [SerializeField] private ELanguage _currentLanhuage;

    [Header("Sound")]
    [Range(0f, 1f)]
    [SerializeField] private float _masterSound = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float _BGMSound = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float _effectSound = 1f;

    [Range(0f, 1f)]
    [SerializeField] private float _enviromentSound = 1f;

    [Header("Display")]
    [SerializeField] private int _width = 1920;
    [SerializeField] private int _height = 1080;
    [SerializeField] private FullScreenMode _screenMode = FullScreenMode.Windowed;

    [Header("KeySetting")]
    [SerializeField] private CKeyBindingData _moveUp = new CKeyBindingData("Move", "<Keyboard>/w", 1, 6);
    [SerializeField] private CKeyBindingData _moveDown = new CKeyBindingData("Move", "<Keyboard>/s", 2, 7);
    [SerializeField] private CKeyBindingData _moveLeft = new CKeyBindingData("Move", "<Keyboard>/a", 3, 8);
    [SerializeField] private CKeyBindingData _moveRight = new CKeyBindingData("Move", "<Keyboard>/d", 4, 9);
    [SerializeField] private CKeyBindingData _attack = new CKeyBindingData("Attack","<Mouse>/leftButton");
    [SerializeField] private CKeyBindingData _specialAttack = new CKeyBindingData("SpecialAttack","<Mouse>/rightButton");
    [SerializeField] private CKeyBindingData _dash = new CKeyBindingData("Dash","<Keyboard>/space");
    [SerializeField] private CKeyBindingData _rapidCasting1 = new CKeyBindingData("RapidCasting1","<Keyboard>/1");
    [SerializeField] private CKeyBindingData _rapidCasting2 = new CKeyBindingData("RapidCasting2","<Keyboard>/2");
    [SerializeField] private CKeyBindingData _interaction = new CKeyBindingData("Interaction", "<Keyboard>/f");
    [SerializeField] private CKeyBindingData _usePotion = new CKeyBindingData("UsePotion", "<Keyboard>/r");
    [SerializeField] private CKeyBindingData _openMap = new CKeyBindingData("OpenMap", "<Keyboard>/tab");
    [SerializeField] private CKeyBindingData _inventory = new CKeyBindingData("Inventory", "<Keyboard>/v");
    [SerializeField] private CKeyBindingData _playerStats = new CKeyBindingData("Stats","<Keyboard>/c");
    [SerializeField] private CKeyBindingData _talent = new CKeyBindingData("Talent", "<Keyboard>/p"); 
    [SerializeField] private CKeyBindingData _presetSetting = new CKeyBindingData("PresetSetting", "<Keyboard>/b");
    [SerializeField] private CKeyBindingData _chatting = new CKeyBindingData("Chatting", "<Keyboard>/enter");
    [SerializeField] private CKeyBindingData _reloading = new CKeyBindingData("Reloading", "<Keyboard>/g");
    [SerializeField] private CKeyBindingData _ping = new CKeyBindingData("Ping", "<Mouse>/middleButton");
    [SerializeField] private CKeyBindingData _choosePotion = new CKeyBindingData("ChoosePotion", "<Keyboard>/backquote");
    [SerializeField] private CKeyBindingData _before = new CKeyBindingData("Before", "<Keyboard>/q");
    [SerializeField] private CKeyBindingData _next = new CKeyBindingData("Next", "<Keyboard>/e");

    public bool SkipBoss => _skipBoss;
    public float CameraShakeMultiplier => _cameraShakeMultiplier;
    public bool DisplayEffect => _displayEffect;
    public float MasterSound => _masterSound;
    public float BGMSound => _BGMSound;
    public float EffectSound => _effectSound;
    public float EnvironmentSound => _enviromentSound;
    public int Width => _width;
    public int Height => _height;
    public FullScreenMode ScreenMode => _screenMode;
    public ELanguage CurrentLanhuage => _currentLanhuage;


    public void SetSkipBoss(bool skipBoss)
    {
        _skipBoss = skipBoss;
    }

    public void SetCameraShakeMultiplier(float multiplier)
    {
        _cameraShakeMultiplier = multiplier;
    }

    public void SetDisplayEffect(bool displayEffect)
    {
        _displayEffect = displayEffect;
    }

    public void SetMasterSound(float sound)
    {
        _masterSound = sound;
    }

    public void SetBGMSound(float sound)
    {
        _BGMSound = sound;
    }

    public void SetEffectSound(float sound)
    {
        _effectSound = sound;
    }

    public void SetEnvironmentSound(float sound)
    {
        _enviromentSound = sound;
    }

    public void SetCurrentLanhuage(ELanguage language)
    {
        _currentLanhuage = language;
    }

    public void SetDisplay(int width, int height, FullScreenMode screenMode)
    {
        if (width <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "해상도 너비 오류");
        }

        if (height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(height), "해상도 높이 오류");
        }

        _width = width;
        _height = height;
        _screenMode = screenMode;
    }

    public CKeyBindingData GetKeyBinding(EKeyAction action)
    {
        switch (action)
        {
            case EKeyAction.MoveUp:
                return _moveUp;

            case EKeyAction.MoveDown:
                return _moveDown;

            case EKeyAction.MoveLeft:
                return _moveLeft;

            case EKeyAction.MoveRight:
                return _moveRight;

            case EKeyAction.Attack:
                return _attack;

            case EKeyAction.SpecialAttack:
                return _specialAttack;

            case EKeyAction.Dash:
                return _dash;

            case EKeyAction.RapidCasting1:
                return _rapidCasting1;

            case EKeyAction.RapidCasting2:
                return _rapidCasting2;

            case EKeyAction.Interaction:
                return _interaction;

            case EKeyAction.UsePotion:
                return _usePotion;

            case EKeyAction.OpenMap:
                return _openMap;

            case EKeyAction.Inventory:
                return _inventory;

            case EKeyAction.PlayerStats:
                return _playerStats;

            case EKeyAction.Talent:
                return _talent;

            case EKeyAction.PresetSetting:
                return _presetSetting;

            case EKeyAction.Chatting:
                return _chatting;

            case EKeyAction.Reloading:
                return _reloading;

            case EKeyAction.Ping:
                return _ping;

            case EKeyAction.ChoosePotion:
                return _choosePotion;

            case EKeyAction.Before:
                return _before;

            case EKeyAction.Next:
                return _next;

            default:
                throw new ArgumentOutOfRangeException(nameof(action), action, null);
        }
    }

    public void SetKeyBinding(EKeyAction action, EKeySlot slot, string keyPath)
    {
        CKeyBindingData binding = GetKeyBinding(action);

        switch (slot)
        {
            case EKeySlot.Primary:
                binding._primary = keyPath;
                break;

            case EKeySlot.Secondary:
                binding._secondary = keyPath;
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(slot), slot, null);
        }
    }
}


[Serializable]
public class CFlowGame
{
    [SerializeField] private EScene _currentScene;
    private string _name;
    private int _playerprogress;
    private int _playerretrycount;
    private long _playerplaytime;

    public EScene CurrentScene => _currentScene;

    public string PlayerName => _name;
    public int PlayerProgress => _playerprogress;
    public int PlayerRetrycount => _playerretrycount;
    public long Playerplaytime => _playerplaytime;

    public void SetCurrentScene(EScene currentScene)
    {
        _currentScene = currentScene;
    }
}