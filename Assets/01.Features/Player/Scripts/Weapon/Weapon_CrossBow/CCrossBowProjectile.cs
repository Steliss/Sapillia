using UnityEngine;

public class CCrossbowProjectile : MonoBehaviour
{
    #region 인스펙터
    [Header("참조")]
    [SerializeField] private Rigidbody2D _rb;

    [Header("충돌 레이어")]
    [SerializeField] private LayerMask _collisionLayers;

    #endregion

    #region 내부변수
    private int _damage;
    private Transform _ownerRoot;
    private bool _isLaunched;

    #endregion

    #region 초기화

    // 풀에서 꺼낸 뒤 매번 호출
    public void Launch
        (
            Vector2 position,
            Vector2 direction,
            float speed,
            int damage,
            Transform owner
        )
    {
        if (_rb == null)
        {
            CLog.Warn("[CrossbowProjectile] Rigidbody2D 누락");
            ReturnToPool();
            return;
        }

        _damage = Mathf.Max(0, damage);
        _ownerRoot = owner != null ? owner.root : null;

        Vector2 fireDirection = direction.normalized;

        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        // 풀에서 재사용하므로 상태 초기화
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        transform.rotation = Quaternion.Euler(0f, 0f, angle);

        _rb.position = position;
        _isLaunched = true;
        _rb.velocity = fireDirection * Mathf.Max(0f, speed);
    }

    #endregion

    #region Collision

    private void OnTriggerEnter2D(Collider2D other)
    {
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other) // 몬스터 관통하도록 수정해야함
    {
        if (!_isLaunched || other == null)
        {
            return;
        }

        // 지정한 충돌 대상만 검사
        int layer = 1 << other.gameObject.layer;

        if ((_collisionLayers.value & layer) == 0)
        {
            return;
        }

        // 발사한 플레이어 제외
        if (_ownerRoot != null && other.transform.root == _ownerRoot)
        {
            return;
        }

        // 충돌한 대상의 데미지 인터페이스 검색
        IDamageable target = other.GetComponentInParent<IDamageable>();

        if (target != null)
        {
            target.TakeDamage(_damage);
        }

        // 몬스터나 벽에 충돌하면 즉시 반환
        ReturnToPool();
    }

    #endregion

    #region Pool

    private void ReturnToPool()
    {
        if (!_isLaunched && !gameObject.activeSelf)
        {
            return;
        }

        _isLaunched = false;

        if (_rb != null)
        {
            _rb.velocity = Vector2.zero;
            _rb.angularVelocity = 0f;
        }

        if (CObjectPoolManager.Instance != null)
        {
            CObjectPoolManager.Instance.ReturnPool(gameObject);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void OnDisable()
    {
        _isLaunched = false;
        _damage = 0;
        _ownerRoot = null;
    }

    #endregion
}
