using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CInventoryUI : MonoBehaviour, IInputCommand, IESCCommand
{
    [SerializeField] private CMerchant _merchant;
    [SerializeField] private CESCUI _esc;
    [SerializeField] private GameObject _UICanvas;
    [SerializeField] private CInventoryDrag _drag;
    [SerializeField] private CItemDatabaseSO _itemdata;

    [SerializeField] private GameObject _InventoryInfo;
    [SerializeField] private Image _infoImage;
    [SerializeField] private TextMeshProUGUI _infoTitle;
    [SerializeField] private TextMeshProUGUI _infoDesc;

    [SerializeField] private CInventorySynergyUI _synergyUI;  // 시너지 UI 추가
    [Header("슬롯 프리팹")]
    [SerializeField] private CInventorySlotUI _slotPrefab;

    [Header("슬롯 생성 위치")]
    [SerializeField] private Transform _inventoryRoot;
    [SerializeField] private Transform _potionRoot;
    [SerializeField] private Transform _beltRoot;

    // 생성한 슬롯 UI 참조
    private readonly List<CInventorySlotUI> _inventorySlotUI = new();
    private readonly List<CInventorySlotUI> _potionSlotUI = new();
    private readonly List<CInventorySlotUI> _beltSlotUI = new();

    private CDataManger _data;
    private CLocalizationManager _localization;
    private CInputCommandHub _input;
    private CInventoryData _inventoryData;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();
    private CUniTaskOnes _localizationTask = new CUniTaskOnes();

    private CRewardEnchant _currentEnchant;

    private bool _isEnchantMode;
    private bool _isActive;

    public bool IsActive => _isActive;
    public bool IsEnchantMode => _isEnchantMode;

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.Inventory, this);

            CLog.Log("CInventoryUI _inputTask OnEnable 작업 완료");
        });

        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);

            CLog.Log("CInventoryUI _localizationTask OnEnable 작업 완료");
        });

        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _inventoryData = _data.InventoryData.GetInventoryData();

            
            CreateInventory();

            _inventoryData.OnInventoryChanged += RefreshInventoryUI;

            CLog.Log("CInventoryUI _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _dataTask.Cancel();
        _localizationTask.Cancel();
                
        _inventoryData.OnInventoryChanged -= RefreshInventoryUI;

        _input.Unregister(EKeyAction.Inventory, this);
        _input.UnregisterOpenedUI(this);
    }



    // 인벤토리 열고 닫기
    public void Execute()
    {
        if (_esc.IsActive || _merchant.IsActive)
        {
            return;
        }

        RefreshInventoryUI();
        ActiveUI(!_isActive);
    }

    public void ExecuteESC()
    {
        if (_isActive)
        {
            ActiveUI(false);
        }
    }

    public void ActiveUI(bool active)
    {
        _isActive = active;

        HideItemInfo();

        if (!active)
        {
            _drag.CancelDrag();
            _isEnchantMode = false;
            _currentEnchant = null;
        }

        _data.SaveGame(_data.CurrentFlow.SaveSlot);
        _UICanvas.SetActive(active);

        if (active)
        {
            _input.RegisterOpenedUI(this);
        }
        else
        {
            _input.UnregisterOpenedUI(this);
        }
    }

    // UI 슬롯 최초 생성
    private void CreateInventory()
    {
        CreateSlots(
            _inventoryData.InventorySlots,
            _inventoryRoot,
            _inventorySlotUI
        );

        CreateSlots(
            _inventoryData.PotionSlots,
            _potionRoot,
            _potionSlotUI
        );

        CreateSlots(
            _inventoryData.ExtraInventorySlots,
            _beltRoot,
            _beltSlotUI
        );

        _slotPrefab.gameObject.SetActive(false);
    }

    // 데이터 개수만큼 슬롯 생성 및 연결
    private void CreateSlots(
        List<CInventorySlotData> dataList,
        Transform root,
        List<CInventorySlotUI> slotUIList)
    {
        for (int i = 0; i < dataList.Count; i++)
        {
            CInventorySlotUI slot = Instantiate(_slotPrefab, root);

            slot.SetInventoryUI(this);
            slot.BindSlot(dataList[i]);

            slotUIList.Add(slot);
            slot.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 인벤토리 슬롯 추가 후 UI 갱신
    /// </summary>
    public void AddSlots()
    {
        for (int i = _inventorySlotUI.Count; i < _inventoryData.InventorySlots.Count; i++)
        {
            CInventorySlotUI slot = Instantiate(_slotPrefab, _inventoryRoot);

            slot.SetInventoryUI(this);
            slot.BindSlot(_inventoryData.InventorySlots[i]);

            _inventorySlotUI.Add(slot);
            slot.gameObject.SetActive(true);
        }
    }


    /// <summary>
    /// 출발과 도착칸의 데이터를 바꾸고 화면에서 갱신
    /// </summary>
    /// <param name="fromSlot"></param>
    /// <param name="toSlot"></param>
    /// <returns></returns>
    public void TryMoveSlot(CInventorySlotUI fromSlot, CInventorySlotUI toSlot)
    {
        if (!_isActive || _isEnchantMode || fromSlot == toSlot)
        {
            return;
        }

        if (!TryFindSlot(fromSlot, out var fromList, out int fromIndex) || !TryFindSlot(toSlot, out var toList, out int toIndex))
        {
            return;
        }

        CInventorySlotData fromData = fromList[fromIndex];
        CInventorySlotData toData = toList[toIndex];

        if (fromData.IsEmpty)
        {
            return;
        }

        // 포션 슬롯에는 포션만 배치 가능
        if (toList == _inventoryData.PotionSlots && fromData.ItemType != EItemType.Potion)
        {
            return;
        }

        // 교환으로 포션 슬롯에 들어올 아이템도 검사
        if (fromList == _inventoryData.PotionSlots && !toData.IsEmpty && toData.ItemType != EItemType.Potion)
        {
            return;
        }

        // 같은 종류의 포션이면 수량 합산
        if (fromData.ItemSO is CPotionSO potion && !toData.IsEmpty && fromData.ItemID == toData.ItemID)
        {
            int remainingSpace = Mathf.Max(0, potion.MaxStack - toData.Count);
            int moveCount = Mathf.Min(fromData.Count, remainingSpace);

            if (moveCount == 0)
            {
                return;
            }

            toData.Count += moveCount;
            fromData.Count -= moveCount;

            if (fromData.Count == 0)
            {
                fromData.Clear();
            }
        }
        else
        {
            // 다른 종류 또는 빈 슬롯이면 기존처럼 교환
            fromList[fromIndex] = toData;
            toList[toIndex] = fromData;
        }

        fromSlot.BindSlot(fromList[fromIndex]);
        toSlot.BindSlot(toList[toIndex]);

        _inventoryData.NotifyInventoryChanged();

        ShowItemInfo(toSlot);
    }

    // 데이터 위치 찾기 
    // 슬롯의 위치를 기반으로 List에서 인덱스를 비교 리스트값 반환
    public bool TryFindSlot(
        CInventorySlotUI slot,
        out List<CInventorySlotData> dataList,
        out int index)
    {
        index = _inventorySlotUI.IndexOf(slot);
        if (index >= 0)
        {
            dataList = _inventoryData.InventorySlots;
            return true;
        }

        index = _potionSlotUI.IndexOf(slot);
        if (index >= 0)
        {
            dataList = _inventoryData.PotionSlots;
            return true;
        }

        index = _beltSlotUI.IndexOf(slot);
        if (index >= 0)
        {
            dataList = _inventoryData.ExtraInventorySlots;
            return true;
        }

        dataList = null;
        return false;
    }


    /// <summary>
    /// 인벤토리 슬롯 UI 갱신
    /// </summary>
    private void RefreshInventoryUI()
    {
        foreach (CInventorySlotUI slot in _inventorySlotUI)
        {
            slot.Refresh();
        }

        foreach (CInventorySlotUI slot in _potionSlotUI)
        {
            slot.Refresh();
        }

        foreach (CInventorySlotUI slot in _beltSlotUI)
        {
            slot.Refresh();
        }
        _synergyUI?.SetSynergy(_inventoryData.SynergyCounts);
    }

    /// <summary>
    /// 마우스를 올린 슬롯의 아이템 정보 표시
    /// </summary>
    public void ShowItemInfo(CInventorySlotUI slot)
    {
        if (!_isActive || _localization == null || !TryFindSlot(slot, out var dataList, out int index))
        {
            HideItemInfo();
            return;
        }

        CInventorySlotData data = dataList[index];

        if (data.IsEmpty || data.ItemSO == null || data.ItemID == 0)
        {
            HideItemInfo();
            return;
        }

        CItemSO item = data.ItemSO;

        _infoImage.sprite = item.Icon;
        _infoTitle.text = _localization.GetText(item.NameKey);

        if (item is CArtifactSO artifact)
        {
            int totalLevel = data.EnhanceLevel + data.InchantLevel;

            _infoDesc.text = _localization.GetText(item.DescriptionKey, artifact.GetDescriptionValues(totalLevel));
        }
        else
        {
            _infoDesc.text = _localization.GetText(item.DescriptionKey, item.GetDescriptionValues());
        }


        if (_merchant.IsActive)
        {
            int price = Mathf.CeilToInt(_itemdata.GetPrice(item.Grade) * Mathf.Max(0f, 0.5f + _merchant.Bargain * 0.004f));

            _infoDesc.text += "\n\n\n" + _localization.GetText("scene.sellprice.text") + price;
        }
        else
        {
            _infoDesc.text += "\n\n" + _localization.GetText(item.StoryKey);
        }
            _InventoryInfo.SetActive(true);
    }

    /// <summary>
    /// 아이템 정보 숨기기
    /// </summary>
    public void HideItemInfo()
    {
        _InventoryInfo.SetActive(false);
    }



    /// <summary>
    /// 인챈트 모드 활성화 및 비활성화
    /// </summary>
    public void SetEnchantMode(bool active)
    {
        _isEnchantMode = active;

        if (active)
        {
            RefreshInventoryUI();
            ActiveUI(true);
        }
    }



    /// <summary>
    /// 인챈트 모드에서 선택한 아티팩트 강화
    /// </summary>
    public void TryEnchantSlot(CInventorySlotUI slotUI)
    {
        if (!_isEnchantMode || !_isActive || _inventoryData == null || _currentEnchant == null || _currentEnchant.RemainCount <= 0)
        {
            return;
        }

        if (!TryFindSlot(slotUI, out var dataList, out int index))
        {
            return;
        }

        if (dataList != _inventoryData.InventorySlots)
        {
            return;
        }

        CInventorySlotData slotData = dataList[index];


        bool success = _inventoryData.TryEnchantArtifact(slotData);

        if (success)
        {
            Debug.Log($"인챈트 성공! 아이템 ID: {slotData.ItemID}, 인챈트 레벨: {slotData.InchantLevel}");

            _currentEnchant?.OnEnchantSuccess();
        }
        else
        {
            Debug.Log("인챈트 실패: 강화할 수 없는 아티팩트입니다.");
        }
    }


    public void StartEnchant(CRewardEnchant enchant)
    {
        if (enchant == null || enchant.RemainCount <= 0)
        {
            return;
        }

        _currentEnchant = enchant;

        SetEnchantMode(true);
    }

}