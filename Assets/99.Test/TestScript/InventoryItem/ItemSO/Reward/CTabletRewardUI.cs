using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CTabletRewardUI : MonoBehaviour, IESCCommand
{
    #region 인스펙터

    [Header("보상 후보 석판")]
    [SerializeField] private List<CTabletSO> _tablets = new();

    [Header("보상 버튼 4개")]
    [SerializeField] private Button[] _rewardButtons = new Button[4];

    [Header("석판 아이콘 이미지 4개")]
    [SerializeField] private Image[] _tabletImages = new Image[4];

    [Header("등급별 배경 이미지")]
    [SerializeField] private Sprite _normalBackground;
    [SerializeField] private Sprite _advancedBackground;
    [SerializeField] private Sprite _rareBackground;
    [SerializeField] private Sprite _legendaryBackground;

    [Header("데이터 매니저")]
    [SerializeField] private CDataManger _dataManager;

    [Header("리롤 버튼")]
    [SerializeField] private Button _rerollButton;

    [Header("리롤 횟수 텍스트")]
    [SerializeField] private TextMeshProUGUI _rerollCountText;

    #endregion

    #region 내부변수

    private readonly List<CTabletSO> _selectedTablets = new();

    private CTabletReward _currentReward;

    private bool _isRewardGenerated = false;

    private CInputCommandHub _inputHub;
    #endregion

    private void OnEnable()
    {
        _inputHub = FindFirstObjectByType<CInputCommandHub>();

        if (_inputHub == null)
        {
            Debug.LogError("CTabletRewardUI 인풋 허브를 찾을 수 없습니다.");
            return;
        }

    }

    public void ExecuteESC()
    {
        CancelSelection();
    }

    private void OnDisable()
    {
        if (_inputHub != null)
        {
            _inputHub.UnregisterOpenedUI(this);
        }
    }

    public void ShowRewards(CTabletReward reward)
    {
        if (_currentReward != reward)
        {
            _isRewardGenerated = false;
        }

        _currentReward = reward;

        if (!_isRewardGenerated)
        {
            if (!GenerateRewardCandidates())
            {
                _currentReward?.CancelReward();
                return;
            }
        }

        gameObject.SetActive(true);
        UpdateRerollText();

        _inputHub.RegisterOpenedUI(this);
    }

    private bool GenerateRewardCandidates()
    {
        List<CTabletSO> candidates = new();
        HashSet<int> usedIDs = new();

        foreach (CTabletSO tablet in _tablets)
        {
            if (tablet == null || tablet.ItemID <= 0)
            {
                continue;
            }

            if (usedIDs.Add(tablet.ItemID))
            {
                candidates.Add(tablet);
            }
        }

        if (candidates.Count < 4)
        {
            Debug.LogWarning("[CTabletRewardUI] 석판 후보가 4개 미만입니다.");
            return false;
        }

        _selectedTablets.Clear();

        // 중복 없이 석판 4개 선택
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = Random.Range(0, candidates.Count);

            _selectedTablets.Add(candidates[randomIndex]);
            candidates.RemoveAt(randomIndex);
        }

        // 석판 아이콘 및 등급별 배경 갱신
        for (int i = 0; i < 4; i++)
        {
            Image image = _tabletImages[i];

            image.sprite = _selectedTablets[i].Icon;
            image.enabled = image.sprite != null;

            Image background = _rewardButtons[i].image;
            background.sprite = GetGradeBackground(_selectedTablets[i].Grade);

            _rewardButtons[i].onClick.RemoveAllListeners();

            int index = i;
            _rewardButtons[i].onClick.AddListener(() => SelectTablet(index));
        }

        _isRewardGenerated = true;

        return true;
    }

    private void SelectTablet(int index)
    {
        if (_dataManager == null)
        {
            _dataManager = CManagerDontDestroy.Instance?.DataManger;
        }

        if (_dataManager == null)
        {
            Debug.LogError("[CTabletRewardUI] DataManager를 찾을 수 없습니다.");
            return;
        }

        CInventoryData inventory = _dataManager.InventoryData?.InventoryData;

        if (inventory == null)
        {
            Debug.LogError("[CTabletRewardUI] InventoryData가 준비되지 않았습니다.");
            return;
        }

        CTabletSO tablet = _selectedTablets[index];

        bool success = inventory.AddTablet(tablet);

        if (!success)
        {
            Debug.LogWarning("[CTabletRewardUI] 석판 획득 실패");
            return;
        }

        Debug.Log($"선택한 석판 획득: {tablet.ItemID}");

        gameObject.SetActive(false);

        if (_currentReward != null)
        {
            Destroy(_currentReward.gameObject);
            _currentReward = null;
        }
    }

    public void CancelSelection()
    {
        gameObject.SetActive(false);

        if (_currentReward != null)
        {
            _currentReward.CancelReward();
        }

        _inputHub.UnregisterOpenedUI(this);
    }

    private Sprite GetGradeBackground(EArtifactGrade grade)
    {
        switch (grade)
        {
            case EArtifactGrade.Normal:
                return _normalBackground;

            case EArtifactGrade.Advanced:
                return _advancedBackground;

            case EArtifactGrade.Rare:
                return _rareBackground;

            case EArtifactGrade.Legendary:
                return _legendaryBackground;

            default:
                return _normalBackground;
        }
    }

    public void RerollRewards()
    {
        if (_dataManager == null)
        {
            _dataManager = CManagerDontDestroy.Instance?.DataManger;
        }

        if (_dataManager == null)
        {
            return;
        }

        CPlayerGamingDataSO playerData =
            _dataManager.PlayerData?.GetPlayerData();

        if (playerData == null)
        {
            return;
        }

        // 주사위가 없는 경우
        if (playerData.Dice < 1)
        {
            UpdateRerollText();
            return;
        }

        // 현재 석판 후보가 정상적으로 생성되어 있는지 확인
        if (_selectedTablets.Count != 4)
        {
            return;
        }

        // 주사위 1개 소비
        if (!playerData.TrySpendDice(1))
        {
            UpdateRerollText();
            return;
        }

        // 석판 후보 다시 추첨
        if (!GenerateRewardCandidates())
        {
            // 추첨 실패 시 주사위 반환
            playerData.AddDice(1);
            UpdateRerollText();
            return;
        }

        UpdateRerollText();

    }


    private void UpdateRerollText()
    {
        if (_dataManager == null)
        {
            _dataManager = CManagerDontDestroy.Instance?.DataManger;
        }

        if (_dataManager == null)
        {
            return;
        }

        CPlayerGamingDataSO playerData =
            _dataManager.PlayerData?.GetPlayerData();

        if (playerData == null)
        {
            return;
        }

        _rerollCountText.text = $"리롤({playerData.Dice}회)";

        _rerollButton.interactable = playerData.Dice > 0;
    }
}