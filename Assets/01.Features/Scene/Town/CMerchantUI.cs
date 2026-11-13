using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class CMerchantUI : MonoBehaviour
{
    [SerializeField] private CMerchantSlotUI _slotPrefab;
    [SerializeField] private Transform _slotRoot;
    [SerializeField] private CInventoryUI _inventoryUI;
    [SerializeField] private CInventoryDrag _drag;

    [SerializeField] private TextMeshProUGUI _merchantText;
    [SerializeField] private TextMeshProUGUI _playerText;

    private List<CMerchantSlotUI> _slotUI = new();
    private List<CInventorySlotData> _shopSlots;

    // 상인의 상품 목록 연결
    public void BindSlots(List<CInventorySlotData> shopSlots)
    {
        _shopSlots = shopSlots;
        RebuildSlots();
    }

    // 상품 목록에 맞춰 슬롯 다시 생성
    public void RebuildSlots()
    {
        ClearSlots();

        if (_shopSlots == null)
        {
            return;
        }

        for (int i = 0; i < _shopSlots.Count; i++)
        {
            CMerchantSlotUI slot = Instantiate(_slotPrefab, _slotRoot);

            // 생성한 슬롯에 설명 UI와 드래그 연결
            slot.SetInventoryUI(_inventoryUI);
            slot.SetDrag(_drag);

            slot.BindSlot(_shopSlots[i]);
            _slotUI.Add(slot);
        }
    }

    // 기존 슬롯 정리
    private void ClearSlots()
    {
        foreach (CMerchantSlotUI slot in _slotUI)
        {
            slot.gameObject.SetActive(false);
            Destroy(slot.gameObject);
        }

        _slotUI.Clear();
    }

    public void MoneyDisplay(int merchant, int player)
    {
        _merchantText.text = merchant.ToString();
        _playerText.text = player.ToString();
    }
}