using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CNextStageTrigger : MonoBehaviour, IInputCommand
{
    #region 인스펙터


    #endregion


    #region 내부변수

    private CStageNodeUI _stageNodeUI;

    private bool _isPlayerInside = false;

    private CInputCommandHub _inputHub;
    #endregion



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

    private void OnEnable()
    {
        _inputHub = FindFirstObjectByType<CInputCommandHub>();

        if (_inputHub == null)
        {
            Debug.LogError("CInputCommandHub를 찾을 수 없습니다.", this);
            return;
        }

        _inputHub.Register(EKeyAction.Interaction, this);
    }

    public void Execute()
    {
        if (!_isPlayerInside)
        {
            return;
        }

        if (_stageNodeUI != null && _stageNodeUI.IsOpened)
        {
            return;
        }

        OpenNextStageNode();
    }

    private void OnDisable()
    {
        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Interaction, this);
        }


        _isPlayerInside = false;
    }
}
