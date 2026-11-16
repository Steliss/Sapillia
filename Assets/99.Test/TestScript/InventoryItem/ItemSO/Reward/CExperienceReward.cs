using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CExperienceReward : MonoBehaviour
{
    #region 인스펙터

    [Header("상자 스프라이트")]
    [SerializeField] private GameObject _boxClose;
    [SerializeField] private GameObject _boxOpen;

    [Header("상자 열림 애니메이션")]
    [SerializeField] private Animator _boxAnimator;

    [Header("경험치 구슬 프리팹")]
    [SerializeField] private CEnemyProjectile_DropItem _expBallPrefab;
    [SerializeField] private CEnemyProjectile_DropItem _expMiniBallPrefab;

    [Header("경험치 생성 위치")]
    [SerializeField] private Transform _expSpawnPoint;

    [Header("경험치 보상 설정")]
    [SerializeField] private int _expBallCount = 3;
    [SerializeField] private int _expMiniBallCount = 5;

    [SerializeField] private int _expBallAmount = 100;
    [SerializeField] private int _expMiniBallAmount = 20;

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
        _boxAnimator.SetTrigger("Open");

        _boxClose.SetActive(false);
        _boxOpen.SetActive(true);

        
        SpawnExperience();

        Debug.Log("경험치 상자 열림 및 경험치 구슬 생성");
    }

    private void SpawnExperience()
    {
        if (_expSpawnPoint == null)
        {
            Debug.LogError("경험치 생성 위치가 없습니다.");
            return;
        }

        SpawnExpBalls(_expBallPrefab, _expBallCount, _expBallAmount);
        SpawnExpBalls(_expMiniBallPrefab, _expMiniBallCount, _expMiniBallAmount);
    }

    private void SpawnExpBalls(CEnemyProjectile_DropItem prefab, int count, int amount)
    {
        if (prefab == null || count <= 0)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            CEnemyProjectile_DropItem expBall = Instantiate(prefab, _expSpawnPoint.position, Quaternion.identity);

            expBall.SetWhatis(CEnemyProjectile_DropItem.EIm.Exp, amount, _expSpawnPoint.position);
        }
    }
}