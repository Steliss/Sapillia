using System;
using UnityEngine;
public enum EWeaponEffectAnchor
{
    Player,
    Weapon,
    HitPoint
}

public enum EWeaponEffectRotationMode
{
    Anchor,
    MouseAim
}


[Serializable]
public struct SWeaponEffectData
{
    [Header("이펙트 프리팹")]
    public GameObject Prefab;

    [Header("재생 시점 (0 ~ 1)")]
    [Range(0f, 1f)]
    public float StartProgress;

    [Header("생성 기준 위치")]
    public EWeaponEffectAnchor Anchor;

    [Header("회전 기준")]
    public EWeaponEffectRotationMode RotationMode;

    [Header("위치 / 회전 보정")]
    public Vector2 Offset;
    public float AngleOffset;
}
