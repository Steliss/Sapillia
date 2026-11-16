using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


public class CMerchantSlotUI : CInventorySlotUI, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private TextMeshProUGUI _nameText;
    [SerializeField] private CItemDatabaseSO _itemdata;
    [SerializeField] private CMerchant _merchant;

    private CMerchantUI _merchantUI;

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

    public void SetMerchantUI(CMerchantUI merchantUI)
    {
        _merchantUI = merchantUI;
    }


    public override void OnPointerEnter(PointerEventData eventData)
    {
        _merchantUI.ShowItemInfo(this);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        _merchantUI.HideItemInfo();
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

        int price = Mathf.CeilToInt(_itemdata.GetPrice(SlotData.ItemSO.Grade) * (Mathf.Max(0f, 1f - _merchant.Bargain * 0.01f)));

        _nameText.text = SlotData.ItemSO.name;

        if(SlotData.ItemType == EItemType.Potion)
        {
            _countText.text = $"{SlotData.Count}  / {price}";
        }
        else
        {
            _countText.text = $"{price}";
        }
    }




}

