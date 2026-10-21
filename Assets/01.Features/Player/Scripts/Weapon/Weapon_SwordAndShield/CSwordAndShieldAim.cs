using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class CSwordAndShieldAim : MonoBehaviour
{
    #region 인스펙터

    [Header("Weapon")]
    [SerializeField] private Transform _weaponPivot;

    [Header("Player Transform")]
    [SerializeField] private Transform _player;

    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("CPlayerFacing")]
    [SerializeField] private CPlayerFacing _playerFacing;

    [Header("Aim Limit")]
    [Range(0f, 89f)]
    [SerializeField] private float _maxDownAngle = 45f;
    #endregion

    #region 변수
    private Vector2 _aimDirection = Vector2.right;

    // 무기 회전 잠금(전투)
    private bool _isAimLocked;

    #endregion

    #region 프로퍼티

    public Vector2 AimDirection => _aimDirection;
    public bool IsAimLocked => _isAimLocked;

    #endregion

    private void Awake()
    {
        if (_weaponPivot == null)
        {
            _weaponPivot = transform;
        }

        if (_camera == null)
        {
            _camera = Camera.main;
        }
    }

    private void Update()
    {
        UpdateAimDirection();

        if(_playerFacing != null)
        {
            _playerFacing.UpdateFacing(_aimDirection);
        }

        if (!_isAimLocked)
        {
            UpdateWeaponRotation();
        }
       
    }

    #region 마우스 방향 계산

    private void UpdateAimDirection()
    {
        if (_weaponPivot == null || _camera == null || Mouse.current == null)
        {
            return;
        }

        // 마우스 스크린 좌표
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // 카메라에서 무기까지의 Z 거리
        float distance = Mathf.Abs(_camera.transform.position.z - _weaponPivot.position.z);

        // 스크린 좌표 → 월드 좌표
        Vector3 mouseWorld = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));

        // 무기에서 마우스까지의 방향
        Vector2 direction = (Vector2)(mouseWorld - _weaponPivot.position);

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        // 무기 잠금 상관없이 갱신
        _aimDirection = direction.normalized;
    }
    #endregion

    #region 무기 회전
    private void UpdateWeaponRotation()
    {
        if(_weaponPivot == null)
        {
            return;
        }

        float angle = Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg;

        if(_aimDirection.y < 0f)
        {
            if (_aimDirection.x >= 0f)
            {
                angle = Mathf.Max(angle, -_maxDownAngle);
            }
            else
            {
                angle = Mathf.Min(angle, -180 + _maxDownAngle);
            }
        }

        _weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }
    #endregion

    #region 무기 잠금


    public void LockAim()
    {
        // 공격 시작 순간의 최신 방향 반영
        UpdateAimDirection();
        UpdateWeaponRotation();

        // 이후 무기 회전만 고정
        _isAimLocked = true;
    }

    public void UnlockAim()
    {
        _isAimLocked = false;
    }

    private void OnDisable()
    {
        _isAimLocked = false;
    }

    #endregion
}