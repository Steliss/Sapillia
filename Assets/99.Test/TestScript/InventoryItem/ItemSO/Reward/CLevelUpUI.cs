using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CLevelUpUI : MonoBehaviour, IInputCommand
{
    #region 인스펙터

    [Header("레벨업 알림 이미지")]
    [SerializeField] private GameObject _levelUpImage;

    [Header("아티팩트 보상 UI")]
    [SerializeField] private CArtifactRewardUI _artifactRewardUI;

    #endregion

    #region 내부변수

    private CPlayerGamingDataSO _playerData;

    private CInputCommandHub _inputHub;

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

    private void OnEnable()
    {
        _inputHub = FindFirstObjectByType<CInputCommandHub>();

        if (_inputHub == null)
        {
            Debug.LogError("[CLevelUpUI] 입력 허브를 찾을 수 없습니다.", this);
            return;
        }

        _inputHub.Register(EKeyAction.Reward, this);
    }

    private void OnDisable()
    {
        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Reward, this);
        }
    }

    private void Update()
    {
        if (_playerData != null)
        {
            UpdateLevelUpImage();
        }
    }

    public void Execute()
    {
        if (_playerData == null || _playerData.LevelUpRewardCount <= 0)
        {
            return;
        }

        if (_artifactRewardUI == null || _artifactRewardUI.gameObject.activeInHierarchy)
        {
            return;
        }

        _artifactRewardUI.ShowLevelUpRewards();
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