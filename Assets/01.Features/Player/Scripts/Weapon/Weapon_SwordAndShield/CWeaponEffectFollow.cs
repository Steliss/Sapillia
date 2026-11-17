using UnityEngine;

public class CWeaponEffectFollow : MonoBehaviour
{
    #region 내부변수
    private Transform _target;
    private CSwordAndShieldAim _aim;

    private Vector2 _offset;
    private float _angleOffset;

    private EWeaponEffectRotationMode _rotationMode;

    #endregion

    #region Initialize

    public void Initialize(Transform target, CSwordAndShieldAim aim, SWeaponEffectData effect)
    {
        _target = target;
        _aim = aim;

        _offset = effect.Offset;
        _angleOffset = effect.AngleOffset;
        _rotationMode = effect.RotationMode;

        UpdateTransform();
    }

    #endregion

    #region Update

    private void LateUpdate()
    {
        UpdateTransform();
    }

    private void UpdateTransform()
    {
        if (_target == null)
        {
            return;
        }

        // 검 끝의 이동을 따라감
        Vector3 position = _target.TransformPoint(_offset);

        Quaternion rotation = _target.rotation;

        // 위치는 검 끝, 회전은 마우스 조준 방향
        if (_rotationMode == EWeaponEffectRotationMode.MouseAim && _aim != null)
        {
            rotation = _aim.EffectAimRotation;
        }

        rotation *= Quaternion.Euler(0f, 0f, _angleOffset);
        transform.SetPositionAndRotation(position, rotation);
    }

    #endregion

    private void OnDisable()
    {
        _target = null;
        _aim = null;
    }
}
