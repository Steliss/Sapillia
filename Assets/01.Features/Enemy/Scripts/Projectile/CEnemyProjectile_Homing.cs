using UnityEngine;

public class CEnemyProjectile_Homing : CEnemyProjectile_Bullet
{
    #region 인스펙터
    [SerializeField] private string _soundEnd;
    [SerializeField] private FMODUnity.StudioEventEmitter _emitter;
    #endregion

    #region 내부 변수
    private Transform _targetTr;
    private float _speed;
    private float _turnSpeed;
    private CSoundManager _soundManager;
    private bool _isSoundOnes;
    #endregion

    private void OnEnable()
    {
        if (_emitter != null)
        {
            _emitter.Play();
        }
    }

    protected override void EndSoundPlay()
    {
        if (_emitter != null)
        {
            if (_emitter.IsPlaying())
            {
                if (_soundManager == null)
                {
                    if (CManagerDontDestroy.Instance != null)
                    {
                        _soundManager = CManagerDontDestroy.Instance.Sound;
                    }
                }
                if (!string.IsNullOrWhiteSpace(_soundEnd))
                {
                    _soundManager?.PlayEffect(_soundEnd, transform.position);
                }

                _emitter.Stop();

            }
        }
    }
    


    public void HomingTargetSet(Transform targetTr, Vector2 velosity, float speed, float turnSpeed)
    {
        _targetTr = targetTr;
        _speed = speed;
        _turnSpeed = turnSpeed;
        _rb.velocity = velosity.normalized * _speed;
    }


    private void FixedUpdate()
    {
        float currentAngle = DirToAngle(_rb.velocity);
        Vector2 targetPos = _targetTr.position;
        float targetAngle = DirToAngle((targetPos - _rb.position).normalized);

        float nextAngle = Mathf.MoveTowardsAngle
            (currentAngle, targetAngle, Time.fixedDeltaTime * _turnSpeed);

        _dir = AngleToDir(nextAngle).normalized;
        _rb.velocity = _dir * _speed;
    }

    private float DirToAngle(Vector2 dir)
    {
        return Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
    }

    private Vector2 AngleToDir(float angle)
    {
        return Quaternion.Euler(0, 0, angle) * Vector2.up;
    }
}
