using System;
using UnityEngine;

public class CEnemySpawner : MonoBehaviour
{
    #region 내부 구조체
    [Serializable]
    public struct SEnemySpawnData
    {
        [SerializeField] public GameObject Enemy;
        [SerializeField] public int Count;
    }
    #endregion

    #region 인스펙터
    [SerializeField] private GameObject[] _doors;
    [SerializeField] private SEnemySpawnData[] _enemys;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Vector2 _randomSpawnRange;
    [Header("전투 종료시 보상 프리팹 (비워도 됨)")]
    [SerializeField] private GameObject _reward;
    [Header("보상 위치 보정 (기본 스포너 위치)")]
    [SerializeField] private Vector2 _rewardPosPlus;
    #endregion

    #region 내부 변수
    private int _aliveEnemys = 0;
    private bool _isBattle = false;

    //private CUniTaskOnes _dataTask = new CUniTaskOnes();
    //private CDataManger _data;
    private CPlayerGamingDataSO _playerData;
    #endregion
    /*
    private void OnEnable()
    {
        if (_playerData == null)
        {
            _dataTask.Run(async token =>
            {
                _data = await CManagerDontDestroy.DataMangerAsync(token);
                _playerData = _data.PlayerData.GetPlayerData();
                CLog.Log("CPlayerUI _dataTask OnEnable 작업 완료");
            });
        }
    }
    
    private void OnDisable()
    {
        _dataTask?.Cancel();
    }
    */

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isBattle)
        {
            return;
        }

        if (((1 << collision.gameObject.layer) & _targetLayer) == 0)
        {
            return;
        }

        CLog.Log("전투 시작!");

        // 트리거 끄기
        GetComponent<Collider2D>().enabled = false;

        // 문 활성화
        for (int i = 0; i < _doors.Length; i++)
        {
            _doors[i].SetActive(true);
        }

        // 몬스터 스폰
        for (int i = 0; i < _enemys.Length; i++)
        {
            for (int j = 0; j < _enemys[i].Count; j++)
            {
                GameObject enemy;
                if (CObjectPoolManager.Instance != null)
                {
                    enemy = CObjectPoolManager.Instance.GetPoolObject(_enemys[i].Enemy);
                    Vector2 ranPos = transform.position;
                    enemy.transform.parent = transform;
                    
                    // 랜덤범위 스폰 적용
                    float ranX = UnityEngine.Random.Range(-_randomSpawnRange.x, _randomSpawnRange.x);
                    float ranY = UnityEngine.Random.Range(-_randomSpawnRange.y, _randomSpawnRange.y);
                    ranPos.x += ranX * 0.5f;
                    ranPos.y += ranY * 0.5f;
                    enemy.transform.position = ranPos;

                    _aliveEnemys++;
                }
            }
        }

        _isBattle = true;

        if (_playerData == null)
        {
            _playerData = CManagerDontDestroy.Instance.DataManger.PlayerData.GetPlayerData();
        }
        _playerData?.SetIsBattle(true);
    }
    
    public void EnemyDie()
    {
        if (_aliveEnemys > 1)
        {
            _aliveEnemys--;
        }
        else
        {
            _isBattle = false;
            _playerData?.SetIsBattle(false);
            gameObject.SetActive(false);

            if (_reward != null)
            {
                Vector2 pos = (Vector2)transform.position + _rewardPosPlus;
                Instantiate(_reward, pos, Quaternion.identity);
            }


            CLog.Log("전투 종료!");
        }
    }
}
