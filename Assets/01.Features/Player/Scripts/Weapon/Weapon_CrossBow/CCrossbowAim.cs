using UnityEngine;
using UnityEngine.InputSystem;

public class CCrossbowAim : MonoBehaviour
{
    #region 인스펙터

    [Header("조준 참조")]
    [SerializeField] private Transform _crossbowPivot;
    [SerializeField] private Camera _camera;

    #endregion

    #region 프로퍼티

    public Vector2 AimDirection { get; private set; } = Vector2.right;
    #endregion

    #region Unity

    private void LateUpdate()
    {
        UpdateAim();
    }

    #endregion


    #region Aim
    private void UpdateAim()
    {
        if (_crossbowPivot == null || _camera == null || Mouse.current == null)
        {
            return;
        }

        // 1. 마우스의 화면 좌표
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        // 2. Pivot이 위치한 평면까지의 카메라 거리
        float distance = Vector3.Dot(_crossbowPivot.position - _camera.transform.position, _camera.transform.forward);

        // 3. 마우스 좌표를 월드 좌표로 변환
        Vector3 mouseWorldPosition = _camera.ScreenToWorldPoint
            (
                new Vector3
                (
                    mousePosition.x,
                    mousePosition.y,
                    distance
                )
            );

        // 4. 석궁에서 마우스까지의 방향
        Vector2 direction = (Vector2)(mouseWorldPosition - _crossbowPivot.position);

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        AimDirection = direction.normalized;

        // 5. 회전 각도 계산
        float angle = Mathf.Atan2(AimDirection.y, AimDirection.x) * Mathf.Rad2Deg;

        // 6. 석궁 Pivot 회전
        _crossbowPivot.rotation = Quaternion.Euler(0f, 0f, angle);
    }

#endregion
}
