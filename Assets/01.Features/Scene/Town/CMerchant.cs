using System.Collections.Generic;
using UnityEngine;

public enum EMarchant
{
    None,
    Cow,
    Rat,
    Turtle
}


public class CMerchant : CTriggerPlayer, IInputCommand
{
    [SerializeField] private GameObject _shopUI;
    [SerializeField] private CInventoryUI _inventoryUI;
    [SerializeField] private CItemDatabaseSO _itemData;
    [SerializeField] private CMerchantUI _merchantUI;
    [SerializeField] private CMerchantData[] _merchantDatas;
    [SerializeField] private Animator _controller;

    private CInputCommandHub _input;
    private CDataManger _data;
    private CInventoryData _inventoryData;

    private CUniTaskOnes _inputTask = new();
    private CUniTaskOnes _dataTask = new();

    private List<CInventorySlotData> _shopSlots = new();


    private bool _isActive;
    private bool _isNear;

    private int _bargain = 0; // 계산용

    private CMerchantData _currentMerchant;

    public bool IsActive => _isActive;
    public int Bargain => _bargain;
    public List<CInventorySlotData> ShopSlots => _shopSlots;
    public CMerchantData CurrentMerchant => _currentMerchant;

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
        CPlayerGamingDataSO playerdata = _data.PlayerData.GetPlayerData();
        _bargain = playerdata.Stats.Bargain - _currentMerchant.Bargain;

        if (_isNear && _inventoryData != null)
        {
            ActiveUI(true);
        }
    }

    // 상점과 인벤토리 열고 닫기
    public void ActiveUI(bool active)
    {
        _isActive = active;

        if (active)
        {
            _merchantUI.RebuildSlots();
        }

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
        if (_merchantDatas == null || _merchantDatas.Length == 0)
        {
            CLog.Warn("상인 데이터가 없음");
            return;
        }

        _currentMerchant = _merchantDatas[Random.Range(0, _merchantDatas.Length)];

        if (_currentMerchant == null)
        {
            CLog.Warn("선택된 상인 데이터가 비어 있음");
            return;
        }

        _controller.runtimeAnimatorController = _currentMerchant.Controller;

        _shopSlots.Clear();

        if (_currentMerchant.Type == EMarchant.Cow)
        {
            AddShopItem(2001,3); // 기본 포션 
            AddShopItem(EItemType.Potion);

            AddShopItem(EItemType.Artifact);
            AddShopItem(EItemType.Artifact);
            AddShopItem(EItemType.Artifact);

            AddShopItem(EItemType.Tablet);
        }
        else if (_currentMerchant.Type == EMarchant.Rat)
        {
            AddShopItem(2001, 3); // 기본 포션 
            AddShopItem(EItemType.Potion);
            AddShopItem(EItemType.Potion);

            AddShopItem(EItemType.Artifact);

            AddShopItem(EItemType.Tablet);
            AddShopItem(EItemType.Tablet);
        }
        else if (_currentMerchant.Type == EMarchant.Turtle)
        {
            AddShopItem(2001, 3); // 기본 포션 

            AddShopItem(EItemType.Artifact);
            AddShopItem(EItemType.Artifact);
            AddShopItem(EItemType.Artifact);

            AddShopItem(EItemType.Tablet);
            AddShopItem(EItemType.Tablet);
        }
        else
        {
            AddShopItem(2001, 3); // 기본 포션 
        }
    }

    // 상점 아이템 생성
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


    private void AddShopItem(int itemID, int count)
    {
        CItemSO item = _itemData.SetItem(itemID);

        if (item == null)
        {
            _shopSlots.Add(new CInventorySlotData());
            return;
        }

        _shopSlots.Add(CreateShopSlot(item, count));
    }


    // 상점 → 플레이어 인벤토리
    public bool TryBuy(CMerchantSlotUI fromSlot, CInventorySlotUI toSlot)
    {
        if (!_isActive || _inventoryData == null || fromSlot == null || toSlot == null)
        {
            return false;
        }

        CInventorySlotData product = fromSlot.SlotData;

        if (product == null || product.IsEmpty || product.ItemSO == null || product.Count <= 0)
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

        CInventorySlotData toData = toList[toIndex];

        // 포션 전용 슬롯에는 포션만 허용
        if (toList == _inventoryData.PotionSlots && product.ItemType != EItemType.Potion)
        {
            return false;
        }

        if (!toData.IsEmpty)
        {
            // 같은 종류의 포션만 합산 가능
            if (!(product.ItemSO is CPotionSO potion) || product.ItemID != toData.ItemID)
            {
                return false;
            }

            // 최대 수량이면 구매 불가
            if (toData.Count >= potion.MaxStack)
            {
                return false;
            }

            if (!TradeItem(product, true))
            {
                return false;
            }

            toData.Count++;
        }
        else
        {
            if (!TradeItem(product, true))
            {
                return false;
            }

            // 포션은 1개 복사, 다른 아이템은 기존 데이터 이동
            toList[toIndex] = product.ItemSO is CPotionSO ? CreateShopSlot(product.ItemSO, 1) : product;
        }

        if (product.ItemSO is CPotionSO)
        {
            // 포션은 구매한 1개만 차감
            product.Count--;

            if (product.Count == 0)
            {
                _shopSlots.RemoveAt(shopIndex);
            }
        }
        else
        {
            _shopSlots.RemoveAt(shopIndex);
        }

        toSlot.BindSlot(toList[toIndex]);

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

        if (product == null || product.IsEmpty || product.ItemSO == null || product.Count <= 0)
        {
            return false;
        }

        if (!TradeItem(product, false))
        {
            return false;
        }

        if (product.ItemSO is CPotionSO potion)
        {
            // 같은 포션 중 수량 여유가 있는 슬롯 검색
            CInventorySlotData shopPotion = _shopSlots.Find(slot =>
                !slot.IsEmpty &&
                slot.ItemID == product.ItemID &&
                slot.Count < potion.MaxStack);

            if (shopPotion != null)
            {
                shopPotion.Count++;
            }
            else
            {
                _shopSlots.Add(CreateShopSlot(product.ItemSO, 1));
            }

            // 플레이어 포션은 1개만 차감
            product.Count--;

            if (product.Count == 0)
            {
                product.Clear();
            }
        }
        else
        {
            // 다른 아이템은 기존 데이터 이동
            _shopSlots.Add(product);
            fromList[fromIndex] = new CInventorySlotData();
        }

        fromSlot.BindSlot(fromList[fromIndex]);

        _inventoryUI.HideItemInfo();
        _merchantUI.RebuildSlots();
        _inventoryData.NotifyInventoryChanged();

        return true;
    }

    // 아이템 1개 가격으로 거래
    private bool TradeItem(CInventorySlotData product, bool isBuy)
    {
        EArtifactGrade grade = product.ItemSO.Grade;
        CPlayerGamingDataSO playerdata = _data.PlayerData.GetPlayerData();

        int price = _itemData.GetPrice(grade);

        if (price <= 0)
        {
            return false;
        }

        // 구매 기본 100%, 판매 기본 50%
        float priceRate = isBuy ? 1f - _bargain * 0.01f : 0.5f + _bargain * 0.004f;

        priceRate = Mathf.Max(0f, priceRate);
        price = Mathf.CeilToInt(price * priceRate);

        if (isBuy)
        {
            if (!playerdata.TrySpendMoney(price))
            {
                return false;
            }

            _currentMerchant.AddMoney(price);
        }
        else
        {
            if (_currentMerchant.Money < price)
            {
                return false;
            }

            _currentMerchant.AddMoney(-price);
            playerdata.ToMarchantAddMoney(price);
        }

        return true;
    }
}