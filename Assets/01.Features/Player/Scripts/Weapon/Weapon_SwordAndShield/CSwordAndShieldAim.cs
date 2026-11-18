using Unity.VisualScripting;
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
    [SerializeField] private SortingGroup _shieldSortingGroup;

    [Header("방향별 렌더링 순서")]
    [SerializeField] private int _upperSortingOffset = -1;
    [SerializeField] private int _lowerSortingOffset = 1;
    [SerializeField] private int _shieldSortingOffset = 2;

    [Header("무기 대기 흔들림")]
    [SerializeField] private CPlayerMovement _movement;
    [SerializeField] private Vector2 _idleSwayAxis = Vector2.right;
    [Min(0f)]
    [SerializeField] private float _idleSwayDistance = 0.03f;
    [Min(0f)]
    [SerializeField] private float _idleSwayFrequency = 0.8f;
    [Min(0f)]
    [SerializeField] private float _idleBlendSpeed = 4f;

    [Header("방패 대기 흔들림")]
    [Min(0f)]
    [SerializeField] private float _shieldSwayDistance = 0.015f;
    [Min(0f)]
    [SerializeField] private float _shieldSwayFrequency = 0.8f;
    [SerializeField] private float _shieldSwayPhase = 0f;

    [Header("방패 Transform")]
    [SerializeField] private Transform _shieldPivot;

    [Header("방향별 방패 위치 보정")]
    [SerializeField] private Vector2 _shieldRightUpOffset;
    [SerializeField] private Vector2 _shieldLeftUpOffset;
    [SerializeField] private Vector2 _shieldLeftDownOffset;
    [SerializeField] private Vector2 _shieldRightDownOffset;
    #endregion

    #region 변수
    private Vector2 _aimDirection = Vector2.right;
    private Vector3 _initialPivotLocalPosition;

    private float _idleSwayTime;
    private float _idleSwayWeight = 1f;

    private float _currentAimAngle;
    private float _currentWeaponAngle;

    private Vector3 _initialShieldLocalPosition;

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

        if (_shieldPivot != null)
        {
            _initialShieldLocalPosition = _shieldPivot.localPosition;
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
     

        UpdateAimDirection();
        // 대기 움직임 계산
        UpdateIdleSway();
        // 무기 위치, 조준각, 렌더링 갱신
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
                
        // 현재 조준 회전 적용
        _weaponPivot.rotation = Quaternion.Euler(0f, 0f, weaponAngle);

        // 검의 축 방향으로 흔들림 거리 계산
        float sway = Mathf.Sin(_idleSwayTime * Mathf.PI * 2f * _idleSwayFrequency) * _idleSwayDistance * _idleSwayWeight;

        // 검 로컬 방향을 월드 방향으로 변환
        Vector3 swayWorld = _weaponPivot.rotation * (Vector3)(_idleSwayAxis.normalized * sway);

        // WeaponPivot 부모의 로컬 좌표로 변환
        Vector3 swayLocal = _weaponPivot.parent != null ? _weaponPivot.parent.InverseTransformVector(swayWorld) : swayWorld;

        // 방향별 손 위치 + 대기 흔들림
        _weaponPivot.localPosition = _initialPivotLocalPosition + new Vector3(offset.x, offset.y, 0f) + swayLocal;
        _currentWeaponAngle = weaponAngle;

        // 플레이어 방향
        if (_playerFacing != null)
        {
            _playerFacing.UpdateFacing(_aimDirection);
        }

        // 방패는 위치만 변경
        UpdateShieldPlacement(angle);
        // 무기 앞뒤 관계
        UpdateWeaponSorting(angle);
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
        _isAimLocked = true;

        ApplyAimPose();
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

    private void UpdateWeaponSorting(float angle)
    {
        if (_playerRenderer == null)
        {
            return;
        }

        int playerOrder = _playerRenderer.sortingOrder;
        int layerID = _playerRenderer.sortingLayerID;

        // 검: 위쪽/아래쪽 조준에 따라 앞뒤 변경
        if (_weaponSortingGroup != null)
        {
            bool isUpper = angle < 180f;

            _weaponSortingGroup.sortingLayerID = layerID;
            _weaponSortingGroup.sortingOrder = playerOrder + (isUpper ? _upperSortingOffset : _lowerSortingOffset);
        }

        // 방패: 항상 플레이어 앞
        if (_shieldSortingGroup != null)
        {
            _shieldSortingGroup.sortingLayerID = layerID;
            _shieldSortingGroup.sortingOrder = playerOrder + _shieldSortingOffset;
        }
    }
    #endregion

    #region 대기 움직임 계산
    private void UpdateIdleSway()
    {
        // Movement가 없으면 이동 여부와 무관하게 흔들림
        bool isIdle = _movement == null || _movement.MoveState == EPlayerMoveState.Idle;

        float targetWeight = isIdle ? 1f : 0f;

        // 이동 시작/정지 시 자연스럽게 전환
        _idleSwayWeight = Mathf.MoveTowards(_idleSwayWeight, targetWeight, _idleBlendSpeed * Time.deltaTime);
        _idleSwayTime += Time.deltaTime;
    }

    #endregion

    #region 방패 위치 변경
    private void UpdateShieldPlacement(float angle)
    {
        if (_shieldPivot == null)
        {
            return;
        }

        Vector2 offset;

        // 오른쪽 위
        if (angle < 90f)
        {
            offset = _shieldRightUpOffset;
        }
        // 왼쪽 위
        else if (angle < 180f)
        {
            offset = _shieldLeftUpOffset;
        }
        // 왼쪽 아래
        else if (angle < 270f)
        {
            offset = _shieldLeftDownOffset;
        }
        // 오른쪽 아래
        else
        {
            offset = _shieldRightDownOffset;
        }

        // 대기 상태에서만 흔들림 적용
        float sway = 0f;

        if (!_isAimLocked)
        {
            sway = Mathf.Sin(_idleSwayTime * Mathf.PI * 2f * _shieldSwayFrequency + _shieldSwayPhase) * _shieldSwayDistance * _idleSwayWeight;
        }

        // 초기 위치 + 방향별 보정
        _shieldPivot.localPosition = _initialShieldLocalPosition + new Vector3(offset.x, offset.y + sway, 0f);
    }

    #endregion
}