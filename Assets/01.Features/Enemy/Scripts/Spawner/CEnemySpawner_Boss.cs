using System;
using UnityEngine;

public class CEnemySpawner_Boss : MonoBehaviour
{
    #region 내부 구조체
    [Serializable]
    public struct SEnemyRewardData
    {
        [SerializeField] public GameObject Reward;
        [SerializeField] public Vector2 PosPlus;
    }
    #endregion

    #region 인스펙터
    [Header("보스와 보상")]
    [SerializeField] private GameObject[] _doors;
    [SerializeField] private GameObject[] _boss;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private SEnemyRewardData[] _rewards;
    [SerializeField] private CEnemyBoss01Production _production;
    #endregion

    #region 내부 변수
    private bool _isBattle = false;
    private Transform _targetTr;
    private int _phase = 0;
    private CUniTaskOnes _uni = new CUniTaskOnes();

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
        for (int i = 0; i < _doors.Length; i++)
        {
            _doors[i].SetActive(true);
        }

        _targetTr = collision.transform;


        // 연출 시작
        _uni.Run(async ct => 
        {
            await _production.StartProduction(ct, _targetTr);
            // 연출 종료
            BossSpawn();
            _isBattle = true;
            _playerData?.SetIsBattle(true);
        });
    }

    private void BossSpawn()
    {
        // 몬스터 스폰
        if (CObjectPoolManager.Instance != null)
        {
            GameObject boss = CObjectPoolManager.Instance.GetPoolObject(_boss[_phase]);
            boss.transform.parent = transform;
            boss.transform.position = _targetTr.position;
            CLog.Log($"소환 위치 : {_targetTr.position}");
        }
    }


    public void EnemyDie()
    {
        if (_phase == 0)
        {
            _phase = 1;
            // 페이즈 연출 필요 후 보스스폰
            _uni.Run(async ct =>
            {
                bool randomLR = UnityEngine.Random.Range(0, 2) == 0;
                await _production.PhaseProduction(ct, randomLR);
                BossSpawn();
            });
        }
        else
        {
            _uni.Run(async ct =>
            {
                // 사망 연출 필요
                await _production.DieProduction(ct);

                _isBattle = false;
                _playerData?.SetIsBattle(false);

                // 보상 드롭
                for (int i = 0; i < _rewards.Length; i++)
                {
                    Vector2 pos = (Vector2)transform.position + _rewards[i].PosPlus;
                    Instantiate(_rewards[i].Reward, pos, Quaternion.identity);
                }

                CLog.Log("전투 종료!");
                // 문 비활성화
                for (int i = 0; i < _doors.Length; i++)
                {
                    _doors[i].SetActive(false);
                }
            });
            
        }
    }
}
