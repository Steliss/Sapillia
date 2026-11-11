using UnityEngine;

public class CAnvilInteractable :
    MonoBehaviour, IInputCommand, IESCCommand
{
    #region 인스펙터

    [Header("플레이어")]
    [SerializeField] private Collider2D _playerCollider;

    [Header("무기 선택 UI")]
    [SerializeField] private CWeaponSelectUI _weaponUI;

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

            if (token.IsCancellationRequested || !isActiveAndEnabled || hub == null)
            {
                return;
            }

            _inputHub = hub;

            _inputHub.Register(EKeyAction.Interaction, (IInputCommand)this);
            _inputHub.Register(EKeyAction.ESC, (IInputCommand)this);
        });
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

        if (_weaponUI != null)
        {
            _weaponUI.Close();
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

    // F키
    public void Execute()
    {
        if (!_isPlayerNearby || _weaponUI == null)
        {
            return;
        }

        if (!_weaponUI.IsOpen)
        {
            _weaponUI.Open();
        }
    }

    // Esc키
    public void ExecuteESC()
    {
        if (_weaponUI != null && _weaponUI.IsOpen)
        {
            _weaponUI.Close();
        }
    }

    #endregion
}
