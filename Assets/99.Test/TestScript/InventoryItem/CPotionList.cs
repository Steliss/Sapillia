using System;
using System.Collections.Generic;
using UnityEngine;


public class CPotionList : MonoBehaviour, IInputCommand
{
    private List<CInventorySlotData> _inventorySlots;
    private List<CInventorySlotData> _potionSlots;

    private CInputCommandHub _input;
    private CDataManger _data;
    private CInventoryData _inventoryData;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private List<CPotionSO> _potionList = new();  // 보관 장소

    private int _index = 0; // 선택 인덱스



    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.ChoosePotion, this);

            CLog.Log("CPotionList _inputTask OnEnable 작업 완료");
        });
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _inventoryData = _data.InventoryData.GetInventoryData();
            _inventorySlots = _inventoryData.InventorySlots;
            _potionSlots = _inventoryData.PotionSlots;

            _inventoryData.OnInventoryChanged += Refresh;

            // 이미 가지고 있는 포션도 최초 목록에 반영
            Refresh();
                
            CLog.Log("CPotionList _dataTask OnEnable 작업 완료");
        });

    }

    private void OnDisable()
    {
        _inputTask.Cancel();
        _dataTask.Cancel();

        _input.Unregister(EKeyAction.ChoosePotion, this);

        _inventoryData.OnInventoryChanged -= Refresh;
    }

    public void Execute()
    {
        // 인덱스에 따른 현재 포션 순회
    }

    public void UseCurrentPotion()
    {
        // 현재 인덱스 포션 사용
    }

    private void Refresh()
    {
        // 목록 업데이트 
    }

}