[System.Serializable]
public struct SRewardNodeData
{
    public ERewardType RewardType;
    public bool HasWanderingMerchant;

    public SRewardNodeData(ERewardType rewardType, bool hasWanderingMerchant)
    {
        RewardType = rewardType;
        HasWanderingMerchant = hasWanderingMerchant;
    }
}