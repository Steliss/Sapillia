using System;
using System.IO;
using UnityEngine;

/// <summary>
/// SO 저장 호출 함수 저장 클래스 
/// </summary>
public static class CSaveData
{
    /// <summary>
    /// SO 저장 호출 함수. 
    /// </summary>
    /// <param name="data"> 저장할 데이터가 들어갈 SO</param>
    /// <param name="fileName"> 저장할 파일의 이름</param>
    /// <returns></returns>
    public static bool Save(ScriptableObject data, string fileName)
    {
        if (data == null)
        {
            CLog.Warn("CSaveData: SO = Null");
            return false;
        }

        try
        {
            string json = JsonUtility.ToJson(data, true);
            string path = Path.Combine(Application.persistentDataPath, fileName + ".json");

            File.WriteAllText(path, json);

            CLog.Log($"CSaveData: 저장 완료 - {fileName}");
            return true;
        }
        catch (Exception exception)
        {
            CLog.Error($"CSaveData: 저장 실패 ({fileName})\n{exception}");
            return false;
        }
    }

    /// <summary>
    /// SO 불러오기 호출 함수.
    /// </summary>
    /// <param name="data"> 불러올 데이터를 저장할 SO</param>
    /// <param name="fileName"> 풀러올 파일의 이름</param>
    /// <returns></returns>
    public static bool Load(ScriptableObject data, string fileName)
    {
        if (data == null)
        {
            CLog.Warn("CSaveData: SO = Null");
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