using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CInventorySynergyUI : MonoBehaviour
{
    #region 인스펙터
    [Header("추가할 UI 프리팹")]
    [SerializeField] private GameObject _prefeb;
    [Header("리스트 추가할 트랜스폼(부모)")]
    [SerializeField] private Transform _content;
    [Header("텍스트")]
    [SerializeField] private string _robust = "견고";
    [SerializeField] private string _precision = "정밀";
    [SerializeField] private string _windsong = "바람노래";
    #endregion

    #region 내부 변수
    private Dictionary<EArtifactSynergy, GameObject> _synergys = new Dictionary<EArtifactSynergy, GameObject>();
    private CLocalizationManager _textData;
    #endregion

    public void SetSynergy(Dictionary<EArtifactSynergy, int> synergys)
    {
        foreach (EArtifactSynergy key in synergys.Keys)
        {
            if (!_synergys.ContainsKey(key))
            {
                _synergys[key] = Instantiate(_prefeb, _content);
            }

            if (synergys[key] == 0)
            {
                _synergys[key].SetActive(false);
                continue;
            }

            if (_textData == null && CManagerDontDestroy.Instance != null)
            {
                _textData = CManagerDontDestroy.Instance.LocalizationData;
            }
            string text = "";

            switch (key)
            {
                case EArtifactSynergy.Robust:
                    text = _textData.GetText(_robust);
                    break;

                case EArtifactSynergy.Precision:
                    text = _textData.GetText(_precision);
                    break;

                case EArtifactSynergy.WindSong:
                    text = _textData.GetText(_windsong);
                    break;
            }


            text += $"\n{synergys[key]} / 10";
            CSynergyElementUI element = _synergys[key].GetComponent<CSynergyElementUI>();
            element.SetSynergyInfo(key, text);
        }


        // 내림차순 정렬하기
    }

}
