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
    [Header("전투 시작 이펙트")]
    [SerializeField] private GameObject _startEffect;
    [SerializeField] private Animator _ani;
    [SerializeField] private string _aniClose = "Close";
    [SerializeField] private string _aniOpen = "Open";
    [SerializeField] private string _soundGate = "GateOpenBone";
    #endregion

    #region 내부 변수
    private int _aliveEnemys = 0;
    private bool _isBattle = false;
    private CObjectPoolManager _pool;
    private int _hashClose;
    private int _hashOpen;

    private CPlayerGamingDataSO _playerData;
    #endregion

    private void OnEnable()
    {
        if (_ani != null)
        {
            _hashClose = Animator.StringToHash(_aniClose);
            _hashOpen = Animator.StringToHash(_aniOpen);
        }
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

        if (_pool == null)
        {
            _pool = CObjectPoolManager.Instance;
            if (_pool == null)
            {
                CLog.Warn($"{name} : 오브젝트 풀이 없음. 몬스터 생성 취소");
                return;
            }
        }

        // 전투 시작 이펙트 생성
        if (_startEffect != null)
        {
            GameObject effect = _pool.GetPoolObject(_startEffect, 1f);
            effect.transform.SetParent(collision.transform);
            effect.transform.localPosition = new Vector2(0, 5f);
        }

        // 문 활성화
        for (int i = 0; i < _doors.Length; i++)
        {
            _doors[i].SetActive(true);
        }

        if (_ani != null)
        {
            _ani.Play(_hashClose);
            if (CManagerDontDestroy.Instance != null)
            {
                CManagerDontDestroy.Instance.Sound.PlayEffect(_soundGate, transform.position);
            }
        }

        // 몬스터 스폰
        for (int i = 0; i < _enemys.Length; i++)
        {
            for (int j = 0; j < _enemys[i].Count; j++)
            {
                GameObject enemy = _pool.GetPoolObject(_enemys[i].Enemy);
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

        _isBattle = true;

        if (_playerData == null)
        {
            if (CManagerDontDestroy.Instance != null)
            {
                _playerData = CManagerDontDestroy.Instance.DataManger.PlayerData.GetPlayerData();
            }
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

            if (_ani != null)
            {
                _ani.Play(_hashOpen);
                if (CManagerDontDestroy.Instance != null)
                {
                    CManagerDontDestroy.Instance.Sound.PlayEffect(_soundGate, transform.position);
                }
            }

            CLog.Log("전투 종료!");
        }
    }
}
