using UnityEngine;

public class CEffectSoundEnablePlay : MonoBehaviour
{
    private CSoundManager _soundManager;
    [SerializeField] private string _playSound;

    private void OnEnable()
    {
        if (_soundManager == null)
        {
            _soundManager = CManagerDontDestroy.Instance.Sound;
            if (_soundManager == null)
            {
                CLog.Warn($"{name} 사운드 매니저 없음. 재생취소.");
                return;
            }
        }

        if (string.IsNullOrWhiteSpace(_playSound))
        {
            return;
        }

        _soundManager.PlayEffect(_playSound, transform.position);

    }

}
