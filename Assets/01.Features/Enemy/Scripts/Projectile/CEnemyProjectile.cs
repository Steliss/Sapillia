using UnityEngine;

public class CEnemyProjectile : MonoBehaviour
{
    #region 인스펙터
    [Header("충돌 데이터")]
    [SerializeField] protected float _damage;
    [SerializeField] protected Vector2 _dir;
    [SerializeField] protected LayerMask _targetLayer;

    [Header("애니메이터 스테이트")]
    [SerializeField] protected string _stateIng = "Ing";
    [SerializeField] protected string _stateEnd = "End";
    #endregion

    #region 내부 변수
    protected Animator _ani;
    protected SpriteRenderer _sp;
    protected Rigidbody2D _rb;
    protected int _hashIng;
    protected int _hashEnd;
    protected float _attackTime = 0f;
    #endregion

    private void Awake()
    {
        _ani = GetComponentInChildren<Animator>();
        _sp = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();
        _hashIng = Animator.StringToHash(_stateIng);
        _hashEnd = Animator.StringToHash(_stateEnd);

        if (_ani == null || _sp == null || _rb == null)
        {
            CLog.Warn($"{name} : 애니메이터, 스프라이트, 리지드바디 누락 확인필요");
            enabled = false;
        }
    }


    // 시간이 지나면 충돌off, 애니메이션 재생
    protected virtual void Update()
    {
        if (!_rb.simulated)
        {
            return;
        }
        _attackTime -= Time.deltaTime;

        if (_attackTime > 0f)
        {
            return;
        }

        _rb.simulated = false;
        _rb.velocity = Vector2.zero;
        _ani.Play(_hashEnd);
    }

    public virtual void DamageSet(float damage, Vector2 dir, float attackTime)
    {
        _damage = damage;
        _dir = dir;
        _ani.Play(_hashIng);
        _attackTime = attackTime;
        if (_sp != null)
        {
            if (dir.x < 0)
            {
                _sp.flipX = true;
            }
            else
            {
                _sp.flipX = false;
            }
        }

        _rb.simulated = true;
    }

    protected virtual void OnTriggerEnter2D(Collider2D collision)
    {
        if (((1 << collision.gameObject.layer) & _targetLayer) == 0)
        {
            return;
        }

        IDamageInfoReceiver player = collision.GetComponentInChildren<IDamageInfoReceiver>();

        if (player == null)
        {
            CLog.Log("[MeleeProjectile] IDamageInfoReceiver를 찾지 못함");
            return;
        }

        SDamageInfo info = new SDamageInfo(Mathf.RoundToInt(_damage), _dir);

        player.TakeDamage(info);

        CLog.Log($"[EnemyAttack 적중 대상 : {collision.name}");
        CLog.Log($"[EnemyAttack Damage : {info.Damage}");
        CLog.Log($"[EnemyAttack Direction: {info.IncomingDirection}");

        HitEnd();
    }


    // 데미지를 준 후 처리가 필요하면 상속받은 후 HitEnd 함수를 오버라이드
    protected virtual void HitEnd()
    {

    }
}
