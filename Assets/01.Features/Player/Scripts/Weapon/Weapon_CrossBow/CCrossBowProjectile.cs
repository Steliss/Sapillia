using System.Collections.Generic;
using UnityEngine;

public class CCrossbowProjectile : MonoBehaviour
{
    #region 인스펙터
    [Header("참조")]
    [SerializeField] private Rigidbody2D _rb;

    [Header("충돌 레이어")]
    [SerializeField] private LayerMask _collisionLayers;

    [Header("벽 확인 레이어")]
    [SerializeField] private LayerMask _wallLayers;

    #endregion

    #region 내부변수
    private int _damage;
    private Transform _ownerRoot;
    private bool _isLaunched;

    // 최대 사거리
    private float _maxRange;
    // 발사 후 실제 이동한 거리
    private float _travelledDistance;
    // 직전 물리 프레임 위치
    private Vector2 _lastPosition;

    private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();
    #endregion

    #region 초기화

    // 풀에서 꺼낸 뒤 매번 호출
    public void Launch
        (
            Vector2 direction,
            float speed,
            float maxRange,
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

        if (direction.sqrMagnitude < 0.0001f || speed <= 0f)
        {
            CLog.Warn("[CrossbowProjectile] 발사 방향 또는 속도 오류");
            ReturnToPool();
            return;
        }

        // 1. 발사 정보 초기화
        _damage = Mathf.Max(0, damage);
        _ownerRoot = owner != null ? owner.root : null;

        _maxRange = Mathf.Max(0.01f, maxRange);
        _travelledDistance = 0f;

        _hitTargets.Clear();

        // 2. 현재 위치를 거리 계산 시작점으로 설정
        _lastPosition = _rb.position;

        // 3. 발사 방향 계산
        Vector2 fireDirection = direction.normalized;

        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        // 4. 물리 상태 초기화
        _rb.velocity = Vector2.zero;
        _rb.angularVelocity = 0f;

        // 2D에서는 Z축을 기준으로 회전
        _rb.rotation = angle;

        // 5. 발사 시작
        _isLaunched = true;
        _rb.velocity = fireDirection * speed;
    }

    #endregion

    #region Collision

    private void OnTriggerEnter2D(Collider2D other)
    {
        CLog.Log($"[Projectile] Trigger 발생 : {other.gameObject.name} / " + $"Launched : {_isLaunched}");
        HandleHit(other);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        CLog.Log($"[Projectile] Collision 발생 : " + $"{collision.gameObject.name} / " + $"Launched : {_isLaunched}");
        HandleHit(collision.collider);
    }

    private void HandleHit(Collider2D other) // 몬스터 관통하도록 수정해야함
    {
        if (!_isLaunched || other == null)
        {
            CLog.Warn("닿은 대상 없음");
            return;
        }

        // 발사한 플레이어 제외
        if (_ownerRoot != null && other.transform.root == _ownerRoot)
        {
            CLog.Warn($"[CCrossbowProjectile] 플레이어 {other.gameObject.name}에 맞음");
            return;
        }

        // 충돌 허용 레이어 검사
        int layer = 1 << other.gameObject.layer;

        if((_collisionLayers.value & layer) == 0)
        {
            return;
        }

        // 벽일 경우 → 즉시 반환
        if((_wallLayers.value & layer) != 0)
        {
            CLog.Log("[Projectile] 벽 충돌");

            ReturnToPool();
            return;
        }

        // 충돌한 대상의 데미지 인터페이스 검색
        IDamageable target = other.GetComponentInParent<IDamageable>();

        if(target == null)
        {
            return;
        }        
               
        CLog.Log($"{other.gameObject.name}에게 {_damage}를 줌");
        target.TakeDamage(_damage);
                
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

        _maxRange = 0f;
        _travelledDistance = 0f;
        _lastPosition = Vector2.zero;

        _hitTargets.Clear();
    }

    #endregion

    #region 최대 사거리 도달 시 제거
    private void FixedUpdate()
    {
        if(!_isLaunched || _rb == null)
        {
            return;
        }

        // 현재 위치
        Vector2 currentPosition = _rb.position;
        // 이동 거리
        float moveDistance = Vector2.Distance(_lastPosition, currentPosition);

        _travelledDistance += moveDistance;
        // 위치 갱신
        _lastPosition = currentPosition;

        if(_travelledDistance >= _maxRange)
        {
            ReturnToPool();
        }
    }

    #endregion
}
