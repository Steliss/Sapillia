using UnityEngine;

public class ArtifactTestScript : MonoBehaviour
{
    #region 인스펙터

    [SerializeField] private CArtifactSO _artifactA;
    [SerializeField] private CArtifactSO _artifactB;

    [SerializeField] private int _starsA = 2;
    [SerializeField] private int _starsB = 1;

    #endregion

    private void Start()
    {
        if (_artifactA == null || _artifactB == null)
        {
            Debug.LogError("테스트 아티팩트가 설정되지 않았습니다.");
            return;
        }

        // 각 아티팩트의 강화된 능력치
        SPlayerStats statsA = _artifactA.GetStats(_starsA);
        SPlayerStats statsB = _artifactB.GetStats(_starsB);

        // 두 아티팩트의 능력치 합산
        SPlayerStats totalStats =
            CPlayerStatsCalculator.Add(statsA, statsB);

        Debug.Log($"아티팩트 A ({_starsA}성) | 힘: {statsA.Attribute_Physical} | 체력: {statsA.MaxHP}");
        Debug.Log($"아티팩트 B ({_starsB}성) | 힘: {statsB.Attribute_Physical} | 체력: {statsB.MaxHP}");

        Debug.Log($"합산 결과 | 힘: {totalStats.Attribute_Physical} | 체력: {totalStats.MaxHP}");
    }
}