using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class CMerchantUI : MonoBehaviour
{
    [SerializeField] private CMerchantSlotUI _slotPrefab;
    [SerializeField] private Transform _slotRoot;
    [SerializeField] private CInventoryUI _inventoryUI;
    [SerializeField] private CInventoryDrag _drag;
    [SerializeField] private CItemDatabaseSO _itemdata;
    [SerializeField] private CMerchant _merchant;


    [Header("아이템 정보")]
    [SerializeField] private GameObject _InventoryInfo;
    [SerializeField] private Image _infoImage;
    [SerializeField] private TextMeshProUGUI _infoTitle;
    [SerializeField] private TextMeshProUGUI _infoDesc;

    private List<CMerchantSlotUI> _slotUI = new();
    private List<CInventorySlotData> _shopSlots;

    private CLocalizationManager _localization;
    private CUniTaskOnes _localizationTask = new();

    private void OnEnable()
    {
        _localizationTask.Run(async token =>
        {
            _localization = await CManagerDontDestroy.LocalizationDataAsync(token);
            CLog.Log("CMerchantUI OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _localizationTask.Cancel();
    }

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

            slot.SetMerchantUI(this);
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


    // 마우스를 올린 슬롯의 아이템 정보 표시
    public void ShowItemInfo(CInventorySlotUI slot)
    {
        if (_localization == null || slot == null)
        {
            HideItemInfo();
            return;
        }

        CInventorySlotData data = slot.SlotData;

        if (data == null || data.IsEmpty || data.ItemSO == null)
        {
            HideItemInfo();
            return;
        }

        CItemSO item = data.ItemSO;

        if (item is CArtifactSO artifact)
        {
            List<string> synergyNames = new();

            foreach (EArtifactSynergy synergy in artifact.Synergies)
            {
                string key = $"item.synergy.name.{synergy.ToString().ToLowerInvariant()}";
                synergyNames.Add(_localization.GetText(key));
            }

            _infoTitle.text = _localization.GetText(item.NameKey) + "\n <size=50%>" + string.Join(", ", synergyNames) + "</size>";
            _infoDesc.text = _localization.GetText(item.DescriptionKey, artifact.GetDescriptionValues(0)) + "\n\n" + _localization.GetText(item.StoryKey);

        }
        else
        {
            _infoTitle.text = _localization.GetText(item.NameKey);
            _infoDesc.text = _localization.GetText(item.DescriptionKey, item.GetDescriptionValues()) + "\n\n" + _localization.GetText(item.StoryKey);
        }

        _infoImage.sprite = item.Icon;
        _InventoryInfo.SetActive(true);
    }

    public void HideItemInfo()
    {
        _InventoryInfo.SetActive(false);
    }

}