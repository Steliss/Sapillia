using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CMoneyReward : MonoBehaviour
{
    #region 인스펙터

    [Header("상자 스프라이트")]
    [SerializeField] private GameObject _boxClose;
    [SerializeField] private GameObject _boxOpen;

    [Header("상자 열림 애니메이션")]
    [SerializeField] private Animator _boxAnimator;

    [Header("돈 프리팹")]
    [SerializeField] private CEnemyProjectile_DropItem _moneyPrefab;
    [SerializeField] private CEnemyProjectile_DropItem _moneyMiniPrefab;

    [Header("돈 생성 위치")]
    [SerializeField] private Transform _moneySpawnPoint;

    [Header("돈 보상 설정")]
    [SerializeField] private int _moneyCount = 5;
    [SerializeField] private int _moneyAmount = 100;
    [SerializeField] private int _moneyMiniCount = 5;
    [SerializeField] private int _moneyMiniAmount = 10;

    #endregion

    #region 내부변수

    private bool _isPlayerNearby = false;
    private bool _isOpened = false;

    #endregion

    private void Start()
    {
        _boxClose.SetActive(true);
        _boxOpen.SetActive(false);
    }

    private void Update()
    {
        if (!_isPlayerNearby || _isOpened)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            OpenBox();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        _isPlayerNearby = false;
    }

    private void OpenBox()
    {
        _isOpened = true;

        if (_boxAnimator != null)
        {
            _boxAnimator.SetTrigger("Open");
        }

        _boxClose.SetActive(false);
        _boxOpen.SetActive(true);

        SpawnMoneys(_moneyPrefab, _moneyCount, _moneyAmount);
        SpawnMoneys(_moneyMiniPrefab, _moneyMiniCount, _moneyMiniAmount);

        Debug.Log("돈 보상 상자 열림 및 돈 생성");
    }


    private void SpawnMoneys(CEnemyProjectile_DropItem prefab, int count, int amount)
    {
        if (prefab == null || count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            CEnemyProjectile_DropItem expBall = Instantiate(prefab, _moneySpawnPoint.position, Quaternion.identity);

            expBall.SetWhatis(CEnemyProjectile_DropItem.EIm.Money, amount, _moneySpawnPoint.position);
        }
    }
}
