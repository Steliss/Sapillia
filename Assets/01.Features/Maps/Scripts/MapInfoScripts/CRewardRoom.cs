using UnityEngine;

public class CRewardRoom : MonoBehaviour
{
    #region 인스펙터

    [Header("보상 생성 위치")]
    [SerializeField] private Transform _rewardSpawn;

    #endregion

    #region 내부변수

    private ERewardType _rewardType;

    #endregion

    #region 프로퍼티

    public ERewardType RewardType => _rewardType;
    public Transform RewardSpawn => _rewardSpawn;

    #endregion

    public void Initialize(ERewardType rewardType, GameObject rewardPrefab)
    {
        _rewardType = rewardType;

        if (_rewardSpawn == null)
        {
            Debug.LogWarning("보상 생성 위치가 설정되지 않았습니다.");
            return;
        }

        if (rewardPrefab == null)
        {
            Debug.LogWarning($"{_rewardType} 보상 프리팹이 없습니다.");
            return;
        }

        Instantiate(
            rewardPrefab,
            _rewardSpawn.position,
            Quaternion.identity,
            _rewardSpawn
        );

        Debug.Log($"보상 오브젝트 생성 완료: {_rewardType}");
    }
}