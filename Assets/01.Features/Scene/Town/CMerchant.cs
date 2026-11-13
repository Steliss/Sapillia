using System.Collections.Generic;
using UnityEngine;

public class CMerchant : CTriggerPlayer, IInputCommand
{
    [SerializeField] private GameObject _shopUI;
    [SerializeField] private CInventoryUI _inventoryUI;
    [SerializeField] private CItemDatabaseSO _itemData;
    [SerializeField] private CMerchantUI _merchantUI;

    private CInputCommandHub _input;
    private CDataManger _data;
    private CInventoryData _inventoryData;

    private CUniTaskOnes _inputTask = new();
    private CUniTaskOnes _dataTask = new();

    private List<CInventorySlotData> _shopSlots = new();

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

        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _inventoryData = _data.InventoryData.GetInventoryData();
            CPlayerGamingDataSO playerdata = _data.PlayerData.GetPlayerData();

            _merchantUI.MoneyDisplay(Money, playerdata.Money);
            CLog.Log("CMerchant _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _dataTask.Cancel();

        _input?.Unregister(EKeyAction.Interaction, this);
    }

    private void Start()
    {
        CreateShoppingList();
        _merchantUI.BindSlots(_shopSlots);
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
        if (_isNear && _inventoryData != null)
        {
            ActiveUI(true);
        }
    }

    // 상점과 인벤토리 열고 닫기
    public void ActiveUI(bool active)
    {
        _isActive = active;

        _inventoryUI.ActiveUI(active);
        _shopUI.SetActive(active);
    }

    // 아이템 원본으로 상품 슬롯 생성
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

    // 초기 상점 세팅
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

    // 상점 → 플레이어 인벤토리
    public bool TryBuy(CMerchantSlotUI fromSlot, CInventorySlotUI toSlot)
    {
        if (!_isActive || _inventoryData == null || fromSlot == null || toSlot == null)
        {
            return false;
        }

        CInventorySlotData product = fromSlot.SlotData;

        if (product == null || product.IsEmpty || product.ItemSO == null)
        {
            return false;
        }

        int shopIndex = _shopSlots.IndexOf(product);

        // 이 상인이 가진 상품인지 확인
        if (shopIndex < 0)
        {
            return false;
        }

        if (!_inventoryUI.TryFindSlot(toSlot, out var toList, out int toIndex))
        {
            return false;
        }

        // 빈 슬롯에만 구매 가능
        if (!toList[toIndex].IsEmpty)
        {
            return false;
        }

        // 포션 전용 슬롯에는 포션만 허용
        if (toList == _inventoryData.PotionSlots && product.ItemType != EItemType.Potion)
        {
            return false;
        }

        if (!TradeItem(product, true))
        {
            return false;
        }

        // 상품 이동 후 상점 목록에서 제거
        toList[toIndex] = product;
        _shopSlots.RemoveAt(shopIndex);

        toSlot.BindSlot(product);

        _inventoryUI.HideItemInfo();
        _merchantUI.RebuildSlots();
        _inventoryData.NotifyInventoryChanged();

        return true;
    }

    // 플레이어 인벤토리 → 상점
    public bool TrySell(CInventorySlotUI fromSlot)
    {
        if (!_isActive || _inventoryData == null || fromSlot == null)
        {
            return false;
        }

        if (!_inventoryUI.TryFindSlot(fromSlot, out var fromList, out int fromIndex))
        {
            return false;
        }

        CInventorySlotData product = fromList[fromIndex];

        if (product == null || product.IsEmpty || product.ItemSO == null)
        {
            return false;
        }

        if(!TradeItem(product, false))
        {
            return false;
        }

        // 상점에 추가하고 플레이어 슬롯은 빈 슬롯으로 교체
        _shopSlots.Add(product);
        fromList[fromIndex] = new CInventorySlotData();

        fromSlot.BindSlot(fromList[fromIndex]);

        _inventoryUI.HideItemInfo();
        _merchantUI.RebuildSlots();
        _inventoryData.NotifyInventoryChanged();

        return true;
    }


    private bool TradeItem(CInventorySlotData product, bool isBuy)
    {
        EArtifactGrade grade;
        CPlayerGamingDataSO playerdata = _data.PlayerData.GetPlayerData();

        if (product.ItemSO is CPotionSO potion)
        {
            grade = potion.Grade;
        }
        else if (product.ItemSO is CArtifactSO artifact)
        {
            grade = artifact.Grade;
        }
        else if (product.ItemSO is CTabletSO tablet)
        {
            grade = tablet.Grade;
        }
        else
        {
            return false;
        }

        int price = grade switch
        {
            EArtifactGrade.Normal => 100,
            EArtifactGrade.Advanced => 200,
            EArtifactGrade.Rare => 400,
            EArtifactGrade.Legendary => 800,    
            _ => 0
        };

        if (price == 0)
        {
            return false;
        }

        // TODO: 협상력에 따른 가격 조정
        //price *= playerdata.Stats.Bargain;

        if (isBuy)
        {
            if (!playerdata.TrySpendMoney(price))
            {
                return false;
            }

            playerdata.AddMoney(-price);
            AddMoney(price);
        }
        else
        {
            price /= 2;

            if (_money < price)
            {
                return false;
            }

            AddMoney(-price);
            playerdata.AddMoney(price);
        }

        _merchantUI.MoneyDisplay(Money, playerdata.Money);
        return true;
    }

   
}