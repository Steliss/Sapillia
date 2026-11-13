using Cinemachine;
using Unity.VisualScripting;
using UnityEngine;

public class CPlayerCamera : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private LayerMask _playerMask;
    [SerializeField] private float _shakeRecovery = 5f;
    #endregion

    #region 내부 변수
    private CinemachineBasicMultiChannelPerlin _noise;
    private CinemachineVirtualCamera _camera;
    #endregion

    #region 인스턴스
    public static CPlayerCamera Instance;
    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        _camera = GetComponent<CinemachineVirtualCamera>();
        if (_camera == null)
        {
            transform.AddComponent<CinemachineVirtualCamera>();
            _camera = GetComponent<CinemachineVirtualCamera>();
        }

        _noise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (_noise == null)
        {
            _camera.AddCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            _noise = _camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }
    }


    private void Start()
    {
        if (_camera.Follow == null)
        {
            Collider2D hit = Physics2D.OverlapBox
                (transform.position, new Vector2(100f, 100f), 0f, _playerMask);
            if (hit == null)
            {
                CLog.Warn("카메라가 플레이어를 찾지 못함! 비활성화");
                enabled = false;
                return;
            }
            _camera.Follow = hit.transform;
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    private void Update()
    {
        if (_noise.m_AmplitudeGain > 0)
        {
            _noise.m_AmplitudeGain = Mathf.MoveTowards
                (_noise.m_AmplitudeGain, 0f, Time.unscaledDeltaTime * _shakeRecovery);
        }
    }


    /// <summary>
    /// 플레이어 카메라 흔들기.
    /// </summary>
    /// <param name="power">흔드는 힘</param>
    public void PlayerCameraShaking(float power)
    {
        _noise.m_AmplitudeGain = power;
    }


}
