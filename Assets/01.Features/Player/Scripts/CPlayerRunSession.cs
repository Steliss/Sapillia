using UnityEngine;

// 런타임중 데이터 생성 관리
public class CPlayerRunSession : MonoBehaviour
{
    public CPlayerGamingDataSO PlayerData { get; private set; }

    /// <summary>
    /// CDataManager 저장용 함수
    /// </summary>
    /// <returns></returns>
    public CPlayerGamingDataSO GetPlayerData()
    {
        return PlayerData;
    }

    /// <summary>
    /// CDataManager 불러오기용 함수 
    /// </summary>
    /// <param name="playerData"></param>
    public void SetPlayerData(CPlayerGamingDataSO playerData = null)
    {
        // 플레이어 정보가 없을시 새 플레이어 생성
        if (playerData == null)
        {
            PlayerData = new CPlayerGamingDataSO();
            PlayerData.Initalize();
            return;
        }


        PlayerData = playerData;
    }
    


    
}
