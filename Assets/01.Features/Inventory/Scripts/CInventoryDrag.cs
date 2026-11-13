using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class CInventoryDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IInputCommand
{
    [SerializeField] private Image _dragIcon;
    [SerializeField] private CInventoryUI _inventoryUI;
    [SerializeField] private RectTransform _UICanvas;
    [SerializeField] private CMerchant _merchant;

    private CInventoryData _inventoryData;
    private CInventorySlotUI _fromSlot;

    private CDataManger _data;
    private CInputCommandHub _input;
    private CUniTaskOnes _dataTask = new CUniTaskOnes();
    private CUniTaskOnes _inputTask = new CUniTaskOnes();

    private int _pointerId;


    private void Awake()
    {
        _dragIcon.raycastTarget = false;
        _dragIcon.preserveAspect = true;
        CancelDrag();
    }

    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _inventoryData = _data.InventoryData.GetInventoryData();
            token.ThrowIfCancellationRequested();

            CLog.Log("CBGMScene _dataTask OnEnable 작업 완료");
        });
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.UsePotion, this);
            token.ThrowIfCancellationRequested();

            CLog.Log("CBGMScene _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        CancelDrag();

        _dataTask.Cancel();
        _inputTask.Cancel();

        _input.Unregister(EKeyAction.UsePotion, this);
    }

    public void Execute()
    {

        if (!_inventoryUI.IsActive || _inventoryData == null || _fromSlot != null)
        {
            return;
        }

        CInventorySlotUI targetSlot = FindMouseSlot();

        if (targetSlot == null)


        {
            return;
        }

        CInventorySlotData slot = targetSlot.SlotData;

        if (slot == null ||
            slot.IsEmpty ||
            slot.ItemSO is not CTabletSO tablet ||
            !tablet.CanRotate)
        {
            return;
        }

        // 슬롯의 회전 상태 변경
        slot.RotateTablet();

        // 변경된 방향으로 석판 효과 재계산 및 변경 알림
        _inventoryData.NotifyInventoryChanged();

        targetSlot.Refresh();
    }

    // 마우스 아래 슬롯 
    private CInventorySlotUI FindMouseSlot()
    {
        if (EventSystem.current == null || Mouse.current == null)
        {
            return null;
        }

        PointerEventData pointer = new PointerEventData(EventSystem.current)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointer, results);

        foreach (RaycastResult result in results)
        {
            CInventorySlotUI slot = FindSlot(result);

            if (slot != null)
            {
                return slot;
            }
        }

        return null;
    }


    // 유니티 함수 드래그 시작시
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!_inventoryUI.IsActive || _fromSlot != null || eventData.button != PointerEventData.InputButton.Left)
        {
            return;
        }

        // 드래그 판정이 난 위치가 아니라 처음 눌렀던 슬롯을 찾음
        CInventorySlotUI fromSlot = FindSlot(eventData.pointerPressRaycast);

        if (fromSlot == null || fromSlot.ItemImage == null)
        {
            return;
        }

        _fromSlot = fromSlot;
        _pointerId = eventData.pointerId;

        _dragIcon.sprite = fromSlot.ItemImage;
        _dragIcon.gameObject.SetActive(true);
        _dragIcon.enabled = true;
        MoveIcon(eventData);
    }

    // 유니티 함수 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        if (_fromSlot != null && eventData.pointerId == _pointerId)
        {
            MoveIcon(eventData);
        }
    }

    // 유니티 함수 드래그 끝
    public void OnEndDrag(PointerEventData eventData)
    {
        if (_fromSlot == null || eventData.pointerId != _pointerId)
        {
            return;
        }

        CInventorySlotUI fromSlot = _fromSlot;
        CInventorySlotUI toSlot = FindSlot(eventData.pointerCurrentRaycast);

        // 거래 중 상점 슬롯이 삭제될 수 있으므로 먼저 드래그 종료
        CancelDrag();

        if (toSlot == null || fromSlot == toSlot)
        {
            return;
        }

        bool fromShop = fromSlot is CMerchantSlotUI;
        bool toShop = toSlot is CMerchantSlotUI;

        // 상점 → 상점
        if (fromShop && toShop)
        {
            return;
        }

        // 상점 → 인벤토리
        if (fromShop)
        {
            if (_merchant != null)
            {
                _merchant.TryBuy((CMerchantSlotUI)fromSlot, toSlot);
            }

            return;
        }

        // 인벤토리 → 상점
        if (toShop)
        {
            if (_merchant != null)
            {
                _merchant.TrySell(fromSlot);
            }

            return;
        }

        // 인벤토리 → 인벤토리
        _inventoryUI.TryMoveSlot(fromSlot, toSlot);
    }

    // CInventorySlotUI이 붙은 UI 찾기 
    private CInventorySlotUI FindSlot(RaycastResult result)
    {
        GameObject hit = result.gameObject;
        return hit == null ? null : hit.GetComponentInParent<CInventorySlotUI>();
    }

    // 화면좌표를 UI좌표로 변환 드래그 이미지를 이동
    private void MoveIcon(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(_UICanvas, eventData.position, eventData.pressEventCamera, out Vector2 localPosition))
        {
            _dragIcon.rectTransform.localPosition = localPosition;
        }
    }

    // 드래그 취소 및 초기화 과정
    public void CancelDrag()
    {
        _fromSlot = null;
        _dragIcon.gameObject.SetActive(false);
        _dragIcon.sprite = null;
    }

}