using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class CStageNodeData
{
    [SerializeField] private int _floor;
    [SerializeField] private EStageNodeReward _reward;

    [SerializeField] private bool _hasMerchant;
    [SerializeField] private bool _hasMiniBoss;
    [SerializeField] private bool _hasStageBoss;

    public int Floor => _floor;
    public EStageNodeReward Reward => _reward;

    public bool HasMerchant => _hasMerchant;
    public bool HasMiniBoss => _hasMiniBoss;
    public bool HasStageBoss => _hasStageBoss;

    public CStageNodeData(
        int floor,
        EStageNodeReward reward,
        bool hasMerchant = false,
        bool hasMiniBoss = false,
        bool hasStageBoss = false)
    {
        _floor = floor;
        _reward = reward;

        _hasMerchant = hasMerchant;
        _hasMiniBoss = hasMiniBoss;
        _hasStageBoss = hasStageBoss;
    }
}
