using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct SEnemySpawnData
{
    [SerializeField] public GameObject Enemy;
    [SerializeField] public int Count;
}


public class CEnemySpawner : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private List<GameObject> _doors;
    [SerializeField] private List<SEnemySpawnData> _enemys;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Vector2 _randomSpawnRange;
    #endregion

    #region 내부 변수
    private List<GameObject> _aliveEnemys = new List<GameObject>();
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
        _aliveEnemys.Clear();
        for (int i = 0; i < _enemys.Count; i++)
        {
            for (int j = 0; j < _enemys[i].Count; j++)
            {
                GameObject enemy;
                if (CObjectPoolManager.Instance != null)
                {
                    enemy = CObjectPoolManager.Instance.GetPoolObject(_enemys[i].Enemy);
                    Vector2 ranPos = transform.position;
                    
                    // 랜덤범위 스폰 적용
                    float ranX = UnityEngine.Random.Range(-_randomSpawnRange.x, _randomSpawnRange.x);
                    float ranY = UnityEngine.Random.Range(-_randomSpawnRange.y, _randomSpawnRange.y);
                    ranPos.x += ranX * 0.5f;
                    ranPos.y += ranY * 0.5f;
                    enemy.transform.position = ranPos;

                    _aliveEnemys.Add(enemy);
                }
            }
        }

        _isBattle = true;
        _playerData?.SetIsBattle(true);
    }

    private void Update()
    {
        if (!_isBattle)
        {
            return;
        }

        // 모두 죽었으면 전투 종료
        if (_aliveEnemys.Count <= 0)
        {
            _isBattle = false;
            _playerData?.SetIsBattle(false);
            gameObject.SetActive(false);
            CLog.Log("전투 종료!");
            return;
        }

        // 몬스터가 살아있는지 체크
        for (int i = _aliveEnemys.Count - 1; i >= 0; i--)
        {
            if (!_aliveEnemys[i].activeSelf)
            {
                _aliveEnemys.RemoveAt(i);
                CLog.Log($"남은 적 : {_aliveEnemys.Count}마리");
            }
        }

    }
}
