using UnityEngine;

[CreateAssetMenu(fileName = "RewardPrefab_", menuName = "SO/Dungeon/RewardPrefab")]
public class CRewardPrefabSO : ScriptableObject
{
    #region 인스펙터

    [Header("주사위")]
    [SerializeField] private GameObject _dicePrefab;

    [Header("아티팩트")]
    [SerializeField] private GameObject _artifactPrefab;

    [Header("석판")]
    [SerializeField] private GameObject _tabletPrefab;

    [Header("인챈트")]
    [SerializeField] private GameObject _enchantPrefab;

    [Header("인벤토리 확장")]
    [SerializeField] private GameObject _inventoryExpansionPrefab;

    [Header("경험치")]
    [SerializeField] private GameObject _experiencePrefab;

    [Header("잎")]
    [SerializeField] private GameObject _leafPrefab;

    #endregion

    public GameObject GetRewardPrefab(ERewardType rewardType)
    {
        switch (rewardType)
        {
            case ERewardType.Dice:
                return _dicePrefab;

            case ERewardType.Artifact:
                return _artifactPrefab;

            case ERewardType.Tablet:
                return _tabletPrefab;

            case ERewardType.Enchant:
                return _enchantPrefab;

            case ERewardType.Experience:
                return _experiencePrefab;

            case ERewardType.Leaf:
                return _leafPrefab;

            default:
                return null;
        }
    }
}