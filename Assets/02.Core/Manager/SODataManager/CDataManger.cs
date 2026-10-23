using System;
using System.IO;
using UnityEngine;

#region 저장 불러오기

/// <summary>
/// 공통 JSON 저장 및 불러오기
/// </summary>
public static class CSaveData
{
    public static bool Save(object data, string fileName)
    {
        if (data == null)
        {
            CLog.Warn("CSaveData: 저장할 데이터가 없습니다.");
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(Application.persistentDataPath, fileName + ".json");

            File.WriteAllText(path, json);

            CLog.Log($"CSaveData: 저장 완료 - {path}");
            return true;
        }
        catch (Exception exception)
        {
            CLog.Error($"CSaveData: 저장 실패 ({fileName})\n{exception}");
            return false;
        }
    }

    public static bool Load(object data, string fileName)
    {
        if (data == null)
        {
            CLog.Warn("CSaveData: 데이터를 받을 대상이 없습니다.");
            return false;
        }

        try
        {
            string path = Path.Combine(Application.persistentDataPath, fileName + ".json");

            if (!File.Exists(path))
            {
                CLog.Warn($"CSaveData: 저장 파일 없음 - {path}");
                return false;
            }

            string json = File.ReadAllText(path);

            JsonUtility.FromJsonOverwrite(json, data);

            CLog.Log($"CSaveData: 불러오기 완료 - {path}");
            return true;
        }
        catch (Exception exception)
        {
            CLog.Error($"CSaveData: 불러오기 실패 ({fileName})\n{exception}");
            return false;
        }
    }
}

#endregion

// ※ TODO: Json 하나로 세이브 로드 관리 
public class CDataManger : MonoBehaviour
{
    [SerializeField] private CFlowData _flowData; // 진행 상황 및 옵션

    public CSettings CurrentSetting => _flowData.CurrentSetting;
    public CSettings DefaultSetting => _flowData.DefaultSetting;

    public CFlowGame CurrentFlow => _flowData.CurrentFlow;
    public CFlowGame DefaultFlow => _flowData.DefalutFlow;


    // 데이터 로딩
    private void Awake()
    {
        string path = Path.Combine(Application.persistentDataPath, "Setting.json");

        AwakeSetting(path);

        // 슬롯 세이브 관련 로딩은 CTitle
    }

    // 세팅값 
    private void AwakeSetting(string path)
    {
        // 최초실행
        if (!File.Exists(path))
        {
            ResetSettings();
            SaveSettings();
            return;
        }

        if(!LoadSettings())
        {   
            CLog.Warn("세팅 불러오기 실패");
        }
    }


    /// <summary>
    /// 설정 리셋 
    /// </summary>
    /// <returns></returns>
    public bool ResetSettings()
    {
        return CSaveData.Save(_flowData.DefaultSetting, "Setting");
    }

    /// <summary>
    /// 설정 저장
    /// </summary>
    /// <returns></returns>
    public bool SaveSettings()
    {
        return CSaveData.Save(_flowData.CurrentSetting, "Setting");
    }

    /// <summary>
    /// 설정 블러오기
    /// </summary>
    /// <returns></returns>
    public bool LoadSettings()
    {
        return CSaveData.Load(_flowData.CurrentSetting, "Setting");
    }

    /// <summary>
    /// 게임 진행 저장
    /// </summary>
    /// <returns></returns>
    public bool SaveGame(int slot)
    {
        return CSaveData.Save(_flowData.CurrentFlow, $"CurrentGame_{slot}");
    }

    /// <summary>
    /// 게임 진행 불러오기
    /// </summary>
    /// <returns></returns>
    public bool LoadGame(int slot)
    {
        return CSaveData.Load(_flowData.CurrentFlow, $"CurrentGame_{slot}");
    }

}