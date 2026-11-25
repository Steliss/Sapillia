using Cinemachine;
using UnityEngine;

public class CSkyShow : CTriggerPlayer
{
    [SerializeField] private CinemachineVirtualCamera _skyCamera;

    [SerializeField] private int _active = 20;
    [SerializeField] private int _nomActive = 0;

    private void OnEnable()
    {
        SetCameraActive(false);
    }

    private void OnDisable()
    {
        SetCameraActive(false);
    }

    protected override void OnPlayerEnter()
    {
        SetCameraActive(true);
    }

    protected override void OnPlayerExit()
    {
        SetCameraActive(false);
    }

    private void SetCameraActive(bool active)
    {
        if (_skyCamera == null)
        {
            CLog.Warn("CSkyShow : 카메라 없음");
            return;
        }

        _skyCamera.Priority = active ? _active : _nomActive;
    }
}
