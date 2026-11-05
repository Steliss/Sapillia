using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[Serializable]
public class CPlayerButton
{
    [SerializeField] private Button _button;
    [SerializeField] private Sprite _originImage;
    [SerializeField] private Sprite _changeImage;


    public Button Button
    {
        get => _button;
        set => _button = value;
    }
    public Sprite OriginImage
    {
        get => _originImage;
        set => _originImage = value;
    }

    public Sprite ChangeImage
    {
        get => _changeImage;
        set => _changeImage = value;
    }
}



public class CPlayerButtonUI : MonoBehaviour
{
    [SerializeField] private CPlayerStatusUI _state;
    [SerializeField] private CInventoryUI _inventory;

    [SerializeField] private CPlayerButton _InventoryButton;
    [SerializeField] private CPlayerButton _StatusButton;
    //[SerializeField] private CPlayerButton _Button;
    //[SerializeField] private CPlayerButton _Button;

    [SerializeField] private TextMeshProUGUI _dice;
    [SerializeField] private TextMeshProUGUI _sapphire;
    [SerializeField] private TextMeshProUGUI _money;

    private CDataManger _data;
    private CPlayerGamingDataSO _playerData;

    private CUniTaskOnes _dataTask = new CUniTaskOnes();


    private void OnEnable()
    {
        _InventoryButton.Button.onClick.AddListener(InventoryOpen);
        _StatusButton.Button.onClick.AddListener(StateOpen);

        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _playerData = _data.PlayerData.GetPlayerData();
            _playerData.OnMoneyChanged += UpdatemoneyText;
            _playerData.OnDiceChanged += UpdateDiceText;
            _data.CurrentFlow.OnSapphireChanged += UpdateSapphireText;

            UpdatemoneyText(_playerData.Money);
            UpdateDiceText(_playerData.Dice);
            UpdateSapphireText(_data.CurrentFlow.Sapphire);
            CLog.Log("CPlayerButtonUI _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _InventoryButton.Button.onClick.RemoveListener(InventoryOpen);
        _StatusButton.Button.onClick.RemoveListener(StateOpen);

        _playerData.OnMoneyChanged -= UpdatemoneyText;
        _playerData.OnDiceChanged -= UpdateDiceText;
        _data.CurrentFlow.OnSapphireChanged -= UpdateSapphireText;

        _dataTask.Cancel();
    }

    private void Awake()
    {
        SetButton(_InventoryButton);
        SetButton(_StatusButton);
    }

    // 마우스 이동에 따른 버튼 이미지 변경
    private void SetButton(CPlayerButton data)
    {
        Button button = data.Button;
        Image image = button.GetComponent<Image>();

        Navigation navigation = button.navigation;
        navigation.mode = Navigation.Mode.None;
        button.navigation = navigation;

        image.sprite = data.OriginImage;
        image.raycastTarget = true;
        button.targetGraphic = image;

        button.transition = Selectable.Transition.SpriteSwap;

        SpriteState sprites = button.spriteState;
        sprites.highlightedSprite = data.ChangeImage;
        sprites.pressedSprite = data.ChangeImage;

        button.spriteState = sprites;
    }

    private void StateOpen()
    {
        _state.Execute();
    }

    private void InventoryOpen()
    {
        _inventory.Execute();
    }


    private void UpdateDiceText(int dice)
    {
        _dice.text = $"{dice}";
    }

    private void UpdatemoneyText(int money)
    {
        _money.text = $"{money}";
    }

    private void UpdateSapphireText(int sapphire)
    {
        _sapphire.text = $"{sapphire}";
    }
}
