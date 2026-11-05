
using UnityEngine;

public class CDashAfterimageEffect : MonoBehaviour
{
    #region 인스펙터

    [Header("잔상 Sprite")]
    [SerializeField] private SpriteRenderer _spriteRenderer;

    [Header("잔상 설정")]
    [Min(0.01f)]
    [SerializeField] private float _fadeDuration = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float _startAlpha = 0.5f;

    [SerializeField]
    private Color _tintColor = new Color(0.6f, 0.85f, 1f, 1f);

    [Min(0)]
    [SerializeField] private int _sortingOrderOffset = 1;

    #endregion

    #region 내부변수

    private float _elapsed;
    private bool _isPlaying;
    private Color _startColor;

    #endregion

    #region Unity

    private void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        _elapsed += Time.deltaTime;

        float progress = Mathf.Clamp01(_elapsed / _fadeDuration);

        // 시간이 지날수록 투명하게
        Color color = _startColor;
        color.a = _startColor.a * (1f - progress);

        _spriteRenderer.color = color;

        if (progress >= 1f)
        {
            StopEffect();
        }
    }

    private void OnDisable()
    {
        _isPlaying = false;
        _elapsed = 0f;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
            _spriteRenderer.sprite = null;
        }
    }

    #endregion

    #region Effect

    public void Play(SpriteRenderer source)
    {
        if (source == null || source.sprite == null || _spriteRenderer == null)
        {
            StopEffect();
            return;
        }

        // 현재 플레이어의 위치와 회전 복사
        transform.SetPositionAndRotation(source.transform.position, source.transform.rotation);

        // 플레이어 스프라이트의 크기 복사
        transform.localScale = source.transform.lossyScale;

        // 현재 애니메이션 프레임 복사
        _spriteRenderer.sprite = source.sprite;

        // 좌우 반전 상태 복사
        _spriteRenderer.flipX = source.flipX;
        _spriteRenderer.flipY = source.flipY;

        // 플레이어보다 뒤쪽에 출력
        _spriteRenderer.sortingLayerID = source.sortingLayerID;
        _spriteRenderer.sortingOrder = source.sortingOrder - _sortingOrderOffset;

        // 잔상 색상 적용
        Color sourceColor = source.color;

        // 원본 색상과 관계없이 흰색 잔상
        _startColor = new Color
            (
                1f,
                1f,
                1f,
                source.color.a * _startAlpha
            );

        _spriteRenderer.color = _startColor;
        _spriteRenderer.enabled = true;

        _elapsed = 0f;
        _isPlaying = true;
    }

    private void StopEffect()
    {
        _isPlaying = false;

        if (_spriteRenderer != null)
        {
            _spriteRenderer.enabled = false;
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

    #endregion
}
