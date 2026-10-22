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
            CLog.Warn("CSaveData: 불러올 데이터를 받을 대상이 없습니다.");
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

//
public class CDataManger : MonoBehaviour
{
    [SerializeField] private CFlowData _flowData; // 진행 상황 및 옵션

    public bool SaveSettings()
    {
        return CSaveData.Save(_flowData.CurrentSetting, "Setting");
    }

    public bool LoadSettings()
    {
        return CSaveData.Load(_flowData.CurrentSetting, "Setting");
    }

    public bool SaveGame(int slot)
    {
        return CSaveData.Save(_flowData.CurrentFlow, $"CurrentGame_{slot}");
    }

    public bool LoadGame(int slot)
    {
        return CSaveData.Load(_flowData.CurrentFlow, $"CurrentGame_{slot}");
    }

}