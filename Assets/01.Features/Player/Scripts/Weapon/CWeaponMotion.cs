using System.Collections.Generic;
using UnityEngine;

public class CWeaponMotion : MonoBehaviour
{
    #region 인스펙터

    [Header("무기")]
    [SerializeField] private Transform _weaponSwing;
    [SerializeField] private Transform _hitPoint;

    // ※
    [Header("무기 조준")]
    [SerializeField] private CSwordAndShieldAim _aim;

    [Header("공격 이펙트")]
    [SerializeField] private SpriteRenderer _slashRenderer;

    [Header("공격될 레이어")]
    [SerializeField] private LayerMask _targetLayers;

    [Header("공격 데이터")]
    [SerializeField] private CWeaponAttackDataSO _attackData;

    [Header("공격자 위치 Transform")]
    [SerializeField] private Transform _attacker;


    // ※ 테스트 용도
    [Header("테스트 용도")]
    [SerializeField] CWeaponAttackDataSO _previewAttack;
    #endregion

    #region 내부변수
    private bool _isAttacking;
    private bool _isHolding;

    private float _elapsedTime;
    private float _duration;
    private float _nextAttackTime;

    private float _holdTimer;
    private float _holdDuration;

    // 최초 자세
    private Vector3 _initialLocalPosition;
    private Quaternion _initialLocalRotation;
    private Vector3 _initialHitPointPosition;

    private bool _hasInitialPose;

    private int _damage;


    private readonly HashSet<IDamageable> _hitTargets = new HashSet<IDamageable>();

    private readonly Collider2D[] _overlapResults = new Collider2D[32];
    private readonly RaycastHit2D[] _castResults = new RaycastHit2D[32];
    #endregion

    #region 프로퍼티

    public bool IsAttacking => _isAttacking;
    public bool IsHolding => _isHolding;
    public bool CanStart => isActiveAndEnabled && !_isAttacking && Time.time >= _nextAttackTime;
    public float Progress => _duration > 0f ? Mathf.Clamp01(_elapsedTime / _duration) : 1f;
    #endregion

    private void Awake()
    {
        if (_weaponSwing == null || _hitPoint == null)
        {
            CLog.Error("[WeaponMotion] Weapon 참조 누락");
            enabled = false;
            return;
        }

        // 최초 무기 위치 및 회전 저장
        SaveInitialPose();

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

        if (_isHolding)
        {
            UpdateHold(Time.deltaTime);
            return;
        }

        UpdateAttack(Time.deltaTime);
    }

    // Inspector에서 설정한 최초 Transform 저장
    private void SaveInitialPose()
    {
        
        _initialLocalPosition = _weaponSwing.localPosition;
        _initialLocalRotation = _weaponSwing.localRotation;
        _initialHitPointPosition = _hitPoint.localPosition;

        _hasInitialPose = true;
    }

    private void ResetPose()
    {
        if(!_hasInitialPose || _weaponSwing == null)
        {
            return;
        }

        // 최초 손잡이 위치 복원
        _weaponSwing.localPosition = _initialLocalPosition;

        // 최초 무기 회전 복원
        _weaponSwing.localRotation = _initialLocalRotation;

        if(_hitPoint != null)
        {
            _hitPoint.localPosition = _initialHitPointPosition;
        }
    }


    private void UpdateAttack(float deltaTime)
    {
        float previous = Progress;

        _elapsedTime = Mathf.Min(_duration, _elapsedTime + deltaTime);

        float current = Progress;

        // 검의 실제 위치와 회전 갱신
        ApplyPose(current);
        // SO에 설정된 범위로 타격 가능한 구간 검사
        DetectHits(previous, current);
        // 베기 이펙트
        UpdateSlashEffect(current);

        if (current >= 1f)
        {
            BeginHold();
            
        }
    }

    #region 공격
    public bool TryPlay(CWeaponAttackDataSO attack, int baseDamage, SPlayerStats stats, bool isSpecial = false)
    {
        if (!CanStart || attack == null)
        {
            return false;
        }

        _attackData = attack;

        float speed = Mathf.Max(0.01f, stats.Attack_Speed);

        _duration = Mathf.Max(0.01f, attack.Duration / speed);
        _holdDuration = Mathf.Max(0f, attack.Recovery / speed);
        
        _nextAttackTime = Time.time + _duration + _holdDuration;

        _elapsedTime = 0f;
        _holdTimer = 0f;

        _isAttacking = true;
        _isHolding = false;

        _hitTargets.Clear();

        // 무기 잠금
        if (_aim != null)
        {
            _aim.LockAim();
        }

        _damage = CalculateDamage(attack, baseDamage, stats, isSpecial);       
        
        // 검 끝 판정 위치 설정
        _hitPoint.localPosition = Vector3.right * attack.HitDistance;

        if (_slashRenderer != null)
        {
            _slashRenderer.sprite = attack.SlashSprite;
            _slashRenderer.enabled = false;
        }

        ApplyPose(0f);

        // 공격 확인용 테스트 ※
        CLog.Log
           (
                $"[WeaponMotion] " +
                $"Type:{(isSpecial ? "Special" : "Normal")} / " +
                $"Damage:{_damage} / " +
                $"Radius:{attack.HitRadius} / " +
                $"Distance:{attack.HitDistance}"
           );

        return true;
    }

    // 최종 데미지 계산
    private int CalculateDamage(CWeaponAttackDataSO attack, int baseDamage, SPlayerStats stats, bool isSpecial)
    {
        // 일반공격은 공격 패턴 배율을 적용
        // 특수공격은 플레이어의 Damage_Special 사용
        float attackMultiplier = isSpecial ? stats.Damage_Special : attack.DamageMultiplier * stats.Damage_Normal;
        float damage = baseDamage * stats.Damage_All * stats.Damage_Weapon * attackMultiplier;

        return Mathf.Max(0, Mathf.RoundToInt(damage) + stats.Damage_Fixed);
    }





    private void EndAttack()
    {
        _isAttacking = false;
        _isHolding = false;

        _elapsedTime = 0f;
        _holdTimer = 0f;

        _hitTargets.Clear();

        if (_slashRenderer != null)
        {
            _slashRenderer.enabled = false;
        }

        // 최초 무기 자세로 복귀
        ResetPose();

        // 무기 잠금 해제
        if(_aim != null )
        {
            _aim.UnlockAim();
        }
    }

    #endregion

    #region 공격 모션

    private void ApplyPose(float progress)
    {
        if(_attackData == null || _weaponSwing == null)
        {
            return;
        }

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

    #region 공격 후 자세 유지
    private void BeginHold()
    {
        _isHolding = true;

        // Recovery만큼 마지막 자세 유지
        _holdTimer = _holdDuration;

        //_hitTargets.Clear();

        if(_slashRenderer != null)
        {
            _slashRenderer.enabled = false;
        }

        if(_holdTimer <= 0f)
        {
            EndAttack();
        }
    }

    private void UpdateHold(float deltaTime)
    {
        _holdTimer = Mathf.Max(0f, _holdTimer - deltaTime);

        if(_holdTimer <= 0f)
        {
            EndAttack();
        }
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
        if (targetCollider == null || _attacker == null)
        {
            return;
        }

        // 본인 타격 방지
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

        Vector2 incomingDircetion = (Vector2)targetCollider.bounds.center - (Vector2)_attacker.position;
        SDamageInfo damageInfo = new SDamageInfo(_damage, incomingDircetion);

        CDamageUtility.DealDamage(target, damageInfo);                       

        target.TakeDamage(_damage);
        CLog.Log($"[PlayerAttack] 몬스터 적중\n Damage : {_damage}\n Direction : {damageInfo.IncomingDirection}");
    }

    #endregion



    // 검로 이펙트
    private void UpdateSlashEffect(float progress)
    {
        if (_slashRenderer == null || _attackData == null)
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
        _isHolding = false;

        _elapsedTime = 0f;
        _holdTimer = 0f;

        _hitTargets.Clear();

        if (_slashRenderer != null)
        {
            _slashRenderer.enabled = false;
        }

        ResetPose();
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