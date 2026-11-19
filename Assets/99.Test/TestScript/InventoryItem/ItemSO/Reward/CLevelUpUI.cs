using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLevelUpUI : MonoBehaviour
{
    #region 인스펙터

    [Header("레벨업 알림 이미지")]
    [SerializeField] private GameObject _levelUpImage;

    [Header("아티팩트 보상 UI")]
    [SerializeField] private CArtifactRewardUI _artifactRewardUI;

    #endregion

    #region 내부변수

    private CPlayerGamingDataSO _playerData;

    #endregion

    private void Start()
    {
        CDataManger dataManager = CManagerDontDestroy.Instance?.DataManger;

        if (dataManager == null)
        {
            Debug.LogError("[CLevelUpUI] DataManager를 찾을 수 없습니다.");
            return;
        }

        _playerData = dataManager.PlayerData?.GetPlayerData();

        if (_playerData == null)
        {
            Debug.LogError("[CLevelUpUI] PlayerData를 찾을 수 없습니다.");
            return;
        }

        UpdateLevelUpImage();
    }

    private void Update()
    {
        if (_playerData == null)
        {
            return;
        }

        UpdateLevelUpImage();

        // 레벨업 보상이 없으면 종료
        if (_playerData.LevelUpRewardCount <= 0)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            if (_artifactRewardUI == null)
            {
                return;
            }

            // 이미 보상 UI가 열려 있으면 중복 실행 방지
            if (_artifactRewardUI.gameObject.activeInHierarchy)
            {
                return;
            }

            _artifactRewardUI.ShowLevelUpRewards();
        }
    }

    private void UpdateLevelUpImage()
    {
        if (_levelUpImage == null)
        {
            return;
        }

        bool hasReward = _playerData.LevelUpRewardCount > 0;

        if (_levelUpImage.activeSelf != hasReward)
        {
            _levelUpImage.SetActive(hasReward);
        }
    }
}