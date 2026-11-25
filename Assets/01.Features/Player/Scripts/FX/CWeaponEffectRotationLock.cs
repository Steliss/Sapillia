using UnityEngine;

[DefaultExecutionOrder(1000)]
public class CWeaponEffectRotationLock : MonoBehaviour
{
    private Quaternion _lockedRotation;
    private bool _isLocked;

    // 생성 시점의 회전을 저장
    public void LockRotation(Quaternion rotation)
    {
        _lockedRotation = rotation;
        _isLocked = true;

        transform.rotation = _lockedRotation;
    }

    private void LateUpdate()
    {
        if (!_isLocked)
        {
            return;
        }

        // 부모의 회전과 관계없이 월드 회전 유지
        transform.rotation = _lockedRotation;
    }

    private void OnDisable()
    {
        _isLocked = false;
    }
}
