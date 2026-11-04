using System;
using System.Collections.Generic;
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
    [SerializeField] private List<GameObject> _doors;
    [SerializeField] private List<SEnemySpawnData> _enemys;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Vector2 _randomSpawnRange;
    #endregion

    #region 내부 변수
    private int _aliveEnemys = 0;
    private bool _isBattle = false; 

    private CDataManger _data;
    private CPlayerGamingDataSO _playerData;
    private CUniTaskOnes _dataTask = new CUniTaskOnes();
    #endregion

    private void OnEnable()
    {
        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            _playerData = _data.PlayerData.GetPlayerData();
            // ReloadData();
            CLog.Log("CPlayerUI _dataTask OnEnable 작업 완료");
        });
    }
    private void OnDisable()
    {
        _dataTask?.Cancel();
    }

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
        for (int i = 0; i < _doors.Count; i++)
        {
            _doors[i].SetActive(true);
        }

        // 몬스터 스폰
        for (int i = 0; i < _enemys.Count; i++)
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
            CLog.Log("전투 종료!");
        }
    }
}
