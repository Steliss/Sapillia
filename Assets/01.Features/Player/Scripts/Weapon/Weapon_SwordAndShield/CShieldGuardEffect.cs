using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CShieldGuardEffect : MonoBehaviour
{
    #region 인스펙터
    [Header("참조")]
    [SerializeField] private CPlayerGuard _playerGuard;
    [SerializeField] private CSwordAndShieldAim _aim;
    [SerializeField] private LineRenderer _arcRenderer;

    [Header("LineRenderer Settings")]
    [Min(0.1f)]
    [SerializeField] private float _radius = 0.85f;
    [Range(6, 64)]
    [SerializeField] private int _segments = 24;
    [Min(0.001f)]
    [SerializeField] private float _lineWidth = 0.035f;
    [SerializeField] private float _forwardOffset = 0f;

    [Header("Effect")]
    [Min(0f)]
    [SerializeField] private float _raiseDuration = 0.08f;
    [Range(0.1f, 1f)]
    [SerializeField] private float _startScale = 0.85f;

    #endregion

    #region 내부변수
    private bool _wasGuarding;
    private float _raiseTimer;

    #endregion

    #region 유니티 엔진
    private void Awake()
    {
        if (_arcRenderer == null)
        {
            enabled = false;
            return;
        }

        CreateArc();

        _arcRenderer.enabled = false;
    }

    private void LateUpdate()
    {
        if (_playerGuard == null || _aim == null || _arcRenderer == null)
        {
            return;
        }

        bool isGuarding = _playerGuard.isActiveAndEnabled && _playerGuard.IsGuarding;
        CLog.Log($"IsGuarding : {_playerGuard.IsGuarding} / isGuarding : {isGuarding}");
        // 방패를 내리거나 GuardBreak 발생
        if (!isGuarding)
        {
            HideEffect();
            return;
        }

        // 방패를 처음 올린 순간
        if (!_wasGuarding)
        {
            _wasGuarding = true;
            _raiseTimer = 0f;

            _arcRenderer.enabled = true;
        }

        UpdateDirection();
        UpdateRaiseEffect();
    }

    private void OnDisable()
    {
        if (_arcRenderer != null)
        {
            _arcRenderer.enabled = false;
        }

        _wasGuarding = false;
        _raiseTimer = 0f;

        transform.localScale = Vector3.one;
    }


    #endregion

    #region Arc
    private void CreateArc()
    {
        _arcRenderer.useWorldSpace = false;
        _arcRenderer.loop = false;

        _arcRenderer.positionCount = _segments + 1;
        _arcRenderer.widthMultiplier = _lineWidth;

        _arcRenderer.numCapVertices = 0;
        _arcRenderer.numCornerVertices = 0;

        // 연노란색 + 흰색
        Gradient gradient = new Gradient();

        gradient.SetKeys
            (
                new GradientColorKey[]
            {
                new GradientColorKey(new Color(1f, 0.88f, 0.45f), 0f),
                new GradientColorKey(Color.white, 0.25f),
                new GradientColorKey(Color.white, 0.75f),
                new GradientColorKey(new Color(1f, 0.88f, 0.45f), 1f)
            },

            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 1f)
            }
            );

        _arcRenderer.colorGradient = gradient;

        // 마우스가 오른쪽을 향할 때
        // -90도부터 +90도까지 반원 생성
        for (int i = 0; i <= _segments; i++)
        {
            float t = (float)i / _segments;

            float angle = Mathf.Lerp(-90f, 90f, t) * Mathf.Deg2Rad;

            float x = Mathf.Cos(angle) * _radius + _forwardOffset;
            float y = Mathf.Sin(angle) * _radius;

            _arcRenderer.SetPosition(i, new Vector3(x, y, 0f));
        }
    }

    #endregion

    #region 방향

    private void UpdateDirection()
    {
        Vector2 direction = _aim.AimDirection;

        if (direction.sqrMagnitude < 0.0001f)
        {
            return;
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    #endregion

    #region 이펙트

    private void UpdateRaiseEffect()
    {
        _raiseTimer += Time.deltaTime;

        float progress = _raiseDuration > 0f ? Mathf.Clamp01(_raiseTimer / _raiseDuration) : 1f;
        progress = Mathf.SmoothStep(0f, 1f, progress);

        float scale = Mathf.Lerp(_startScale, 1f, progress);
        transform.localScale = Vector3.one * scale;
    }

    private void HideEffect()
    {
        _wasGuarding = false;
        _raiseTimer = 0f;

        _arcRenderer.enabled = false;

        transform.localScale = Vector3.one;
    }
    #endregion


}
