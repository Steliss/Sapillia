using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CInventorySynergyUI : MonoBehaviour
{
    #region 내부 구조체
    [Serializable]
    public struct SSynergyInfo
    {
        public EArtifactSynergy Synergy;
        public string Name;
        public string[] Info;
        public Sprite Icon;
        [Min(1)] public int LevelStepSize;
        [Min(1)] public int MaxLevel;
    }
    #endregion

    #region 인스펙터
    [Header("추가할 UI 프리팹")]
    [SerializeField] private GameObject _prefeb;
    [Header("리스트 추가할 트랜스폼(부모) -> 이 객체의 자식이여야 구독해제 안전")]
    [SerializeField] private Transform _content;
    [Header("시너지 정보")]
    [SerializeField] public SSynergyInfo[] _synergyInfo;
    [Header("상세설명 UI")]
    [SerializeField] private GameObject _synergyInfoUI;
    [SerializeField] private Image _synergyInfoUI_Icon;
    [SerializeField] private TextMeshProUGUI _synergyInfoUI_Name;
    [SerializeField] private TextMeshProUGUI _synergyInfoUI_Info;
    #endregion

    #region 내부 변수
    private Dictionary<EArtifactSynergy, CSynergyElementUI> _synergyElementMap = new Dictionary<EArtifactSynergy, CSynergyElementUI>();
    private Dictionary<EArtifactSynergy, SSynergyInfo> _synergyInfoMap = new Dictionary<EArtifactSynergy, SSynergyInfo>();
    private IReadOnlyDictionary<EArtifactSynergy, int> _synergyLevelMap;
    private CLocalizationManager _textData;
    private int _showLevel;
    private EArtifactSynergy _showSynergy;
    private readonly List<KeyValuePair<EArtifactSynergy, int>> _sortList = new List<KeyValuePair<EArtifactSynergy, int>>();
    #endregion

    /// <summary>
    /// UI에 시너지 종류와 레벨 표시
    /// </summary>
    /// <param name="synergys"></param>
    public void SetSynergy(IReadOnlyDictionary<EArtifactSynergy, int> synergys)
    {
        _synergyLevelMap = synergys;

        foreach (EArtifactSynergy key in synergys.Keys)
        {
            // 초기 생성
            if (!_synergyElementMap.ContainsKey(key) && !TryCreateSynergyPrefeb(key))
            {
                continue;
            }

            // 시너지 레벨 0이면 끄기
            if (_synergyLevelMap[key] < 1)
            {
                _synergyElementMap[key].SetActive(false);
                continue;
            }

            // 텍스트 설정 (시너지 레벨 업데이트)
            if (_textData == null && CManagerDontDestroy.Instance != null)
            {
                _textData = CManagerDontDestroy.Instance.LocalizationData;
            }

            string text = "<color=green>" + _textData?.GetText(_synergyInfoMap[key].Name) 
                + $"</color>\n{synergys[key]} / {_synergyInfoMap[key].MaxLevel}";
            _synergyElementMap[key].SetSynergyLevel(text);

        }
        // 내림차순 정렬하기
        SortElements();

    }

    // 시너지 프리팹 생성 및 딕셔너리 초기 설정
    private bool TryCreateSynergyPrefeb(EArtifactSynergy key)
    {
        if (!_synergyInfoMap.TryGetValue(key, out SSynergyInfo info))
        {
            for (int i = 0; i < _synergyInfo.Length; i++)
            {
                _synergyInfoMap[_synergyInfo[i].Synergy] = _synergyInfo[i];
            }

            if (_synergyInfoMap.Count == 0)
            {
                return false;
            }
        }

        // 프리팹 생성
        GameObject prefeb = Instantiate(_prefeb, _content);
        _synergyElementMap[key] = prefeb.GetComponent<CSynergyElementUI>();
        _synergyElementMap[key].SetSynergyIcon(_synergyInfoMap[key].Icon, key);

        // 이 객체가 파괴될 때 같이 파괴되므로 구독해제는 하지않음.
        _synergyElementMap[key].OnMouseEnter += ShowSynergyInfo;
        _synergyElementMap[key].OnMouseExit += CloseSynergyInfo;
        return true;
    }

    // 시너지 상세설명 표시
    private void ShowSynergyInfo(EArtifactSynergy synergy)
    {
        if (_synergyInfoUI == null)
        {
            return;
        }
        _synergyInfoUI.SetActive(true);

        // 현재 설정과 목표가 같으면 리턴
        int activeLevel = _synergyLevelMap[synergy] / _synergyInfoMap[synergy].LevelStepSize;
        if (_showSynergy == synergy && _showLevel == activeLevel)
        {
            return;
        }
        _showLevel = activeLevel;
        _showSynergy = synergy;

        // 아이콘, 텍스트 설정
        _synergyInfoUI_Icon.sprite = _synergyInfoMap[synergy].Icon;
        string name = _textData?.GetText(_synergyInfoMap[synergy].Name);
        _synergyInfoUI_Name.SetText($"<color=green>{name}");

        int infoCount = _synergyInfoMap[synergy].Info.Length;
        string info = "";
        bool ones = true;
        for (int i = 0; i < infoCount; i++)
        {
            if (activeLevel <= i && ones)
            {
                info += "<color=grey>";
                ones = false;
            }
            info += _textData?.GetText(_synergyInfoMap[synergy].Info[i]) + "\n";
        }
        _synergyInfoUI_Info.SetText(info);
    }

    private void CloseSynergyInfo()
    {
        _synergyInfoUI?.SetActive(false);
    }

    private void SortElements()
    {
        _sortList.Clear();

        foreach (var kv in _synergyLevelMap)
        {
            if (_synergyElementMap.ContainsKey(kv.Key))
            {
                _sortList.Add(kv);
            }
        }

        // 레벨 내림차순, 같으면 Enum 순서
        _sortList.Sort((a, b) =>
        {
            int cmp = b.Value.CompareTo(a.Value);
            return cmp != 0 ? cmp : a.Key.CompareTo(b.Key);
        });

        // 하이어라키를 리스트 순서대로 정리
        for (int i = 0; i < _sortList.Count; i++)
        {
            _synergyElementMap[_sortList[i].Key].transform.SetSiblingIndex(i);
        }
    }
}
