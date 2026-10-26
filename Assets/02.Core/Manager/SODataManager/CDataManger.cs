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
/// <summary>
/// 새 프로필 들어갈때 신규 생성이라 매니저 클래스에서 _currentFlow값 보관해서 쓰면 안됨. 
/// </summary>
public class CDataManger : MonoBehaviour
{
    [SerializeField] private CSettings _defaultSetting = new CSettings();
    [SerializeField] private CSettings _currentSetting = new CSettings();
    [SerializeField] private CFlowGame _defalutFlow = new CFlowGame();
    [SerializeField] private CFlowGame _currentFlow = new CFlowGame();
    [SerializeField] private CSettingUI cSettingUI;
    [SerializeField] private CPlayerRunSession _playerData; // 플레이어 데이터


    // ※ Todo :  아이템 관련
    /*
        아이템 이름
        아이템 위치 
        아이템 종류 
        아이템 인덱스 
     */



    public CFlowGame CurrentFlow => _currentFlow;
    public CSettings CurrentSetting => _currentSetting;
    public CPlayerRunSession PlayerData => _playerData;

    // 데이터 로딩
    private void OnEnable()
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

        SaveSettings();
    }


    /// <summary>
    /// 설정 리셋 
    /// </summary>
    /// <returns></returns>
     public bool ResetSettings()
    {
        if (!CSaveData.Save(_defaultSetting, "Setting"))
        {
            return false;
        }

        return LoadSettings();
    }

    /// <summary>
    /// 설정 저장
    /// </summary>
    /// <returns></returns>
    public bool SaveSettings()
    {
        return CSaveData.Save(_currentSetting, "Setting");
    }

    /// <summary>
    /// 설정 블러오기
    /// </summary>
    /// <returns></returns>
    public bool LoadSettings()
    {
        return CSaveData.Load(_currentSetting, "Setting");
    }

    // 게임 진행 + 플레이어 저장
    public bool SaveGame(int slot)
    {
        CPlayerGamingDataSO player = _playerData.GetPlayerData();

        if (player == null || !player.IsInitialized)
        {
            CLog.Warn("저장할 플레이어 데이터가 없습니다.");
            return false;
        }

        CGameSaveData data = new CGameSaveData
        {
            Flow = _currentFlow,
            Player = player
        };

        return CSaveData.Save(data, $"Game_{slot}");
    }

    // 게임 진행 + 플레이어 불러오기
    public bool LoadGame(int slot)
    {
        string path = Path.Combine(Application.persistentDataPath, $"Game_{slot}" + ".json");

        // 파일이 없으면 새 게임 생성
        if (!File.Exists(path))
        {
            _playerData.SetPlayerData();
            _currentFlow = new CFlowGame();
            JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(_defalutFlow), _currentFlow);
            SaveGame(slot);
            return true;
        }

        CGameSaveData data = new CGameSaveData();

        // 파일은 있지만 불러오기에 실패
        if (!CSaveData.Load(data, $"Game_{slot}"))
        {
            return false;
        }

        // 필요한 정보가 누락된 경우
        if (data.Flow == null || data.Player == null)
        {
            CLog.Warn("저장 파일에 진행 또는 플레이어 데이터가 없습니다.");
            return false;
        }

        JsonUtility.FromJsonOverwrite(JsonUtility.ToJson(data.Flow), _currentFlow);

        // 플레이어 복원은 런 세션에서 처리
        _playerData.SetPlayerData(data.Player);

        return true;
    }
}