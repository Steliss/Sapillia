using UnityEngine;

public class CEnterBox : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _sprite;
    [SerializeField, Min(1f)] private float _duration = 6f;

    private Vector3 _originalScale;
    private Color _originalColor;
    private float _elapsed = 0f;

    private void Awake()
    {
        _originalScale = _sprite.transform.localScale;
        _originalColor = _sprite.color;
    }

    private void Update()
    {
        _elapsed += Time.deltaTime;

        float progress = (1f - Mathf.Cos(_elapsed / _duration * Mathf.PI * 2f)) * 0.5f;

        float scale = Mathf.Lerp(1f, 0.8f, progress);
        _sprite.transform.localScale = _originalScale * scale;

        Color color = _originalColor;
        color.a = Mathf.Lerp(_originalColor.a, 0.5f, progress);
        _sprite.color = color;
    }

    private void OnDisable()
    {
        _sprite.transform.localScale = _originalScale;
        _sprite.color = _originalColor;
    }
}