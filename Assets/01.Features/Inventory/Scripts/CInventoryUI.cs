using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CInventoryUI : MonoBehaviour, IInputCommand
{
    [SerializeField] private CESCUI _esc;
    [SerializeField] private GameObject _UICanvas;
    [SerializeField] private CInventoryDrag _drag;

    [SerializeField] private GameObject _InventoryInfo;
    [SerializeField] private Image _infoImage;
    [SerializeField] private TextMeshProUGUI _infoTitle;
    [SerializeField] private TextMeshProUGUI _infoDesc;

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


    private bool _isActive;
    public bool IsActive => _isActive;

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
            _localization.OnLanguageChanged += ReloadText;
            ReloadText();

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

        _localization.OnLanguageChanged -= ReloadText;
        _input.Unregister(EKeyAction.Inventory, this);
    }


    private void ReloadText()
    {
    }

    // 인벤토리 열고 닫기
    public void Execute()
    {
        if (_esc.IsActive)
        {
            return;
        }

        RefreshInventoryUI();
        ActiveUI(!_isActive);
    }

    public void ActiveUI(bool active)
    {
        _isActive = active;

        HideItemInfo();

        if (!active)
        {
            _drag.CancelDrag();
        }

        _UICanvas.SetActive(active);
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
    /// 출발과 도착칸의 데이터를 바꾸고 화면에서 갱신
    /// </summary>
    /// <param name="fromSlot"></param>
    /// <param name="toSlot"></param>
    /// <returns></returns>
    public void TryMoveSlot(CInventorySlotUI fromSlot, CInventorySlotUI toSlot)
    {
        // 같은 위치거나 창이 안열려 있을 경우 
        if (!_isActive || fromSlot == toSlot)
        {
            return;
        }

        // 출발과 도착 슬롯이 없으면 종료
        if (!TryFindSlot(fromSlot, out var fromList, out int fromIndex) || !TryFindSlot(toSlot, out var toList, out int toIndex))
        {
            return;
        }

        // ※ : TODO : 포션 슬롯에 포션만 들어가게 추가 


        // 교환을 위한 변수
        CInventorySlotData fromData = fromList[fromIndex];

        // 같은 영역과 다른 영역 모두 두 위치의 데이터를 교환
        fromList[fromIndex] = toList[toIndex];
        toList[toIndex] = fromData;

        // 변경된 두 칸만 갱신
        fromSlot.BindSlot(fromList[fromIndex]);
        toSlot.BindSlot(toList[toIndex]);

        // 아티팩트 갱신        
        _inventoryData.NotifyInventoryChanged();
        
        ShowItemInfo(toSlot);

        return;
    }

    // 데이터 위치 찾기 
    // 슬롯의 위치를 기반으로 List에서 인덱스를 비교 리스트값 반환
    private bool TryFindSlot(
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
        _infoDesc.text = _localization.GetText(item.DescriptionKey);

        _InventoryInfo.SetActive(true);
    }

    /// <summary>
    /// 아이템 정보 숨기기
    /// </summary>
    public void HideItemInfo()
    {
        _InventoryInfo.SetActive(false);
    }

}