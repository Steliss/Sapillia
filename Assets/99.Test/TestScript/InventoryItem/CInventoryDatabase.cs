using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템에 대한 데이터베이스
/// </summary>
[CreateAssetMenu(menuName = "SO/Item/Database")]
public class CItemDatabaseSO : ScriptableObject
{
    [SerializeField] private List<CItemSO> _items = new List<CItemSO>();

    public CItemSO SetItem(int itemID)
    {
        foreach (CItemSO item in _items)
        {
            if (item != null && item.ItemID == itemID)
            {
                return item;
            }
        }
        return null;
    }
}