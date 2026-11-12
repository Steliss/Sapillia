using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 아이템에 대한 데이터베이스
/// </summary>
[CreateAssetMenu(menuName = "SO/Item/Database")]
public class CItemDatabaseSO : ScriptableObject
{
    [SerializeField] private List<CItemSO> _items = new List<CItemSO>();

    /// <summary>
    /// ID를 통한 아이템 획득
    /// </summary>
    /// <param name="itemID"></param>
    /// <returns></returns>
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


    /// <summary>
    /// 해당 종류의 아이템 중 하나를 조회 
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public CItemSO GetRandomItem(EItemType type)
    {
        // 아이템 후보군
        List<CItemSO> candidates = new();

        foreach (CItemSO item in _items)
        {
            if (item != null && item.Type == type)
            {
                candidates.Add(item);
            }
        }

        if (candidates.Count == 0)
        {
            CLog.Log("해당 종류의 아이템이 없습니다");
            return null;
        }

        int index = Random.Range(0, candidates.Count);
        return candidates[index];
    }
}