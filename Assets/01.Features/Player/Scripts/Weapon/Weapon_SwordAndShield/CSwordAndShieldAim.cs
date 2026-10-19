using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class CSwordAndShieldAim : MonoBehaviour
{
    #region 인스펙터

    [Header("Weapon")]
    [SerializeField] private Transform _weaponPivot;

    [Header("Camera")]
    [SerializeField] private Camera _camera;

    [Header("Aim Limit")]
    [Range(0f, 89f)]
    [SerializeField] private float _maxDownAngle = 45f;
    #endregion

    #region 변수

    private bool _facingLeft;

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
        UpdateAim();
    }

    #region Aim

    private void UpdateAim()
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
        Vector2 direction = mouseWorld - _weaponPivot.position;

        if (direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        // 방향 → 회전각
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 마지막으로 바라본 좌우 방향 기억
        if (Mathf.Abs(direction.x) > 0.01f)
        {
            _facingLeft = direction.x < 0f;
        }

        // 아래쪽 회전각 제한
        if (direction.y < 0f)
        {
            if (_facingLeft)
            {
                angle = Mathf.Min(angle, -180f + _maxDownAngle);
            }
            else
            {
                angle = Mathf.Max(angle, -_maxDownAngle);
            }
        }

        // 실제 무기 회전 적용
        _weaponPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    #endregion
}