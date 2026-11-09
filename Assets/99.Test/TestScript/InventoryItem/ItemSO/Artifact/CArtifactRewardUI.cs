using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;

public class CArtifactRewardUI : MonoBehaviour
{
    #region 인스펙터

    [Header("보상 후보 아티팩트")]
    [SerializeField] private List<CArtifactSO> _artifacts = new();

    [Header("보상 버튼 4개")]
    [SerializeField] private Button[] _rewardButtons = new Button[4];

    [Header("데이터 매니저")]
    [SerializeField] private CDataManger _dataManager;
    #endregion

    #region 내부변수

    private readonly List<CArtifactSO> _selectedArtifacts = new();

    private CArtifactReward _currentReward;

    #endregion

    /// <summary>
    /// 랜덤 아티팩트 4개 선택 및 표시
    /// </summary>
    public void ShowRewards(CArtifactReward reward)
    {
        _currentReward = reward;

        _selectedArtifacts.Clear();

        // 중복된 ID를 제거하고 후보 목록 생성
        List<CArtifactSO> candidates = new();
        HashSet<int> usedIDs = new();

        foreach (CArtifactSO artifact in _artifacts)
        {
            if (artifact == null || artifact.ItemID <= 0)
                continue;

            if (usedIDs.Add(artifact.ItemID))
                candidates.Add(artifact);
        }

        if (candidates.Count < 4)
        {
            Debug.LogWarning("보상 후보 아티팩트가 4개 미만입니다.");
            return;
        }

        // 후보 중 중복 없이 4개 선택
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            _selectedArtifacts.Add(candidates[randomIndex]);
            candidates.RemoveAt(randomIndex);
        }

        // 버튼에 아티팩트 아이콘 표시
        for (int i = 0; i < 4; i++)
        {
            Image image = _rewardButtons[i].image;

            image.sprite = _selectedArtifacts[i].Icon;
            image.enabled = image.sprite != null;

            // 기존 클릭 이벤트 제거
            _rewardButtons[i].onClick.RemoveAllListeners();

            // 클릭했을 때 해당 아티팩트 선택
            int index = i;
            _rewardButtons[i].onClick.AddListener(() => SelectArtifact(index));
        }

        gameObject.SetActive(true);
    }


    /// <summary>
    /// 선택한 아티팩트 획득
    /// </summary>
    private void SelectArtifact(int index)
    {
        Debug.Log($"[보상 선택 테스트] 버튼 {index + 1} 클릭됨");

        if (_dataManager == null)
        {
            return;
        }

        CInventoryData inventory = _dataManager.InventoryData?.InventoryData;

        if (inventory == null)
        {
            Debug.LogError("[CArtifactRewardUI] DataManager가 연결되지 않았습니다.");
            return;
        }

        CArtifactSO artifact = _selectedArtifacts[index];

        bool success = inventory.AddArtifact(artifact);

        if (!success)
        {
            Debug.LogWarning("아티팩트 획득 실패");
            return;
        }

        Debug.Log($"선택한 아티팩트 획득: {artifact.ItemID}");

        // 보상 선택 UI 닫기
        gameObject.SetActive(false);

        if (_currentReward != null)
        {
            Destroy(_currentReward.gameObject);
            _currentReward = null;
        }
    }
}
