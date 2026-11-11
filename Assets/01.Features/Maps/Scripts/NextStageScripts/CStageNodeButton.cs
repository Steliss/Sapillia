using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CStageNodeButton : MonoBehaviour
{
    #region 인스펙터

    [SerializeField] private Image _rewardIcon;
    [SerializeField] private Image _specialIcon;
    [SerializeField] private Button _button;

    #endregion

    #region 내부변수

    private CStageNodeData _nodeData;

    #endregion

    public void SetNodeData(CStageNodeData nodeData, Sprite rewardSprite, Sprite specialSprite)
    {
        _nodeData = nodeData;

        // 보상 아이콘
        _rewardIcon.sprite = rewardSprite;
        _rewardIcon.gameObject.SetActive(rewardSprite != null);

        // 특수 아이콘
        _specialIcon.sprite = specialSprite;
        _specialIcon.gameObject.SetActive(specialSprite != null);
    }

    public void SetInteractable(bool interactable)
    {
        _button.interactable = interactable;
    }
}
