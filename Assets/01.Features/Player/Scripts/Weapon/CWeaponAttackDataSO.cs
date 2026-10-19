using UnityEngine;

[CreateAssetMenu(menuName = "SO/Weapon/AttackPattern", fileName = "AttackPattern")]
public class CWeaponAttackDataSO : ScriptableObject
{
    #region Inspector

    [Header("Attack")]
    [Min(0.01f)] [SerializeField] private float _duration = 0.3f;
    [Min(0f)] [SerializeField] private float _recovery = 0.1f;
    [Min(0f)] [SerializeField] private float _damageMultiplier = 1f;

    [Header("손잡이 위치")]
    [SerializeField] private Vector2 _gripOffset = new Vector2(0.2f, 0.25f);

    [Min(0f)]
    [SerializeField] private float _hitDistance = 0.8f;

    [Min(0f)]
    [SerializeField] private float _hitRadius = 0.25f;

    [Header("Motion Curves")]
    [SerializeField] private AnimationCurve _angleCurve = new AnimationCurve
        (
            new Keyframe(0f, -80f),
            new Keyframe(0.7f, 80f),
            new Keyframe(1f, 0f)
        );

    [SerializeField] private AnimationCurve _reachCurve = new AnimationCurve
        (
            new Keyframe(0f, 0f),
            new Keyframe(0.5f, 0.2f),
            new Keyframe(1f, 0f)
        );

    [Header("Hit Window")]
    [Range(0f, 1f)]
    [SerializeField] private float _hitStart = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float _hitEnd = 0.75f;

    [Header("Slash 이펙트")]
    [SerializeField] private Sprite _slashSprite;

    [Min(0f)]
    [SerializeField] private float _slashScale = 1f;

    #endregion

    #region Properties

    public float Duration => _duration;
    public float Recovery => _recovery;
    public float DamageMultiplier => _damageMultiplier;

    public Vector2 GripOffset => _gripOffset;
    public float HitDistance => _hitDistance;
    public float HitRadius => _hitRadius;
    public float HitStart => Mathf.Min(_hitStart, _hitEnd);
    public float HitEnd => Mathf.Max(_hitStart, _hitEnd);
    public Sprite SlashSprite => _slashSprite;
    public float SlashScale => _slashScale;

    #endregion

    #region 공격모션

    public float EvaluateAngle(float progress)
    {
        return _angleCurve.Evaluate(progress);
    }

    public float EvaluateReach(float progress)
    {
        return _reachCurve.Evaluate(progress);
    }

    #endregion
}