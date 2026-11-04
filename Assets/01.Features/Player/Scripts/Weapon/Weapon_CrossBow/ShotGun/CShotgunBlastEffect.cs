using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CShotgunBlastEffect : MonoBehaviour
{
    #region 인스펙터

    [Header("Sprite Renderer")]
    [SerializeField] private SpriteRenderer _shootRenderer;
    [SerializeField] private SpriteRenderer _smokeRenderer;

    [Header("Shoot Frames")]
    [SerializeField] private Sprite[] _shootFrames;

    [Header("Smoke Frames")]
    [SerializeField] private Sprite[] _smokeFrames;

    [Header("Animation")]
    [Min(1f)]
    [SerializeField] private float _frameRate = 20f;

    [Min(0f)]
    [SerializeField] private float _smokeStartDelay = 0.1f;

    #endregion

    #region 내부변수

    private float _elapsed;
    private float _duration;
    private bool _isPlaying;

    #endregion

    #region Unity

    private void OnDisable()
    {
        _isPlaying = false;
        _elapsed = 0f;

        if (_shootRenderer != null)
        {
            _shootRenderer.enabled = false;
        }

        if (_smokeRenderer != null)
        {
            _smokeRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (!_isPlaying)
        {
            return;
        }

        _elapsed += Time.deltaTime;

        UpdateFrame(_shootRenderer, _shootFrames, _elapsed);

        UpdateFrame(_smokeRenderer, _smokeFrames, _elapsed - _smokeStartDelay);

        // 모든 프레임이 끝나면 풀로 반환
        if (_elapsed >= _duration)
        {
            _isPlaying = false;

            Destroy(gameObject);
        }
    }

    #endregion

    #region Animation

    public void Play(Vector2 direction)
    {
        _elapsed = 0f;
        _isPlaying = true;

        // 실제 발사 방향(+X)에 맞춰 -90도 보정
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);

        float shootDuration = _shootFrames.Length / _frameRate;

        float smokeDuration =_smokeStartDelay + _smokeFrames.Length / _frameRate;

        _duration = Mathf.Max(shootDuration, smokeDuration);

        UpdateFrame(_shootRenderer, _shootFrames, 0f);
        UpdateFrame(_smokeRenderer, _smokeFrames, -_smokeStartDelay);
    }

    private void UpdateFrame(SpriteRenderer renderer, Sprite[] frames, float time)
    {
        if (renderer == null || frames == null || frames.Length == 0 || time < 0f)
        {
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            return;
        }

        int index = Mathf.FloorToInt(time * _frameRate);

        if (index >= frames.Length)
        {
            renderer.enabled = false;
            return;
        }

        renderer.enabled = true;
        renderer.sprite = frames[index];
    }

    #endregion
}
