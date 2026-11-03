using System.Collections.Generic;
using UnityEngine;

public class CInventoryUI : MonoBehaviour, IInputCommand
{
    [SerializeField] private CESCUI _esc;
    [SerializeField] private GameObject _UICanvas;

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
            CLog.Log("CInventoryUI _dataTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _dataTask.Cancel();
        _localizationTask.Cancel();

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

        ActiveUI(!_isActive);
    }

    // 인벤토리 닫기
    public void ActiveUI(bool active)
    {
        _isActive = active;
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

            slot.BindSlot(dataList[i]);

            slotUIList.Add(slot);
        }
    }
}