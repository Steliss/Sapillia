using System;
using UnityEngine;


//SceneListSO 관련 필드
public enum EScene
{
    Menu,
    Town,
    stage1,
    stage2,
    stage3
}


[System.Serializable]
public class CSceneInfo
{
    [SerializeField] private EScene _sceneID;
    [SerializeField] private string _sceneName;

    [Header("페이드 설정")]
    [SerializeField] private bool _useFade = true;
    [SerializeField] private float _fadeOutDuration = 0.5f;
    [SerializeField] private float _fadeInDuration = 0.5f;
    [SerializeField] private Color _fadeColor = Color.black;

    [SerializeField] private GameObject _backGroundPrefab = null;

    public EScene SceneID => _sceneID;
    public string SceneName => _sceneName;

    public bool UseFade => _useFade;
    public float FadeOutDuration => _fadeOutDuration;
    public float FadeInDuration => _fadeInDuration;
    public Color FadeColor => _fadeColor;

    public GameObject BackGroundPrefab => _backGroundPrefab;
}


// FlowDataSo 관련 필드
public enum EKeyAction
{
    Move,
    MoveUp,
    MoveDown,
    MoveLeft,
    MoveRight,
    Attack,
    SpecialAttack,
    Dash,
    RapidCasting1,
    RapidCasting2,
    Interaction,
    UsePotion,
    OpenMap,
    Inventory,
    PlayerStats,
    Talent,
    PresetSetting,
    Chatting,
    Reloading,
    Ping,
    ChoosePotion,
    Before,
    Next
}

public enum EKeySlot
{
    Primary,
    Secondary
}

[Serializable]
public class CKeyBindingData
{
    public string _actionName;

    public string _primary;
    public string _secondary;

    // 키 바인드로 인한 추가
    public int _primaryIndex;
    public int _secondaryIndex;

    public CKeyBindingData(string primary)
    {
        _primary = primary;
        _secondary = "";
    }
}


// LocalizationSO 관련 필드 
[Serializable]
public class CLocalizationEntry
{
    public string key;
    public string value;
}

[Serializable]
public class CLocalizationData
{
    public CLocalizationEntry[] items;
    public CLocalizationEntry[] talk;
    public CLocalizationEntry[] scene;
}
