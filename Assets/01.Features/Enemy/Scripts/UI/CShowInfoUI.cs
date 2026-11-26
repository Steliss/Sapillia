using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CShowInfoUI : MonoBehaviour
{
    #region 아이템 설명
    [Header("보상 스크립트")]
    [SerializeField] public CArtifactRewardUI _rewardUI;
    [Header("아이템 설명 UI")]
    [SerializeField] private GameObject _InventoryInfo;
    [SerializeField] private Image _infoImage;
    [SerializeField] private TextMeshProUGUI _infoTitle;
    [SerializeField] private TextMeshProUGUI _infoDesc;
    #endregion

    #region 시너지 설명
    [Header("시너지 스크립트")]
    [SerializeField] public CInventorySynergyUI _synergy;
    [Header("시너지 상세설명 UI")]
    [SerializeField] private GameObject _synergyInfoUI;
    [SerializeField] private Image _synergyInfoUI_Icon;
    [SerializeField] private TextMeshProUGUI _synergyInfoUI_Name;
    [SerializeField] private TextMeshProUGUI _synergyInfoUI_Info;
    #endregion

    private CLocalizationManager _localization;


    private void Awake()
    {
        if (_rewardUI != null)
        {
            _rewardUI.OnMouseEnter += ShowItemInfo;
            _rewardUI.OnMouseExit += CloseInfoUI;
        }
    }



    /// <summary>
    /// 마우스를 올린 슬롯의 아이템 정보 표시
    /// </summary>
    public void ShowItemInfo(CItemSO item)
    {
        
        if (_localization == null && CManagerDontDestroy.Instance != null)
        {
            _localization = CManagerDontDestroy.Instance.LocalizationData;
        }

        _infoImage.sprite = item.Icon;

        if (item is CArtifactSO artifact)
        {
            List<string> synergyNames = new();

            foreach (EArtifactSynergy synergy in artifact.Synergies)
            {
                string key = $"item.synergy.name.{synergy.ToString().ToLowerInvariant()}";
                synergyNames.Add(_localization.GetText(key));
            }
            ShowSynergyInfo(artifact.Synergies[0]);

            _infoTitle.text = _localization.GetText(item.NameKey) + "\n <size=50%>" + string.Join(", ", synergyNames) + "</size>"; // join = 리스트의 문자열들을 , 로 구분 합쳐줌

            _infoDesc.text = _localization.GetText(item.DescriptionKey, artifact.GetDescriptionValues(0));
        }
        else
        {
            _infoTitle.text = _localization.GetText(item.NameKey);
            _infoDesc.text = _localization.GetText(item.DescriptionKey, item.GetDescriptionValues());
        }

        _InventoryInfo.SetActive(true);
    }

    private void ShowSynergyInfo(EArtifactSynergy synergy)
    {
        if (_synergyInfoUI == null)
        {
            return;
        }


        _synergyInfoUI.SetActive(true);


        // 아이콘, 텍스트 설정
        _synergyInfoUI_Icon.sprite = _synergy.SynergyInfoMap[synergy].Icon;
        string name = _localization?.GetText(_synergy.SynergyInfoMap[synergy].Name);
        _synergyInfoUI_Name.SetText($"<color=green>{name}");

        int infoCount = _synergy.SynergyInfoMap[synergy].Info.Length;
        string info = "";
        for (int i = 0; i < infoCount; i++)
        {
            info += _localization?.GetText(_synergy.SynergyInfoMap[synergy].Info[i]) + "\n";
        }
        _synergyInfoUI_Info.SetText(info);
    }

    private void CloseInfoUI()
    {
        _synergyInfoUI?.SetActive(false);
        _InventoryInfo?.SetActive(false);
    }
}
