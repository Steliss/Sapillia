using System.Collections.Generic;
using UnityEngine;

public class CWeaponMotion : MonoBehaviour
{
    #region 인스펙터

    [Header("무기")]
    [SerializeField] private Transform _weaponSwing;
    [SerializeField] private Transform _hitPoint;

    [Header("공격 좌표계")]
    [SerializeField] private Transform _attackSpace;

    // ※
    [Header("무기 조준")]
    [SerializeField] private CSwordAndShieldAim _aim;

    [Header("공격 이펙트")]
    [SerializeField] private SpriteRenderer _slashRenderer;

    [Header("공격될 레이어")]
    [SerializeField] private LayerMask _targetLayers;

    [Header("공격 데이터")]
    [SerializeField] private CWeaponAttackDataSO _attackData;
    [SerializeField] private CPlayerGamingDataSO _player;

    [Header("공격자 위치 Transform")]
    [SerializeField] private Transform _attacker;

    [Header("기본 검기 보정")]
    [SerializeField] private Vector2 _slashOffset;
    [SerializeField] private float _slashAngleOffset;

    [Header("디버그")]
    [SerializeField] private bool _debugAttackDirection;
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

    private Quaternion _initialAttackSpaceRotation;

    private Vector3 _slashInitialScale;
    private Color _slashInitialColor;

    private bool _hasInitialPose;

    // 검로가 표시 중인지
    private bool _slashVisible;
    // 검로가 처음 나타난 순간의 월드 회전
    private Quaternion _slashLockedRotation;


    private int _damage;
    private bool[] _effectPlayed;

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

    #region Unity
    private void Awake()
    {
        if (_weaponSwing == null || _hitPoint == null || _attackSpace == null || _aim == null)
        {
            CLog.Error("[WeaponMotion] Weapon 참조 누락");
            enabled = false;
            return;
        }

        if (_weaponSwing.parent != _attackSpace)
        {
            CLog.Warn("[WeaponMotion] WeaponSwing의 부모와 AttackSpace가 다름. 공격 좌표계를 확인하세요.");
        }

        // 최초 무기 위치 및 회전 저장
        SaveInitialPose();

        if (_slashRenderer != null)
        {
            _slashInitialScale = _slashRenderer.transform.localScale;
            _slashInitialColor = _slashRenderer.color;

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

    private void LateUpdate()
    {
        if (!_debugAttackDirection || !_isAttacking || _weaponSwing == null)
        {
            return;
        }

        // 매 프레임 로그가 쌓이지 않도록 제한
        if (Time.frameCount % 10 != 0)
        {
            return;
        }

        float pivotAngle = _aim != null ? _aim.EffectAimRotation.eulerAngles.z : 0f;
        float swingAngle =  _weaponSwing.eulerAngles.z;

        CLog.Log($"[WeaponMotion] Pivot : {pivotAngle:F1}° / SwordGrip : {swingAngle:F1}°");

        // Scene 뷰에서 실제 검끝 방향 표시
        if (_hitPoint != null)
        {
            Debug.DrawLine(_weaponSwing.position, _hitPoint.position, Color.yellow);
        }

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

        // 무기 교체 또는 오브젝트 비활성화 시 조준 잠금 해제
        if (_aim != null)
        {
            _aim.UnlockAim();
        }

    }


    #endregion
    // Inspector에서 설정한 최초 Transform 저장
    private void SaveInitialPose()
    {
        
        _initialLocalPosition = _weaponSwing.localPosition;
        _initialLocalRotation = _weaponSwing.localRotation;
        _initialHitPointPosition = _hitPoint.localPosition;

        // AttackSpace의 초기 로컬 회전 저장
        _initialAttackSpaceRotation = _attackSpace.localRotation;

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

        // 공격 좌표계의 최초 로컬 회전 복원
        if (_attackSpace != null)
        {
            _attackSpace.localRotation = _initialAttackSpaceRotation;
        }


        if (_hitPoint != null)
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
        // 추가 이펙트 실행
        UpdateAttackEffects(current);

        if (current >= 1f)
        {
            BeginHold();
            
        }
    }

    #region 공격
    public bool TryPlay(CWeaponAttackDataSO attack, int baseDamage, SPlayerStats stats, bool isSpecial = false, bool isDash = false)
    {
        if (!CanStart || attack == null)
        {
            return false;
        }

        _attackData = attack;

        ResetAttackEffects();

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

            // 보정되지 않은 실제 마우스 방향
            Vector2 direction = _aim.AimDirection;

            if (direction.sqrMagnitude < 0.0001f)
            {
                direction = Vector2.right;
            }

            float aimAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            // 실제 마우스 각도를 공격 좌표계에 적용
            _attackSpace.rotation = Quaternion.Euler(0f, 0f, aimAngle);

            CLog.Log(
                $"[WeaponMotion] AimDirection : {aimAngle:F1}° / " +
                $"EffectAim : {_aim.EffectAimRotation.eulerAngles.z:F1}° / " +
                $"AttackSpace : {_attackSpace.eulerAngles.z:F1}°");                        
        }

        _damage = CalculateDamage(attack, baseDamage, stats, isSpecial, isDash);       
        
        // 검 끝 판정 위치 설정
        _hitPoint.localPosition = Vector3.right * attack.HitDistance;

        if (_slashRenderer != null)
        {
            _slashRenderer.sprite = attack.SlashSprite;
            _slashRenderer.enabled = false;
        }

        _slashVisible = false;

        ApplyPose(0f);

        string attackType = isDash ? "Dash" : isSpecial ? "Special" : "Normal";

        CLog.Log($"[WeaponMotion] " + $"Type:{attackType} / Damage:{_damage}");

        return true;
    }

    // 최종 데미지 계산
    private int CalculateDamage(CWeaponAttackDataSO attack, int baseDamage, SPlayerStats stats, bool isSpecial, bool isDash)
    {
        float attackMultiplier;

        if (isSpecial)
        {
            // 특수공격
            attackMultiplier = stats.Damage_Special;
        }
        else if (isDash)
        {
            // 대시 공격
            attackMultiplier = attack.DamageMultiplier * stats.Damage_Dash;
        }
        else
        {
            // 일반공격
            attackMultiplier = attack.DamageMultiplier * stats.Damage_Normal;
        }
                        
        float damage = baseDamage * stats.GetAttributePower(EAttribute.Physical) * stats.Damage_All * stats.Damage_Weapon * attackMultiplier;

        if (_player.IsCritical())
        {
            CLog.Log($"[CWeaponMotion] 크리티컬 확인");
            damage = damage * stats.Critical_Damage;
        }

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
        // SO에 설정된 공격 모션
        float angle = _attackData.EvaluateAngle(progress);
        float reach = _attackData.EvaluateReach(progress);

        // WeaponPivot 기준 손잡이 위치
        Vector3 grip = (Vector3)(_attackData.GripOffset + Vector2.right * reach);
        // 공격 커브에 따른 검끝 위치
        Vector3 localTip = grip + Quaternion.Euler(0f, 0f, angle) * Vector3.right * _attackData.HitDistance;
                
        return _attackSpace.TransformPoint(localTip);
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
        
        CLog.Log($"[PlayerAttack] 몬스터 적중\n Damage : {_damage}\n Direction : {damageInfo.IncomingDirection}");
    }

    #endregion



    // 검로 이펙트
    private void UpdateSlashEffect(float progress)
    {
        if (_slashRenderer == null || _weaponSwing == null || _attackData == null)
        {
            return;
        }

        bool active = _attackData.SlashSprite != null && progress >= _attackData.HitStart && progress <= _attackData.HitEnd;

        _slashRenderer.enabled = active;

        if (!active)
        {
            _slashRenderer.enabled = false;
            _slashVisible = false;
            return;
        }

        Transform slashTransform = _slashRenderer.transform;

        // 처음 표시되는 순간에만 회전값 저장
        if (!_slashVisible)
        {
            _slashLockedRotation = _attackSpace.rotation * Quaternion.Euler(0f, 0f, _slashAngleOffset);

            _slashVisible = true;
        }

        // SwordSwing 기준 위치 보정
        Vector3 position = _attackSpace.TransformPoint(_slashOffset);

        // 회전은 처음 나타났을 때의 월드 각도 유지
        slashTransform.position = position;
        slashTransform.rotation = _slashLockedRotation;
        // 초기 스케일을 유지하면서 SO 배율 적용
        slashTransform.localScale = _slashInitialScale * _attackData.SlashScale;

        // 타격 구간에서 나타났다가 사라지는 알파 효과
        float t = Mathf.InverseLerp(_attackData.HitStart, _attackData.HitEnd, progress);
        float alpha = Mathf.Sin(t * Mathf.PI);

        Color color = _slashInitialColor;
        color.a *= alpha;

        _slashRenderer.color = color;
        _slashRenderer.enabled = true;
    }

    private void ResetAttackEffects()
    {
        SWeaponEffectData[] effects = _attackData.Effects;

        _effectPlayed = effects != null ? new bool[effects.Length] : null;
    }

    private void UpdateAttackEffects(float progress)
    {
        if (_attackData == null || _effectPlayed == null)
        {
            return;
        }

        SWeaponEffectData[] effects = _attackData.Effects;

        for (int i = 0; i < effects.Length; i++)
        {
            if (_effectPlayed[i])
            {
                continue;
            }

            if (progress < effects[i].StartProgress)
            {
                continue;
            }

            // 실행 여부를 먼저 기록
            _effectPlayed[i] = true;

            SpawnAttackEffect(effects[i]);
        }
    }

    private void SpawnAttackEffect(SWeaponEffectData effect)
    {
        if (effect.Prefab == null || CObjectPoolManager.Instance == null)
        {
            return;
        }

        Transform anchor = null;

        switch (effect.Anchor)
        {
            case EWeaponEffectAnchor.Player:
                anchor = _attacker;
                break;

            case EWeaponEffectAnchor.Weapon:
                anchor = _weaponSwing;
                break;

            case EWeaponEffectAnchor.HitPoint:
                anchor = _hitPoint;
                break;
        }

        if (anchor == null)
        {
            return;
        }

        GameObject effectObject = CObjectPoolManager.Instance.GetPoolObject(effect.Prefab, anchor);

        if (effectObject == null)
        {
            return;
        }
                
        Transform effectTransform = effectObject.transform;

        // 부모를 따라 위치 이동
        effectTransform.SetParent(anchor, false);
        if (effect.RotationMode == EWeaponEffectRotationMode.Anchor)
        {
            effectTransform.localPosition = effect.Offset;
        }
        else
        {
            effectTransform.localPosition = _aim.AimDirection * 1.2f;
        }
        // 생성 순간의 월드 회전 계산
        Quaternion rotation = GetEffectRotation(effect, anchor);
        effectTransform.rotation = rotation;

        // 생성된 이펙트의 월드 회전 고정
        CWeaponEffectRotationLock rotationLock = effectObject.GetComponent<CWeaponEffectRotationLock>();

        if (rotationLock != null)
        {
            rotationLock.LockRotation(rotation);
        }

        CWeaponEffectFollow follow = effectObject.GetComponent<CWeaponEffectFollow>();

        if (follow != null)
        {
            follow.Initialize(anchor, _aim, effect);
        }

        Destroy(effectObject, 0.5f);
    }

    #region 이펙트 효과 회전

    private Quaternion GetEffectRotation(SWeaponEffectData effect, Transform anchor)
    {
        // 기본값은 생성 기준 Transform의 회전
        Quaternion baseRotation = anchor.rotation;

        // 마우스 조준 회전을 사용하도록 설정한 경우
        if (effect.RotationMode == EWeaponEffectRotationMode.MouseAim && _aim != null)
        {
            baseRotation = _aim.EffectAimRotation;
            
        }

        // SO의 AngleOffset을 추가 보정값으로 적용
        return baseRotation * Quaternion.Euler(0f, 0f, effect.AngleOffset);
    }

    #endregion





    


}