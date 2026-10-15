using System;
using System.IO;
using System.Text;
using UnityEngine;

/// <summary>
/// SO 저장 호출 함수 저장 클래스 
/// </summary>
public static class CSaveData
{
    // fileName에는 확장자를 제외한 이름을 전달한다. 예: "Player"
    // 같은 이름으로 저장하면 기존 파일을 덮어쓴다.
    // Unity 메인 스레드에서 호출한다.

    /// <summary>
    /// SO 저장 호출 함수. 
    /// </summary>
    /// <param name="data"> SO를 넣어주세요</param>
    /// <param name="fileName"> 저장할 폴더 이름</param>
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

            File.WriteAllText(fileName, json);
                
            CLog.Log($"CSaveData: 저장 완료 - {fileName}");
            return true;
        }
        catch (Exception exception)
        {
            CLog.Error($"CSaveData: 저장 실패 ({fileName})\n{exception}");
            return false;
        }
    }
}