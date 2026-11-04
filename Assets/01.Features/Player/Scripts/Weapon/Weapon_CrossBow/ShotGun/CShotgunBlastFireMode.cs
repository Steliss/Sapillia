using System.Collections.Generic;
using UnityEngine;

public class CShotgunBlastFireMode : MonoBehaviour
{
    #region 인스펙터

    [Header("피격 대상")]
    [SerializeField] private LayerMask _targetLayers;

    [Header("장애물")]
    [SerializeField] private LayerMask _wallLayers;

    #endregion


    #region 내부변수

    // 피격 판정 탐색 변수
    private readonly Collider2D[] _hitColliders = new Collider2D[32];
    private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    #endregion

    #region Fire

    public bool Fire(CCrossbowDataSO data, Transform firePoint, int damage)
    {
        // 필수 참조
        if (data == null || firePoint == null || data.BlastEffectPrefab == null || CObjectPoolManager.Instance == null)
        {
            return false;
        }

        Vector2 origin = firePoint.position;
        Vector2 direction = firePoint.right;

        // 폭발 이펙트 생성
        GameObject effectObject = CObjectPoolManager.Instance.GetPoolObject(data.BlastEffectPrefab, firePoint);

        if (effectObject == null)
        {
            return false;
        }

        if (!effectObject.TryGetComponent(out CShotgunBlastEffect effect))
        {
            CLog.Warn("[Shotgun] 이펙트 컴포넌트 없음");

            Destroy(effectObject);

            return false;
        }

        // 이펙트 재생
        effect.Play(direction);

        // 공격 판정
        ApplyBlastDamage
            (
                origin,
                direction,
                data.BlastRange,
                data.BlastHalfAngle,
                damage
            );

        return true;
    }

    #endregion


    #region Damage

    private void ApplyBlastDamage(Vector2 origin, Vector2 direction, float range, float halfAngle, int damage)
    {
        _hitTargets.Clear();

        int count = Physics2D.OverlapCircleNonAlloc
            (
                origin,
                range,
                _hitColliders,
                _targetLayers
            );

        float minDot = Mathf.Cos(halfAngle * Mathf.Deg2Rad);

        for (int i = 0; i < count; i++)
        {
            Collider2D hit = _hitColliders[i];

            if (hit == null)
            {
                continue;
            }

            Vector2 targetPosition = hit.bounds.center;

            Vector2 toTarget = targetPosition - origin;

            if (toTarget.sqrMagnitude <= 0.0001f)
            {
                continue;
            }

            // 전방 각도 판정
            Vector2 targetDirection = toTarget.normalized;

            if (Vector2.Dot(direction, targetDirection)
                < minDot)
            {
                continue;
            }

            // 벽 뒤의 몬스터는 타격하지 않음
            if (Physics2D.Linecast(
                origin,
                targetPosition,
                _wallLayers))
            {
                continue;
            }

            // 몬스터의 자식 콜라이더도 처리
            IDamageable target =
                hit.GetComponentInParent<IDamageable>();

            if (target == null)
            {
                continue;
            }

            // 같은 몬스터 중복 타격 방지
            if (!_hitTargets.Add(target))
            {
                continue;
            }

            target.TakeDamage(damage);
        }

        _hitTargets.Clear();
    }

    #endregion
}
