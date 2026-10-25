using System;
using System.Collections.Generic;
using UnityEngine;

public enum ELanguage
{
    Korean,
    English
}

public class CLocalizationManager : MonoBehaviour
{
    [SerializeField] private CDataManger _flowData;
    [SerializeField] private TextAsset _koreanJson;
    [SerializeField] private TextAsset _englishJson;

    private readonly Dictionary<string, string> _texts = new Dictionary<string, string>();

    public event Action OnLanguageChanged;


    private void Awake()
    {
        if(_flowData == null)
        {
            CLog.Log("_FlowDataSO : Null");
        }
        if(_koreanJson == null)
        {
            CLog.Log("_koreanJson : Null");
        }
        if(_englishJson == null)
        {
            CLog.Log("_englishJson : Null");
        }

        LoadLanguage(_flowData.CurrentSetting.CurrentLanhuage);
    }

    /// <summary>
    ///  언어 설정 변경 함수 
    /// </summary>
    /// <param name="language"></param>
    public void ChangeLanguage(ELanguage language)
    {
        LoadLanguage(language);
    }

    // 실제 언어 변경 로직
    private void LoadLanguage(ELanguage language)
    {
        TextAsset json;

        switch (language)
        {
            case ELanguage.English:
                json = _englishJson;
                break;

            default:
                json = _koreanJson;
                break;
        }

        if (json == null)
        {
            CLog.Error($"번역 Json 연결 확인 : {language}");
            return;
        }

        CLocalizationData data = JsonUtility.FromJson<CLocalizationData>(json.text);

        if (data == null)
        {
            CLog.Error($"번역 데이터가 올바르지 않습니다");
            return;
        }

        _texts.Clear();

        AddEntries(data.items);
        AddEntries(data.talk);
        AddEntries(data.scene);

        _flowData.CurrentSetting.SetCurrentLanhuage(language);

        OnLanguageChanged?.Invoke();
    }

    // 번역 데이터를 Dictionary에 등록
    private void AddEntries(CLocalizationEntry[] entries)
    {
        if (entries == null)
        {
            return;
        }

        foreach (CLocalizationEntry entry in entries)
        {
            if (entry == null || string.IsNullOrWhiteSpace(entry.key))
            {
                CLog.Error("번역 데이터에 비어 있는 항목 또는 키가 있습니다.");
                continue;
            }

            if (_texts.ContainsKey(entry.key))
            {
                CLog.Error($"번역 키가 중복되었습니다 : {entry.key}");
                continue;
            }

            _texts.Add(entry.key, entry.value ?? entry.key);
        }
    }


    /// <summary>
    /// 불러온 키에 맞는 번역 출력 
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    public string GetText(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            CLog.Log($"키가 없습니다 : {key}");
            return string.Empty;
        }

        if (_texts.TryGetValue(key, out string text))
        {
            return text;
        }

        CLog.Log($"번역 문장이 없습니다 : {key}");
        return key;
    }
}