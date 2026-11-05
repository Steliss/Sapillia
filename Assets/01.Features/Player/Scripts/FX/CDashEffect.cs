using UnityEngine;

public class CDashEffect : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Min(0.01f)]
    [SerializeField] private float _frameInterval = 0.04f;
    #endregion

    #region 내부변수
    private Sprite[] _frames;
    private int _frameIndex;
    private float _timer;
    private bool _isPlaying;
    #endregion

    private void OnEnable()
    {
        _frameIndex = 0;
        _timer = 0f;
        _isPlaying = false;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = null;
        }
    }

    private void Update()
    {
        if (!_isPlaying || _frames == null || _frames.Length == 0)
        {
            return;
        }

        _timer += Time.deltaTime;

        if (_timer < _frameInterval)
        {
            return;
        }

        _timer -= _frameInterval;
        _frameIndex++;

        if (_frameIndex >= _frames.Length)
        {
            StopEffect();
            return;
        }

        _spriteRenderer.sprite = _frames[_frameIndex];
    }

    public void Play(Sprite[] frames, Vector3 worldPosition)
    {
        if (_spriteRenderer == null || frames == null || frames.Length == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        transform.position = worldPosition;

        _frames = frames;
        _frameIndex = 0;
        _timer = 0f;
        _isPlaying = true;

        _spriteRenderer.sprite = _frames[0];
    }

    private void StopEffect()
    {
        _isPlaying = false;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.sprite = null;
        }

        if (CObjectPoolManager.Instance != null)
        {
            gameObject.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }
}
