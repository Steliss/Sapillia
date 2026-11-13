using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class CMerchantSlotUI : CInventorySlotUI, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TextMeshProUGUI _nameText;

    private int _price;

    private CInventoryDrag _drag;

    public void SetDrag(CInventoryDrag drag)
    {
        _drag = drag;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _drag.OnBeginDrag(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        _drag.OnDrag(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _drag.OnEndDrag(eventData);
    }


    public void SetPrice(int price)
    {
        _price = price;
        Refresh();
    }

    public override void Refresh()
    {
        // 부모의 아이콘·수량 갱신
        base.Refresh();

        if (SlotData == null || SlotData.IsEmpty || SlotData.ItemSO == null)
        {
            _nameText.text = "";
            _countText.text = "";
            return;
        }

        _nameText.text = SlotData.ItemSO.name;
        _countText.text = _price.ToString();
    }
}

