using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CRewardEnchant : MonoBehaviour, IInputCommand
{
    #region 인스펙터

    [Header("인벤토리 UI")]
    [SerializeField] private CInventoryUI _inventoryUI;

    [Header("인챈트 횟수")]
    [SerializeField] private int _maxEnchantCount = 3;

    #endregion

    #region 내부변수

    private bool _isPlayerInside;

    private int _remainCount;

    private CInputCommandHub _inputHub;

    #endregion

    #region 프로퍼티
    public int RemainCount => _remainCount;

    #endregion


    private void Awake()
    {
        _inventoryUI = FindFirstObjectByType<CInventoryUI>(FindObjectsInactive.Include);

        _remainCount = _maxEnchantCount;
    }


    private void OnEnable()
    {
        _inputHub = FindFirstObjectByType<CInputCommandHub>();

        if (_inputHub == null)
        {
            Debug.LogError("[CRewardEnchant] 입력 허브를 찾을 수 없습니다.", this);
            return;
        }

        _inputHub.Register(EKeyAction.Interaction, this);
    }

    public void Execute()
    {
        if (!_isPlayerInside)
        {
            return;
        }

        if (_inventoryUI == null || _inventoryUI.IsActive || _remainCount <= 0)
        {
            return;
        }

        _inventoryUI.StartEnchant(this);
    }

    private void OnDisable()
    {
        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Interaction, this);
        }

        _isPlayerInside = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerInside = false;
    }



    /// <summary>
    /// 인챈트 성공 시 남은 횟수 감소
    /// </summary>
    public void OnEnchantSuccess()
    {
        if (_remainCount <= 0)
        {
            return;
        }

        _remainCount--;

        if (_remainCount <= 0)
        {
            _inventoryUI.SetEnchantMode(false);
            _inventoryUI.ActiveUI(false);
        }
    }

}
