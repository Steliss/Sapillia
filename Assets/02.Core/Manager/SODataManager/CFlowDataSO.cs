using System;
using UnityEngine;


// 씬 및 옵션 관리 SO
[CreateAssetMenu(menuName = "SO/Data/Data", fileName = "OptionSO")]
public class CFlowDataSO : ScriptableObject
{
    [SerializeField] private EScene _currentScene; // 맵 정보 

    [Header("GamePlay")]
    [SerializeField] private bool _skipBoss = false;
    [Range(0f, 1f)]
    [SerializeField] private float _cameraShakeMultiplier = 1.0f;
    [SerializeField] private bool _displayEffect = true;
    [SerializeField] private ELanguage _currentLanhuage;

    [Header("Sound")]
    [Range(0f, 1f)]
    [SerializeField] private float _masterSound = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float _BGMSound = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float _effectSound = 1.0f;
    [Range(0f, 1f)]
    [SerializeField] private float _enviromentSound = 1.0f;

    [Header("Display")]
    [SerializeField] private int _width = 1920;
    [SerializeField] private int _height = 1080;
    [SerializeField] private FullScreenMode _screenMode = FullScreenMode.Windowed;

    [Header("KeySetting")]
    [SerializeField] private CKeyBindingData _moveUp = new CKeyBindingData("<Keyboard>/w");
    [SerializeField] private CKeyBindingData _moveDown = new CKeyBindingData("<Keyboard>/s");
    [SerializeField] private CKeyBindingData _moveLeft = new CKeyBindingData("<Keyboard>/a");
    [SerializeField] private CKeyBindingData _moveRight = new CKeyBindingData("<Keyboard>/d");
    [SerializeField] private CKeyBindingData _attack = new CKeyBindingData("<Mouse>/leftButton");
    [SerializeField] private CKeyBindingData _specialAttack = new CKeyBindingData("<Mouse>/rightButton");
    [SerializeField] private CKeyBindingData _dash = new CKeyBindingData("<Keyboard>/space");
    [SerializeField] private CKeyBindingData _rapidCasting1 = new CKeyBindingData("<Keyboard>/1");
    [SerializeField] private CKeyBindingData _rapidCasting2 = new CKeyBindingData("<Keyboard>/2");
    [SerializeField] private CKeyBindingData _interaction = new CKeyBindingData("<Keyboard>/f");
    [SerializeField] private CKeyBindingData _usePotion = new CKeyBindingData("<Keyboard>/r");
    [SerializeField] private CKeyBindingData _openMap = new CKeyBindingData("<Keyboard>/tab");
    [SerializeField] private CKeyBindingData _inventory = new CKeyBindingData("<Keyboard>/v");
    [SerializeField] private CKeyBindingData _playerStats = new CKeyBindingData("<Keyboard>/c");
    [SerializeField] private CKeyBindingData _talent = new CKeyBindingData("<Keyboard>/p");
    [SerializeField] private CKeyBindingData _presetSetting = new CKeyBindingData("<Keyboard>/b");
    [SerializeField] private CKeyBindingData _chatting = new CKeyBindingData("<Keyboard>/enter");
    [SerializeField] private CKeyBindingData _reloading = new CKeyBindingData("<Keyboard>/g");
    [SerializeField] private CKeyBindingData _ping = new CKeyBindingData("<Mouse>/middleButton");
    [SerializeField] private CKeyBindingData _choosePotion = new CKeyBindingData("<Keyboard>/backquote");
    [SerializeField] private CKeyBindingData _before = new CKeyBindingData("<Keyboard>/q");
    [SerializeField] private CKeyBindingData _next = new CKeyBindingData("<Keyboard>/e");


    public EScene CurrentScene => _currentScene;
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


    public void SetCurrentScene(EScene currentScene)
    {
        _currentScene = currentScene;
    }

    public void SetSkipBoss(bool skipboss)
    {
        _skipBoss = skipboss;
    }

    public void SetCameraShakeMultiplier(float multiplier)
    {
        _cameraShakeMultiplier = multiplier;
    }

    public void SetDisplayEffect(bool skipboss)
    {
        _displayEffect = skipboss;
    }


    // 사운드 설정
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

    // 화면 설정 UI에서 호출할때 수정 필요
    public void SetDisplay(int width, int height, FullScreenMode screenMode)
    {
        if (width <= 0 || height <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(width), "해상도 높이 너비 오류");
        }

        _width = width;
        _height = height;
        _screenMode = screenMode;
    }



    // 키 설정 조회
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

    // 키 설정 변경
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
