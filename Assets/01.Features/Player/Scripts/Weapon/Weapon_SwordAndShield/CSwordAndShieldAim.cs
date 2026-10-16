using UnityEngine.InputSystem;
using UnityEngine;

public class CSwordAndShieldAim : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private Transform _weaponPivot;
    [SerializeField] private Camera _camera;

    #endregion

    private void Awake()
    {
        if(_weaponPivot == null)
        {
            _weaponPivot = transform;
        }

        if(_camera == null)
        {
            _camera = Camera.main;
        }
    }

    private void LateUpdate()
    {
        
    }

    private void UpadataAim()
    {
        if(_weaponPivot == null  || _camera == null)
        {
            return;
        }

        Vector3 mousePosition = Mouse.current.position.ReadValue();

        mousePosition.z = Mathf.Abs(_camera.transform.position.z - _weaponPivot.position.z);
        Vector3 mouseWorld = _camera.ScreenToWorldPoint(mousePosition);
        Vector2 direction = mouseWorld - _weaponPivot.position;

        if(direction.sqrMagnitude < 0.001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
    }

}
