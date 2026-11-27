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

    // 플레이어 참조
    private Transform _playerTransform;
    private bool _isPlayerNearby;

    #endregion

    #region Unity

    private void OnEnable()
    {
        // 플레이어 참조 설정
        BindPlayer();

        _weaponUI.OnClosed += HandleWeaponClosed;

        _inputTask.Run(async token =>
        {
            CInputCommandHub hub = await CManagerDontDestroy.InputCommandHubAsync(token);

            if (token.IsCancellationRequested || !isActiveAndEnabled || hub == null)
            {
                return;
            }

            _inputHub = hub;

            _inputHub.Register(EKeyAction.Interaction, (IInputCommand)this);
        });

        
    }

    private void OnDisable()
    {
        _inputTask.Cancel();

        if (_inputHub != null)
        {
            _inputHub.Unregister(EKeyAction.Interaction, (IInputCommand)this);
            _inputHub.UnregisterOpenedUI(this);
        }

        _isPlayerNearby = false;

        if (_weaponUI != null)
        {
            _weaponUI.OnClosed -= HandleWeaponClosed;
            _weaponUI.Close();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            _isPlayerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (IsPlayer(other))
        {
            _isPlayerNearby = false;
        }
    }

    #endregion

    #region Input

    // F키
    public void Execute()
    {
        if (!_isPlayerNearby || _weaponUI == null || _inputHub == null)
        {
            return;
        }

        if (!_weaponUI.IsOpen)
        {
            _weaponUI.Open();

            _inputHub.RegisterOpenedUI(this);
        }
    }

    // Esc키
    public void ExecuteESC()
    {
        if (_weaponUI != null && _weaponUI.IsOpen && _inputHub != null)
        {
            _weaponUI.Close();
        }
    }

    private void HandleWeaponClosed()
    {
        _inputHub.UnregisterOpenedUI(this);
    }
    #endregion

    #region 플레이어 참조

    private void BindPlayer()
    {
        _playerTransform = null;

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        if (player == null)
        {
            CLog.Warn("[Anvil] 플레이어를 찾지 못했습니다.");
            return;
        }

        _playerTransform = player.transform;

        CLog.Log("[Anvil] 플레이어 연결 완료");
    }

    private bool IsPlayer(Collider2D other)
    {
        if (other == null)
        {
            return false;
        }

        // 플레이어가 나중에 생성된 경우 보완
        if (_playerTransform == null)
        {
            if (other.CompareTag("Player"))
            {
                _playerTransform = other.transform;
            }
            else
            {
                return false;
            }
        }

        // 플레이어 본체와 자식 Collider 모두 인식
        return other.transform == _playerTransform || other.transform.IsChildOf(_playerTransform);
    }

    #endregion
}
