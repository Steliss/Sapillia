using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CStageNodeUI : MonoBehaviour
{

    #region 인스펙터

    [Header("층별 노드")]
    [SerializeField] private CStageNodeButton[] _floor2Nodes;
    [SerializeField] private CStageNodeButton[] _floor3Nodes;
    [SerializeField] private CStageNodeButton[] _floor4Nodes;
    [SerializeField] private CStageNodeButton _floor5BossNode;

    [Header("보상 아이콘")]
    [SerializeField] private Sprite _artifactIcon;
    [SerializeField] private Sprite _tabletIcon;
    [SerializeField] private Sprite _diceIcon;
    [SerializeField] private Sprite _moneyIcon;
    [SerializeField] private Sprite _experienceIcon;
    [SerializeField] private Sprite _anvilIcon;
    [SerializeField] private Sprite _enchantIcon;

    [Header("특수 아이콘")]
    [SerializeField] private Sprite _merchantIcon;

    [Header("노드 선택 패널")]
    [SerializeField] private GameObject _nextStagePanel;


    [Header("테스트용")]
    [SerializeField] private int _testFloor = 1;

    #endregion

    #region 내부변수

    private CDataManger _dataManager;

    private List<CStageNodeData> _stageNodes = new List<CStageNodeData>();

    #endregion

    private void Awake()
    {
        _dataManager = FindFirstObjectByType<CDataManger>();
    }


    private void Start()
    {
        if (_dataManager == null)
        {
            Debug.LogError("CDataManger를 찾을 수 없습니다.");
            return;
        }

        //int currentFloor = _dataManager.CurrentFlow.PlayerProgress;
        int currentFloor = _testFloor;
        int nextFloor = currentFloor + 1;

        Debug.Log($"현재 층: {currentFloor}");
        Debug.Log($"선택 가능한 다음 층: {nextFloor}");


        GenerateStageNodes();

        SetStageNodeData();

        UpdateNodeInteractable();

        foreach (CStageNodeData node in _stageNodes)
        {
            Debug.LogWarning(
                $"{node.Floor}층 / 보상: {node.Reward} / " +
                $"상인: {node.HasMerchant} / " +
                $"미니보스: {node.HasMiniBoss} / " +
                $"보스: {node.HasStageBoss}"
            );
        }
    }


    private List<EStageNodeReward> GetRewardPool()
    {
        return new List<EStageNodeReward>
        {
            EStageNodeReward.Artifact,
            EStageNodeReward.Tablet,
            EStageNodeReward.Dice,
            EStageNodeReward.Money,
            EStageNodeReward.Experience,
            EStageNodeReward.Enchant
        };
    }


    private void GenerateStageNodes()
    {
        _stageNodes.Clear();

        // 1층 시작 노드
        _stageNodes.Add(new CStageNodeData(1, EStageNodeReward.None));

        // 모루가 등장할 층과 위치 결정
        int anvilFloor = Random.Range(2, 4);
        int anvilIndex = Random.Range(0, 3);

        // 2층과 3층 노드 생성
        for (int floor = 2; floor <= 3; floor++)
        {
            List<EStageNodeReward> rewards = GetRewardPool();

            for (int i = 0; i < 3; i++)
            {
                EStageNodeReward reward;

                if (floor == anvilFloor && i == anvilIndex)
                {
                    reward = EStageNodeReward.Anvil;
                }
                else
                {
                    int randomIndex = Random.Range(0, rewards.Count);
                    reward = rewards[randomIndex];

                    rewards.RemoveAt(randomIndex);
                }

                _stageNodes.Add(new CStageNodeData(floor, reward));
            }
        }

        // 방랑상인은 모루가 아닌 2~3층 노드 중 하나
        List<int> merchantCandidates = new List<int>();

        for (int i = 1; i <= 6; i++)
        {
            if (_stageNodes[i].Reward != EStageNodeReward.Anvil)
            {
                merchantCandidates.Add(i);
            }
        }

        int merchantIndex =
            merchantCandidates[Random.Range(0, merchantCandidates.Count)];

        CStageNodeData merchantNode = _stageNodes[merchantIndex];

        _stageNodes[merchantIndex] = new CStageNodeData(
            merchantNode.Floor,
            merchantNode.Reward,
            true
        );

        // 4층 미니보스 노드 2개
        List<EStageNodeReward> floor4Rewards = GetRewardPool();

        for (int i = 0; i < 2; i++)
        {
            int randomIndex = Random.Range(0, floor4Rewards.Count);
            EStageNodeReward reward = floor4Rewards[randomIndex];

            floor4Rewards.RemoveAt(randomIndex);

            _stageNodes.Add(new CStageNodeData(4, reward, hasMiniBoss: true));
        }

        // 5층 스테이지 보스 노드
        _stageNodes.Add(new CStageNodeData(5, EStageNodeReward.None, hasStageBoss: true));
    }



    private void SetStageNodeData()
    {
        int index = 1;

        for (int i = 0; i < _floor2Nodes.Length; i++)
        {
            SetNode(_floor2Nodes[i], _stageNodes[index]);
            index++;
        }

        for (int i = 0; i < _floor3Nodes.Length; i++)
        {
            SetNode(_floor3Nodes[i], _stageNodes[index]);
            index++;
        }

        for (int i = 0; i < _floor4Nodes.Length; i++)
        {
            SetNode(_floor4Nodes[i], _stageNodes[index]);
            index++;
        }
    }




    private Sprite GetRewardIcon(EStageNodeReward reward)
    {
        switch (reward)
        {
            case EStageNodeReward.Artifact:
                return _artifactIcon;

            case EStageNodeReward.Tablet:
                return _tabletIcon;

            case EStageNodeReward.Dice:
                return _diceIcon;

            case EStageNodeReward.Money:
                return _moneyIcon;

            case EStageNodeReward.Experience:
                return _experienceIcon;

            case EStageNodeReward.Anvil:
                return _anvilIcon;

            case EStageNodeReward.Enchant:
                return _enchantIcon;

            default:
                return null;
        }
    }



    private void SetNode(CStageNodeButton button, CStageNodeData node)
    {
        Sprite rewardSprite = GetRewardIcon(node.Reward);

        Sprite specialSprite = null;

        if (node.HasMerchant)
        {
            specialSprite = _merchantIcon;
        }

        button.SetNodeData(node, rewardSprite, specialSprite);
    }



    private void UpdateNodeInteractable()
    {
        int currentFloor = _testFloor;
        int nextFloor = currentFloor + 1;

        foreach (CStageNodeButton node in _floor2Nodes)
        {
            node.SetInteractable(nextFloor == 2);
        }

        foreach (CStageNodeButton node in _floor3Nodes)
        {
            node.SetInteractable(nextFloor == 3);
        }

        foreach (CStageNodeButton node in _floor4Nodes)
        {
            node.SetInteractable(nextFloor == 4);
        }

        _floor5BossNode.SetInteractable(nextFloor == 5);


    }

    public void Open()
    {
        _nextStagePanel.SetActive(true);
    }


}