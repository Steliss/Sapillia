using System.Collections.Generic;
using UnityEngine;


public class CMerchant : CTriggerPlayer, IInputCommand
{
    [SerializeField] private GameObject _shopUI;
    [SerializeField] private CInventoryUI _inventoryUI;
    [SerializeField] private CItemDatabaseSO _itemData;

    private CInputCommandHub _input;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();


    private List<CInventorySlotData> _shopSlots = new(); // 상품 데이터: 포션 1칸, 아티팩트 3칸, 석판 1칸

    private bool _isActive;
    private bool _isNear;

    private int _money = 2000; 

    public bool IsActive => _isActive; 
    public int Money => _money;

    public List<CInventorySlotData> ShopSlots => _shopSlots;

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.Interaction, this);
            CLog.Log("CMerchant _inputTask OnEnable 작업 완료");
        });

    }

    private void OnDisable()
    {
        _inputTask?.Cancel();

        _input?.Unregister(EKeyAction.Interaction, this);
    }

    private void Start()
    {
        CreateShoppingList();
    }

    protected override void OnPlayerEnter()
    {
        _isNear = true;
    }

    protected override void OnPlayerExit()
    {
        _isNear = false;
    }

    
    public void Execute()
    {
        if (_isNear)
        {
            ActiveUI(true);
        }
    }

    public void ActiveUI(bool active)
    {
        _isActive = active;
        _inventoryUI.ActiveUI(active);
        _shopUI.SetActive(active);
    }

    // 아이템 원본으로 상점 상품 생성
    private CInventorySlotData CreateShopSlot(CItemSO item, int count = 1)
    {
        return new CInventorySlotData
        {
            ItemID = item.ItemID,
            Count = count,
            ItemType = item.Type,
            EnhanceLevel = 0,
            Rotation = ETabletRotation.None,
            ItemSO = item
        };
    }

    private void CreateShoppingList()
    {
        _shopSlots.Clear();

        AddShopItem(EItemType.Potion);
        AddShopItem(EItemType.Artifact);
        AddShopItem(EItemType.Artifact);
        AddShopItem(EItemType.Artifact);
        AddShopItem(EItemType.Tablet);
    }

    private void AddShopItem(EItemType type)
    {
        CItemSO item = _itemData.GetRandomItem(type);

        // 해당 종류의 아이템이 없으면 빈 슬롯 추가
        if (item == null)
        {
            _shopSlots.Add(new CInventorySlotData());
            return;
        }

        _shopSlots.Add(CreateShopSlot(item));
    }

    public void AddMoney(int value)
    {
        _money += value;
    }
}
