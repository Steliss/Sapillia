using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CTorchFragment : MonoBehaviour
{
    #region 인스펙터

    [Header("날아가는힘")]
    [SerializeField] private float _force = 3f;
    #endregion

    #region 내부변수
    private Rigidbody2D _rb;
    private SpriteRenderer _spriteRenderer;

    private float _timer;
    private bool _isLaunched = false;
    private bool _isStopped = false;

    #endregion

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void Update()
    {
        if( _isLaunched)
        {
            _timer += Time.deltaTime;

            if (_timer > 0.6f && !_isStopped)
            {
                _rb.velocity = Vector2.zero;
                _isStopped = true;
            }
            if(_timer > 0.6f && _timer < 1.5f)
            {
                Color color = _spriteRenderer.color;

                color.a = Mathf.Lerp(1f, 0f, (_timer - 0.6f) / 0.9f);

                _spriteRenderer.color = color;
            }
            if (_timer > 1.5f)
            {
                _isLaunched = false;
                gameObject.SetActive(false);
            }

        }
    }

    public void Launch(Vector2 direction)
    {
        Color color = _spriteRenderer.color;
        color.a = 1f;
        _spriteRenderer.color = color;

        _timer = 0f;
        _isLaunched = true;
        _isStopped = false;

        _rb.velocity = Vector2.zero;
        _rb.AddForce(direction.normalized * _force, ForceMode2D.Impulse);
        
    }




}
