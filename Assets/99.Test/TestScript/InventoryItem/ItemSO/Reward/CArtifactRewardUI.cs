using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CArtifactRewardUI : MonoBehaviour, IESCCommand
{
    #region 인스펙터

    [Header("보상 후보 아티팩트")]
    [SerializeField] private List<CArtifactSO> _artifacts = new();

    [Header("보상 버튼 4개")]
    [SerializeField] private Button[] _rewardButtons = new Button[4];

    [Header("아티팩트 아이콘 이미지 4개")]
    [SerializeField] private Image[] _artifactImages = new Image[4];


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

    private readonly List<CArtifactSO> _selectedArtifacts = new();

    private CArtifactReward _currentReward;

    private bool _isRewardGenerated = false;

    private bool _isLevelUpReward = false;

    private CInputCommandHub _inputHub;
    #endregion

    #region 이벤트
    public event Action<CItemSO> OnMouseEnter;
    public event Action OnMouseExit;
    #endregion

    private void OnEnable()
    {
        _inputHub = FindFirstObjectByType<CInputCommandHub>();

        if (_inputHub == null)
        {
            Debug.LogError("CArtifactRewardUI 인풋 허브를 찾을 수 없습니다.");
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


    public void ShowRewards(CArtifactReward reward)
    {
        _inputHub.RegisterOpenedUI(this);

        _isLevelUpReward = false;


        if (_currentReward != reward)
        {
            _isRewardGenerated = false;
        }

        _currentReward = reward;

        if (!_isRewardGenerated)
        {
            GenerateRewardCandidates();
        }

        gameObject.SetActive(true);
        UpdateRerollText();
    }

    private void GenerateRewardCandidates()
    {
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

        // 중복 없이 4개 선택
        for (int i = 0; i < 4; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);

            _selectedArtifacts.Add(candidates[randomIndex]);
            candidates.RemoveAt(randomIndex);
        }

        // 버튼과 이미지 갱신
        for (int i = 0; i < 4; i++)
        {
            Image image = _artifactImages[i];

            image.sprite = _selectedArtifacts[i].Icon;
            image.enabled = image.sprite != null;

            Image background = _rewardButtons[i].image;
            background.sprite = GetGradeBackground(_selectedArtifacts[i].Grade);

            _rewardButtons[i].onClick.RemoveAllListeners();
            SetButton(_rewardButtons[i], _selectedArtifacts[i]);

            int index = i;
            _rewardButtons[i].onClick.AddListener(() => SelectArtifact(index));
        }

        _inputHub.UnregisterOpenedUI(this);

        _isRewardGenerated = true;
    }

    // 마우스 이동에 따른 설명 UI 오픈
    private void SetButton(Button button, CItemSO itemSO)
    {
        EventTrigger trigger = button.GetComponent<EventTrigger>();

        if (trigger == null)
        {
            trigger = button.gameObject.AddComponent<EventTrigger>();
        }
        // 이벤트 트리거 비우기
        trigger.triggers.Clear();

        // 마우스 진입
        EventTrigger.Entry enter = new EventTrigger.Entry();
        enter.eventID = EventTriggerType.PointerEnter;
        enter.callback.AddListener(eventData =>
        {
            OnMouseEnter.Invoke(itemSO);
        });

        // 마우스 이탈
        EventTrigger.Entry exit = new EventTrigger.Entry();
        exit.eventID = EventTriggerType.PointerExit;
        exit.callback.AddListener(eventData =>
        {
            OnMouseExit.Invoke();
        });

        // 이벤트에 대한 해제 이벤트가 추가되면 고민이 필요
        trigger.triggers.Add(enter);
        trigger.triggers.Add(exit);
    }

    /// <summary>
    /// 선택한 아티팩트 획득
    /// </summary>
    private void SelectArtifact(int index)
    {

        if (_dataManager == null)
        {
            _dataManager = CManagerDontDestroy.Instance?.DataManger;
        }

        if (_dataManager == null)
        {
            Debug.LogError("[CArtifactRewardUI] DataManager를 찾을 수 없습니다.");
            return;
        }


        CInventoryData inventory = _dataManager.InventoryData?.InventoryData;

        if (inventory == null)
        {
            Debug.LogError("[CArtifactRewardUI] InventoryData가 준비되지 않았습니다.");
            return;
        }

        CArtifactSO artifact = _selectedArtifacts[index];

        bool success = inventory.AddArtifact(artifact);

        if (!success)
        {
            Debug.LogWarning("아티팩트 획득 실패");
            return;
        }

        // 레벨업 보상획득 시 횟수 감소
        if (_isLevelUpReward)
        {
            CPlayerGamingDataSO playerData = _dataManager.PlayerData?.GetPlayerData();

            if (playerData != null && playerData.LevelUpRewardCount > 0)
            {
                playerData.LevelUpRewardCount--;

            }
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


    /// <summary>
    /// 보상 선택을 취소하고 UI 닫기
    /// </summary>
    public void CancelSelection()
    {
        gameObject.SetActive(false);

        if (_currentReward != null)
        {
            _currentReward.CancelReward();
        }
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

        CPlayerGamingDataSO playerData = _dataManager.PlayerData?.GetPlayerData();

        if (playerData == null)
        {
            return;
        }

        if (!playerData.TrySpendDice(1))
        {
            return;
        }

        GenerateRewardCandidates();
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

        CPlayerGamingDataSO playerData = _dataManager.PlayerData?.GetPlayerData();

        if (playerData == null)
        {
            return;
        }

        // 현재 주사위 개수 표시
        _rerollCountText.text = $"   리롤({playerData.Dice}회)";

        // 주사위가 없으면 리롤 버튼 비활성화
        _rerollButton.interactable = playerData.Dice > 0;
    }

    public void ShowLevelUpRewards()
    {
        _isLevelUpReward = true;
        _currentReward = null;
        _isRewardGenerated = false;

        GenerateRewardCandidates();

        gameObject.SetActive(true);
        UpdateRerollText();
    }
}
