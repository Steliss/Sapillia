using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CTabletEffectLange
{
    [SerializeField] private Vector2Int _offset;
    [SerializeField] private int _levelChange;

    public Vector2Int Offset => _offset;
    public int LevelChange => _levelChange;

    public CTabletEffectLange(Vector2Int offset, int levelChange)
    {
        _offset = offset;
        _levelChange = levelChange;
    }
}

[CreateAssetMenu(fileName = "Tablet_", menuName = "SO/Item/Tablet")]
public class CTabletSO : CItemSO
{
    [Header("석판 정보")]
    [SerializeField] private EArtifactGrade _grade;
    [SerializeField] private bool _canRotate = true;

    [Header("석판 범위")]
    [SerializeField] private List<CTabletEffectLange> _effects = new();

    public override EItemType Type => EItemType.Tablet;

    public EArtifactGrade Grade => _grade;
    public bool CanRotate => _canRotate;



    // 슬롯의 현재 회전 상태에 맞는 전체 영향 범위 계산
    public List<CTabletEffectLange> RotatedEffects(ETabletRotation rotation)
    {
        List<CTabletEffectLange> result = new();

        foreach (CTabletEffectLange effect in _effects)
        {
            Vector2Int offset = effect.Offset;

            if (_canRotate)
            {
                if (rotation == ETabletRotation.Rotation90)
                {
                    offset = new Vector2Int(offset.y, -offset.x);
                }
                else if (rotation == ETabletRotation.Rotation180)
                {
                    offset = new Vector2Int(-offset.x, -offset.y);
                }
                else if (rotation == ETabletRotation.Rotation270)
                {
                    offset = new Vector2Int(-offset.y, offset.x);
                }
            }

            result.Add(new CTabletEffectLange(offset, effect.LevelChange));
        }

        return result;
    }


    // 다음 시계 방향 회전 상태 반환
    public ETabletRotation NextRotation(ETabletRotation rotation)
    {
        if (!_canRotate)
        {
            return ETabletRotation.None;
        }

        if (rotation == ETabletRotation.None)
        {
            return ETabletRotation.Rotation90;
        }
        else if (rotation == ETabletRotation.Rotation90)
        {
            return ETabletRotation.Rotation180;
        }
        else if (rotation == ETabletRotation.Rotation180)
        {
            return ETabletRotation.Rotation270;
        }

        return ETabletRotation.None;
    }
}