using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CDiceReward : MonoBehaviour
{
    #region 인스펙터

    [Header("상자 스프라이트")]
    [SerializeField] private GameObject _boxClose;
    [SerializeField] private GameObject _boxOpen;


    [Header("주사위 프리팹")]
    [SerializeField] private CEnemyProjectile_DropItem _dicePrefab;

    [Header("주사위 생성 위치")]
    [SerializeField] private Transform _diceSpawnPoint;

    [Header("주사위 보상 설정")]
    [SerializeField] private int _diceCount = 2;
    [SerializeField] private int _diceAmount = 1;

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

        _boxClose.SetActive(false);
        _boxOpen.SetActive(true);

        SpawnDice(_dicePrefab, _diceCount, _diceAmount);

        Debug.Log("주사위 보상 상자 열림 및 주사위 생성");
    }

    private void SpawnDice(CEnemyProjectile_DropItem prefab, int count, int amount)
    {
        if (prefab == null || count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            CEnemyProjectile_DropItem expBall = Instantiate(prefab, _diceSpawnPoint.position, Quaternion.identity);

            expBall.SetWhatis(CEnemyProjectile_DropItem.EIm.Dice, amount, _diceSpawnPoint.position);
        }
    }
}