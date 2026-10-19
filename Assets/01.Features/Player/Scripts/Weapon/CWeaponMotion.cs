using System.Collections.Generic;
using UnityEngine;

public class CWeaponMotion : MonoBehaviour
{
    #region 인스펙터

    [Header("무기")]
    [SerializeField] private Transform _weaponSwing;
    [SerializeField] private Transform _hitPoint;

    [Header("공격 이펙트")]
    [SerializeField] private SpriteRenderer _slashRenderer;

    [Header("공격될 레이어")]
    [SerializeField] private LayerMask _targetLayers;

    [Header("공격 데이터")]
    [SerializeField] private CWeaponAttackDataSO _attackData;


    // ※ 테스트 용도
    [Header("테스트 용도")]
    [SerializeField] CWeaponAttackDataSO _previewAttack;
    #endregion

    #region Variables
    private float _elapsedTime;
    private float _duration;
    private float _nextAttackTime;

    private int _damage;
    private bool _isAttacking;


    private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    private readonly Collider2D[] _overlapResults = new Collider2D[32];
    private readonly RaycastHit2D[] _castResults = new RaycastHit2D[32];
    #endregion

    #region 프로퍼티

    public bool IsAttacking => _isAttacking;
    public bool CanStart => !_isAttacking && Time.time >= _nextAttackTime;
    public float Progress => _duration > 0f ? Mathf.Clamp01(_elapsedTime / _duration) : 1f;
    #endregion

    private void Awake()
    {
        if (_weaponSwing == null || _hitPoint == null)
        {
            CLog.Error("Weapon 참조 누락");
            enabled = false;
            return;
        }

        if (_slashRenderer != null)
        {
            _slashRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (!_isAttacking)
        {
            return;
        }

        UpdateAttack(Time.deltaTime);
    }

    #region Attack

    public bool TryPlay(CWeaponAttackDataSO attack, int baseDamage, SPlayerStats stats)
    {
        if (!isActiveAndEnabled || !CanStart || attack == null)
        {
            return false;
        }

        _attackData = attack;

        float speed = Mathf.Max(0.01f, stats.Attack_Speed);

        _duration = attack.Duration / speed;
        _elapsedTime = 0f;

        _nextAttackTime = Time.time + _duration + attack.Recovery / speed;

        // 데미지 계산
        float damage = baseDamage * attack.DamageMultiplier * stats.Damage_All * stats.Damage_Weapon * stats.Damage_Normal;
        _damage = Mathf.Max(0, Mathf.RoundToInt(damage) + stats.Damage_Fixed);

        _hitTargets.Clear();
        _isAttacking = true;

        // 검 끝 판정 위치 설정
        _hitPoint.localPosition = Vector3.right * attack.HitDistance;

        if (_slashRenderer != null)
        {
            _slashRenderer.sprite = attack.SlashSprite;
            _slashRenderer.enabled = false;
        }

        ApplyPose(0f);

        return true;
    }

    private void UpdateAttack(float deltaTime)
    {
        float previous = Progress;

        _elapsedTime = Mathf.Min(_duration, _elapsedTime + deltaTime);

        float current = Progress;

        // 검의 실제 위치와 회전 갱신
        ApplyPose(current);
        // 타격 가능한 구간 검사
        DetectHits(previous, current);
        // 베기 이펙트
        UpdateSlashEffect(current);

        if (current >= 1f)
        {
            EndAttack();
        }
    }

    private void EndAttack()
    {
        _isAttacking = false;
        _hitTargets.Clear();

        _weaponSwing.localRotation = Quaternion.identity;
        _weaponSwing.localPosition = (Vector3)_attackData.GripOffset;

        if (_slashRenderer != null)
        {
            _slashRenderer.enabled = false;
        }
    }

    #endregion

    #region 공격 모션

    private void ApplyPose(float progress)
    {
        float angle = _attackData.EvaluateAngle(progress);
        float reach = _attackData.EvaluateReach(progress);

        Vector2 position = _attackData.GripOffset + Vector2.right * reach;

        _weaponSwing.localPosition = new Vector3(position.x, position.y, 0f);
        _weaponSwing.localRotation = Quaternion.Euler(0f, 0f, angle);
    }

    // 검 끝의 월드 좌표 계산
    private Vector2 EvaluateHitPosition(float progress)
    {
        float angle = _attackData.EvaluateAngle(progress);
        float reach = _attackData.EvaluateReach(progress);

        Vector3 grip = (Vector3)(_attackData.GripOffset + Vector2.right * reach);
        Vector3 localTip = grip + Quaternion.Euler(0f, 0f, angle) * Vector3.right * _attackData.HitDistance;
                
        return transform.TransformPoint(localTip);
    }

    #endregion

    #region 공격 적중 판정

    private void DetectHits(float previous, float current)
    {
        float start = Mathf.Max(previous, _attackData.HitStart);
        float end = Mathf.Min(current, _attackData.HitEnd);
        if (start > end)
        {
            return;
        }

        // 원호가 클수록 더 작은 구간으로 나눠 판정
        float angleDelta = Mathf.Abs(_attackData.EvaluateAngle(end) - _attackData.EvaluateAngle(start));
        float reachDelta = Mathf.Abs(_attackData.EvaluateReach(end) - _attackData.EvaluateReach(start));

        int steps = Mathf.Clamp(Mathf.CeilToInt(Mathf.Max(angleDelta / 10f, reachDelta / 0.1f)), 1, 24);
        Vector2 previousPoint = EvaluateHitPosition(start);

        CheckOverlap(previousPoint);

        // 2중 For문이라 퍼포먼스 고려해봐야 하긴 함.
        for (int i = 1; i <= steps; i++)
        {
            float t = Mathf.Lerp(start, end, (float)i / steps);

            Vector2 point = EvaluateHitPosition(t);
            Vector2 delta = point - previousPoint;

            float distance = delta.magnitude;

            if (distance > 0.0001f)
            {
                int count = Physics2D.CircleCastNonAlloc
                    (
                        previousPoint,
                        _attackData.HitRadius,
                        delta / distance,
                        _castResults,
                        distance,
                        _targetLayers
                    );

                for (int j = 0; j < count; j++)
                {
                    TryHit(_castResults[j].collider);
                }
            }

            CheckOverlap(point);
            previousPoint = point;
        }
    }

    private void CheckOverlap(Vector2 position)
    {
        int count = Physics2D.OverlapCircleNonAlloc
            (
                position,
                _attackData.HitRadius,
                _overlapResults,
                _targetLayers
            );

        for (int i = 0; i < count; i++)
        {
            TryHit(_overlapResults[i]);
        }
    }

    // 공격 시도
    private void TryHit(Collider2D targetCollider)
    {
        if (targetCollider == null)
        {
            return;
        }

        // 플레이어 자신은 제외
        if (targetCollider.transform.root == transform.root)
        {
            return;
        }

        IDamageable target = targetCollider.GetComponentInParent<IDamageable>();

        if (target == null)
        {
            return;
        }

        // 같은 공격 패턴 중 몬스터당 1회만 타격
        if (!_hitTargets.Add(target))
        {
            return;
        }

        target.TakeDamage(_damage);
    }

    #endregion



    // 검로 이펙트
    private void UpdateSlashEffect(float progress)
    {
        if (_slashRenderer == null)
        {
            return;
        }

        bool active = _attackData.SlashSprite != null && progress >= _attackData.HitStart && progress <= _attackData.HitEnd;
        _slashRenderer.enabled = active;

        if (!active)
        {
            return;
        }

        // 검과 같은 위치·회전 사용
        _slashRenderer.transform.localPosition = _weaponSwing.localPosition;
        _slashRenderer.transform.localRotation = _weaponSwing.localRotation;
        _slashRenderer.transform.localScale = Vector3.one * _attackData.SlashScale;

        // 타격 구간 동안 나타났다 사라지는 효과
        float t = Mathf.InverseLerp
            (
                _attackData.HitStart,
                _attackData.HitEnd,
                progress
            );

        Color color = _slashRenderer.color;
        color.a = Mathf.Sin(t * Mathf.PI);

        _slashRenderer.color = color;
    }

    private void OnDisable()
    {
        _isAttacking = false;
        _hitTargets.Clear();

        if (_slashRenderer != null)
        {
            _slashRenderer.enabled = false;
        }

        if (_weaponSwing != null)
        {
            _weaponSwing.localRotation = Quaternion.identity;

            if (_attackData != null)
            {
                _weaponSwing.localPosition = (Vector3)_attackData.GripOffset;
            }
        }
    }

    // 검 궤적 미리보기
    private void OnDrawGizmosSelected()
    {
        if (_previewAttack == null)
        {
            return;
        }
        const int segments = 40;
        Vector3 previous = Vector3.zero;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;

            float angle = _previewAttack.EvaluateAngle(t);
            float reach = _previewAttack.EvaluateReach(t);

            Vector3 grip = (Vector3)(_previewAttack.GripOffset + Vector2.right * reach);
            Vector3 tip = grip + Quaternion.Euler(0f, 0f, angle) * Vector3.right * _previewAttack.HitDistance;
            Vector3 world = transform.TransformPoint(tip);

            // 타격 가능한 구간은 빨간색
            bool active = t >= _previewAttack.HitStart && t <= _previewAttack.HitEnd;

            Gizmos.color = active ? Color.red : Color.yellow;

            if (i > 0)
            {
                Gizmos.DrawLine(previous, world);
            }

            previous = world;
        }
    }
}