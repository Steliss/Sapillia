using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CCostumeInteractable : MonoBehaviour, IInputCommand, IESCCommand
{
    #region 인스펙터

    [Header("플레이어")]
    [SerializeField] private Collider2D _playerCollider;

    [Header("코스튬 UI")]
    [SerializeField] private CCostumeSelectUI _costumeUI;

    #endregion

    #region 내부변수

    private CInputCommandHub _inputHub;
    private CUniTaskOnes _inputTask = new CUniTaskOnes();

    private bool _isPlayerNearby;

    #endregion

    #region Unity

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            CInputCommandHub hub = await CManagerDontDestroy.InputCommandHubAsync(token);

            if (token.IsCancellationRequested ||!isActiveAndEnabled || hub == null)
            {
                return;
            }

            _inputHub = hub;

            // F키
            _inputHub.Register(EKeyAction.Interaction, (IInputCommand)this);

            // Esc키
            _inputHub.Register(EKeyAction.ESC, (IInputCommand)this);
        });

        CLog.Log("[CCostumeInteractable] Input매니저 등록 완료");
    }

    private void OnDisable()
    {
        _inputTask.Cancel();

        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Interaction, (IInputCommand)this);

            _inputHub.Unregister(EKeyAction.ESC, (IInputCommand)this);
        }

        _isPlayerNearby = false;

        if (_costumeUI != null)
        {
            _costumeUI.Close();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other == _playerCollider)
        {
            _isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other == _playerCollider)
        {
            _isPlayerNearby = false;
        }
    }
    #endregion

    #region Input

    // F키 입력
    public void Execute()
    {
        if (!_isPlayerNearby || _costumeUI == null)
        {
            return;
        }

        if (!_costumeUI.IsOpen)
        {
            _costumeUI.Open();
        }
    }

    // Esc키 입력
    public void ExecuteESC()
    {
        if (_costumeUI != null && _costumeUI.IsOpen)
        {
            _costumeUI.Close();
        }
    }

    #endregion


}

