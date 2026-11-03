using UnityEngine;
using UnityEngine.InputSystem;

[DefaultExecutionOrder(-100)]
public class CCrossbowAim : MonoBehaviour
{
    #region 인스펙터

    [Header("조준 참조")]
    [SerializeField] private Transform _crossbowCenter;
    [SerializeField] private Camera _camera;

    [Header("플레이어 트래킹")]
    [SerializeField] private CPlayerFacing _playerFacing;
    #endregion

    #region 내부변수
    private Vector2 _aimDirection = Vector2.right;


    #endregion

    #region 프로퍼티

    public Vector2 AimDirection => _aimDirection;
    #endregion

    #region Unity
    private void Awake()
    {
        if(_crossbowCenter == null)
        {
            CLog.Warn("[CrossbowAim] Pivot 참조 누락");
        }

        // 카메라 연결 없을 시 기본 카메라 사용
        if(_camera == null)
        {
            _camera = Camera.main;
        }

        if(_camera == null)
        {
            CLog.Warn("[CrossbowAim] 카메라 누락");
        }
    }

    private void OnEnable()
    {
        CLog.Log("[CrossbowAim] 조준 활성화");
    }


    private void LateUpdate()
    {
        UpdateAim();
    }

    #endregion


    #region Aim
    private void UpdateAim()
    {
        if (_crossbowCenter == null || _camera == null || Mouse.current == null)
        {
            return;
        }

        // 마우스의 화면 좌표
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // Pivot이 위치한 평면까지의 카메라 거리
        float distance = Vector3.Dot(_crossbowCenter.position - _camera.transform.position, _camera.transform.forward);

        // 마우스 좌표를 월드 좌표로 변환
        Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint
            (
                new Vector3
                (
                    mousePosition.x,
                    mousePosition.y,
                    distance
                )
            );

        // 석궁에서 마우스까지의 방향
        Vector2 direction = (Vector2)(mouseWorldPosition - _crossbowCenter.position);

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        _aimDirection = direction.normalized;

        if (_playerFacing != null)
        {
            _playerFacing.UpdateFacing(AimDirection);
        }

        // 회전 각도 계산
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;

        // 석궁 Pivot 회전
        _crossbowCenter.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    public void RefreshAim()
    {
        UpdateAim();
    }

#endregion
}
