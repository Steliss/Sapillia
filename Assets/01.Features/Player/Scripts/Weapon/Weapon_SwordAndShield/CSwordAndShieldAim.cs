using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

[DefaultExecutionOrder(-100)]
public class CSwordAndShieldAim : MonoBehaviour
{
    #region 인스펙터

    [Header("Weapon")]
    [SerializeField] private Transform _weaponPivot;
    [SerializeField] private Sprite _swordSprite;

    [Header("Player Transform")]
    [SerializeField] private Transform _player;

    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("CPlayerFacing")]
    [SerializeField] private CPlayerFacing _playerFacing;

    [Header("Aim Limit")]
    [Range(-359f, 359f)]
    [SerializeField] private float _maxDownAngle = 45f;

    [Header("방향별 무기 위치 보정")]
    [SerializeField] private Vector2 _rightUpOffset = new Vector2(0.15f, 0.1f);
    [SerializeField] private Vector2 _leftUpOffset = new Vector2(-0.15f, 0.1f);
    [SerializeField] private Vector2 _leftDownOffset = new Vector2(-0.15f, -0.1f);
    [SerializeField] private Vector2 _rightDownOffset = new Vector2(0.15f, -0.1f);

    [Header("랜더링 순서조정")]
    [SerializeField] private SpriteRenderer _playerRenderer;
    [SerializeField] private SortingGroup _weaponSortingGroup;

    [Header("방향별 렌더링 순서")]
    [SerializeField] private int _upperSortingOffset = -1;
    [SerializeField] private int _lowerSortingOffset = 1;
    #endregion

    #region 변수
    private Vector2 _aimDirection = Vector2.right;
    private Vector3 _initialPivotLocalPosition;

    private float _currentAimAngle;
    private float _currentWeaponAngle;

    // 무기 회전 잠금(전투)
    private bool _isAimLocked;

    #endregion

    #region 프로퍼티

    public Vector2 AimDirection => _aimDirection;
    public bool IsAimLocked => _isAimLocked;
    public float CurrentAimAngle => _currentAimAngle;
    public float CurrentWeaponAngle => _currentWeaponAngle;
    // 이펙트에 전달할 최종 무기 조준 회전
    public Quaternion EffectAimRotation => _weaponPivot != null ? _weaponPivot.rotation : Quaternion.identity;

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

        _initialPivotLocalPosition = _weaponPivot.localPosition;
    }

    private void Update()
    {
        // 공격중이면 자세 유지
        if (_isAimLocked)
        {
            return;
        }
        /*
        UpdateWeaponPlacement();
        UpdateWeaponRotation();
        UpdateWeaponSorting();
        */

        UpdateAimDirection();
        ApplyAimPose();

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

        // 각도 회전
        Vector3 aimOrigin = _player != null ? _player.position : _weaponPivot.position;
        
        // 카메라에서 무기까지의 Z 거리
        float distance = Mathf.Abs(_camera.transform.position.z - aimOrigin.z);

        // 스크린 좌표 → 월드 좌표
        Vector3 mouseWorld = _camera.ScreenToWorldPoint(new Vector3(mousePosition.x, mousePosition.y, distance));

        Vector2 direction = (Vector2)(mouseWorld - aimOrigin);

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }
        // 무기 잠금 상관없이 갱신
        _aimDirection = direction.normalized;

        // 0 ~ 360도
        _currentAimAngle = Mathf.Repeat(Mathf.Atan2(_aimDirection.y, _aimDirection.x) * Mathf.Rad2Deg, 360f);
    }
    #endregion

    #region 무기 자세

    private void ApplyAimPose()
    {
        if (_weaponPivot == null)
        {
            return;
        }

        float angle = _currentAimAngle;

        Vector2 offset;
        float weaponAngle;

        // 오른쪽 위 : 0 ~ 90도
        if (angle < 90f)
        {
            offset = _rightUpOffset;
            weaponAngle = angle;
        }
        // 왼쪽 위 : 90 ~ 180도
        else if (angle < 180f)
        {
            offset = _leftUpOffset;
            weaponAngle = angle-180f;
        }
        // 왼쪽 아래 : 180 ~ 270도
        else if (angle < 270f)
        {
            offset = _leftDownOffset;

            // 왼쪽 아래로 지나치게 회전하지 않음
            weaponAngle = angle+_maxDownAngle-270f;
        }
        // 오른쪽 아래 : 270 ~ 360도
        else
        {
            offset = _rightDownOffset;

            // 오른쪽 아래로 지나치게 회전하지 않음
            weaponAngle = Mathf.Max(angle, 180 + _maxDownAngle);
        }

        // 플레이어 기준 손 위치
        _weaponPivot.localPosition = _initialPivotLocalPosition + new Vector3(offset.x, offset.y, 0f);

        // 조준 방향 회전
        _weaponPivot.rotation = Quaternion.Euler(0f, 0f, weaponAngle);

        _currentWeaponAngle = weaponAngle;

        // 플레이어 방향
        if (_playerFacing != null)
        {
            _playerFacing.UpdateFacing(_aimDirection);
        }

        // 무기 앞뒤 관계
        UpdateWeaponSorting(angle);
    }

    #endregion

    #region 렌더링

    private void UpdateWeaponSorting(float angle)
    {
        if (_playerRenderer == null || _weaponSortingGroup == null)
        {
            return;
        }

        bool isUpper = angle < 180f;

        _weaponSortingGroup.sortingLayerID = _playerRenderer.sortingLayerID;
        _weaponSortingGroup.sortingOrder = _playerRenderer.sortingOrder + (isUpper ? _upperSortingOffset : _lowerSortingOffset);
    }

    #endregion


    #region 무기 잠금

    public void LockAim()
    {

        if (_isAimLocked)
        {
            return;
        }

        // 공격 시작 순간의 최신 방향 반영
        UpdateAimDirection();
        ApplyAimPose();

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

    #region 렌더링 조정

    private void UpdateWeaponSorting()
    {
        if (_playerRenderer == null || _weaponSortingGroup == null)
        {
            return;
        }

        // 위쪽을 바라보면 무기를 플레이어 뒤로
        bool isBehind = _aimDirection.x < 0f;

        _weaponSortingGroup.sortingLayerID = _playerRenderer.sortingLayerID;
        _weaponSortingGroup.sortingOrder = _playerRenderer.sortingOrder + (isBehind ? 1 : -1);
    }
    #endregion
}