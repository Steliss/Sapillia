using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CInventorySlotUI : MonoBehaviour
{
    [SerializeField] private Button _button;
    [SerializeField] private Image _itemImage;
    [SerializeField] private TextMeshProUGUI _countText;

    private CInventorySlotData _slotData;

    public Sprite ItemImage => _itemImage.isActiveAndEnabled ? _itemImage.sprite : null;

    /// <summary>
    /// 슬롯 정보 갱신 함수
    /// </summary>
    /// <param name="slotData"></param>
    public void BindSlot(CInventorySlotData slotData)
    {
        _slotData = slotData;
        Refresh();
    }

    // 아이콘과 수량 갱신
    public void Refresh()
    {
        if (_slotData == null || _slotData.IsEmpty)
        {
            _itemImage.sprite = null;
            _itemImage.enabled = false;
            _countText.text = "";
            return;
        }

        // 아이템 원본이 연결되어 있으면 아이콘 표시
        if (_slotData.ItemSO != null)
        {
            _itemImage.sprite = _slotData.ItemSO.Icon;
            _itemImage.enabled = _itemImage.sprite != null;
        }


        _countText.text = _slotData.Count.ToString();
    }
}