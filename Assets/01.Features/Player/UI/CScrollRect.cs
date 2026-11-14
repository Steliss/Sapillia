using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


// ScrollRect 재정의를 통한 드래그 기능 활성화
public class CScrollRect : ScrollRect
{
    private CInventoryDrag _inventoryDrag;

    protected override void Awake()
    {
        // ScrollRect awake 실행
        base.Awake();

        _inventoryDrag = GetComponentInParent<CInventoryDrag>();
    }


    public override void OnInitializePotentialDrag(PointerEventData eventData) { }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        _inventoryDrag.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        _inventoryDrag.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        _inventoryDrag.OnEndDrag(eventData);
    }


    public override void OnScroll(PointerEventData eventData) { }
}
