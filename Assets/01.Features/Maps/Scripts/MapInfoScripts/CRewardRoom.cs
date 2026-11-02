using UnityEngine;

public class CRewardRoom : MonoBehaviour
{
    #region 내부변수

    private ERewardType _rewardType;

    #endregion

    #region 프로퍼티

    public ERewardType RewardType => _rewardType;

    #endregion

    public void Initialize(ERewardType rewardType)
    {
        _rewardType = rewardType;

        Debug.Log($"보상 방 초기화: {_rewardType}");
    }
}