using UnityEngine.EventSystems;
using UnityEngine.UI;


// ScrollRect 재정의를 통한 드래그 기능 활성화
public class CScrollRect : ScrollRect
{
    public override void OnInitializePotentialDrag(PointerEventData eventData) { }

    public override void OnBeginDrag(PointerEventData eventData) { }

    public override void OnDrag(PointerEventData eventData) { }

    public override void OnEndDrag(PointerEventData eventData) { }

    public override void OnScroll(PointerEventData eventData) { }
}
