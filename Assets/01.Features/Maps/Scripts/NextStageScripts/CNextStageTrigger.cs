using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CNextStageTrigger : MonoBehaviour
{
    #region 인스펙터


    #endregion


    #region 내부변수

    private CStageNodeUI _stageNodeUI;

    private bool _isPlayerInside = false;

    #endregion



    private void Update()
    {
        if (!_isPlayerInside)
        {
            return;
        }

        if (_stageNodeUI != null && _stageNodeUI.IsOpened)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            OpenNextStageNode();
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

    private void OpenNextStageNode()
    {
        if (_stageNodeUI == null)
        {
            _stageNodeUI = FindFirstObjectByType<CStageNodeUI>();
        }

        if (_stageNodeUI == null)
        {
            Debug.LogError("CStageNodeUI를 찾을 수 없습니다.");
            return;
        }

        _stageNodeUI.Open();
    }


    private void OnDisable()
    {
        _isPlayerInside = false;
    }
}
