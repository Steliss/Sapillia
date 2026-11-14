using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CPotionList : MonoBehaviour, IInputCommand
{
    [SerializeField] private Image _slotImage;
    [SerializeField] private TextMeshProUGUI _countText;

    private List<CInventorySlotData> _inventorySlots;
    private List<CInventorySlotData> _potionSlots;

    private CInputCommandHub _input;
    private CDataManger _data;
    private CInventoryData _inventoryData;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private List<CPotionSO> _potionList = new();  // 보관 장소

    private int _index = 0; // 선택 인덱스



    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.ChoosePotion, this);

            CLog.Log("CPotionList _inputTask OnEnable 작업 완료");
        });
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _inventoryData = _data.InventoryData.GetInventoryData();
            _inventorySlots = _inventoryData.InventorySlots;
            _potionSlots = _inventoryData.PotionSlots;

            _inventoryData.OnInventoryChanged += Refresh;

            // 이미 가지고 있는 포션도 최초 목록에 반영
            Refresh();

            CLog.Log("CPotionList _dataTask OnEnable 작업 완료");
        });

    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _dataTask.Cancel();

        _input.Unregister(EKeyAction.ChoosePotion, this);

        _inventoryData.OnInventoryChanged -= Refresh;
    }

    public void Execute()
    {
        if (_potionList.Count == 0)
        {
            return;
        }

        _index = (_index + 1) % _potionList.Count;

        RefreshUI();
    }

    /// <summary>
    /// 현재 포션 사용
    /// </summary>
    /// <param name="player"></param>
    public void UseCurrentPotion(CPlayerGamingDataSO player)
    {
        if (_inventoryData == null || _potionList.Count == 0 || _index < 0 || _index >= _potionList.Count)
        {
            return;
        }

        CPotionSO potion = _potionList[_index];

        if (potion == null)
        {
            return;
        }

        // 포션 전용 슬롯 → 일반 인벤토리 순서로 검색
        CInventorySlotData slot = FindPotionSlot(_potionSlots, potion.ItemID);

        if (slot == null)
        {
            slot = FindPotionSlot(_inventorySlots, potion.ItemID);
        }

        if (slot == null)
        {
            Refresh();
            return;
        }

        bool applied = false;

        foreach (CItemEffectSO effect in potion.Effects)
        {
            if (effect == null)
            {
                continue;
            }

            effect.Apply(player);
            applied = true;
        }

        if (!applied)
        {
            return;
        }

        slot.Count--;

        if (slot.Count <= 0)
        {
            slot.Clear();
        }

        _inventoryData.NotifyInventoryChanged();
    }


    // 실제 소비할 슬롯 검색
    private CInventorySlotData FindPotionSlot(List<CInventorySlotData> slots, int itemID)
    {
        foreach (CInventorySlotData slot in slots)
        {
            if (slot.IsEmpty || slot.Count <= 0)
            {
                continue;
            }

            if (slot.ItemID == itemID && slot.ItemSO is CPotionSO)
            {
                return slot;
            }
        }

        return null;
    }

    // 포션 목록 재생생
    private void Refresh()
    {
        int selectedID = 0;
        int selectedIndex = -1;

        if (_index >= 0 && _index < _potionList.Count)
        {
            CPotionSO selected = _potionList[_index];

            if (selected != null)
            {
                selectedID = selected.ItemID;
            }
        }

        _potionList.Clear();

        CollectPotions(_potionSlots);
        CollectPotions(_inventorySlots);


        // 기존에 선택한 포션 선택 유지
        for (int i = 0; i < _potionList.Count; i++)
        {
            if (_potionList[i].ItemID == selectedID)
            {
                selectedIndex = i;

                break;
            }
        }

        _index = selectedIndex >= 0 ? selectedIndex : 0;
        RefreshUI();
    }


    // 선택된 포션 이미지와 전체 보유 수량 표시
    private void RefreshUI()
    {
        if (_potionList.Count == 0)
        {
            _slotImage.sprite = null;
            _slotImage.enabled = false;
            _countText.text = "";
            return;
        }

        CPotionSO potion = _potionList[_index];

        _slotImage.sprite = potion.Icon;
        _slotImage.enabled = true;
        _countText.text = $"{GetPotionCount(potion.ItemID)}";
    }

    // 인벤토리내 포션 개수 합산
    private int GetPotionCount(int itemID)
    {
        int count = 0;

        foreach (CInventorySlotData slot in _potionSlots)
        {
            if (slot.ItemID == itemID)
            {
                count += slot.Count;
            }
        }

        foreach (CInventorySlotData slot in _inventorySlots)
        {
            if (slot.ItemID == itemID)
            {
                count += slot.Count;
            }
        }

        return count;
    }


    // 보유한 포션을 리스트에 등록
    private void CollectPotions(List<CInventorySlotData> slots)
    {
        if (slots == null)
        {
            return;
        }

        foreach (CInventorySlotData slot in slots)
        {
            if (slot.IsEmpty || slot.Count <= 0)
            {
                continue;
            }

            if (slot.ItemSO is CPotionSO potion)
            {
                if (!_potionList.Contains(potion))
                {
                    _potionList.Add(potion);
                }
            }
        }
    }
}