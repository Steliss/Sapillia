using UnityEngine;

public class CMeleeProjectile : MonoBehaviour
{
    #region 인스펙터
    [Header("충돌 데이터")]
    [SerializeField] private float _damage;
    [SerializeField] private Vector2 _velocity;
    [SerializeField] private LayerMask _targetLayer;

    [Header("애니메이터 스테이트")]
    [SerializeField] private string _stateIng = "Ing";
    [SerializeField] private string _stateEnd = "End";
    #endregion

    #region 내부 변수
    private Animator _ani;
    private SpriteRenderer _sp;
    private Rigidbody2D _rb;
    private int _hashIng;
    private int _hashEnd;
    #endregion

    private void Awake()
    {
        _ani = GetComponent<Animator>();
        _sp = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _hashIng = Animator.StringToHash(_stateIng);
        _hashEnd = Animator.StringToHash(_stateEnd);

        if (_ani == null || _sp == null || _rb == null)
        {
            CLog.Warn($"{name} : 애니메이터, 스프라이트, 리지드바디 누락 확인필요");
        }
    }

    private void OnEnable()
    {
        if (_ani != null)
        {
            _ani.Play(_hashIng);
        }
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _targetLayer) != 0)
        {
            if ( TryGetComponent<IDamageInfoReceiver>(out IDamageInfoReceiver player))
            {
                SDamageInfo info = new SDamageInfo();
                info.Damage = Mathf.RoundToInt(_damage);
                info.IncomingDirection = _velocity;
                info.HasDirection = true;

                player.TakeDamage(info);
                CLog.Log($"[EnemyAttack : {this}");
                CLog.Log($"[EnemyAttack Damage : {info.Damage}");
                CLog.Log($"[EnemyAttack Direction: {info.IncomingDirection}");
            }
        }
    }

    public void AniEnd()
    {
        if (_ani != null)
        {
            _ani.Play(_hashEnd);
        }
        if (_rb != null)
        {
            _rb.simulated = false;
        }
    }

    public void DamageSet(float damage, Vector2 velocity)
    {
        _damage = damage;
        _velocity = velocity;
        if (_sp != null)
        {
            if (velocity.x < 0)
            {
                _sp.flipX = true;
            }
            else
            {
                _sp.flipX = false;
            }
        }

        if (_rb != null)
        {
            _rb.simulated = true;
        }
    }

}
