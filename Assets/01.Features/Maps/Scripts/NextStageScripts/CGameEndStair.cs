using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CGameEndStair : MonoBehaviour
{
    #region 인스펙터

    [Header("게임 흐름")]
    [SerializeField] private CGameFlow _gameFlow;

    #endregion

    #region 내부변수

    private bool _isPlayerInside = false;

    #endregion

    private void Start()
    {
        _gameFlow = FindFirstObjectByType<CGameFlow>();
    }

    private void Update()
    {
        if (!_isPlayerInside)
            return;

        if (Input.GetKeyDown(KeyCode.F))
        {
            if (_gameFlow == null)
                return;

            _gameFlow.WinGame();
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        _isPlayerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        _isPlayerInside = false;
    }



}
