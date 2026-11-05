using UnityEngine;

public enum EDashDustDirection
{
    LeftUp,
    Up,
    RightUp,
    Left,
    Right,
    LeftDown,
    Down,
    RightDown
}

[CreateAssetMenu(menuName = "SO/Player/DashDust", fileName = "DashDustData")]
public class CDashDustDataSO : ScriptableObject
{
    [Header("좌측위")]
    [SerializeField] private Sprite[] _leftUp;

    [Header("위")]
    [SerializeField] private Sprite[] _up;

    [Header("우측위")]
    [SerializeField] private Sprite[] _rightUp;

    [Header("좌측중간")]
    [SerializeField] private Sprite[] _left;

    [Header("우측중간")]
    [SerializeField] private Sprite[] _right;

    [Header("좌측아래")]
    [SerializeField] private Sprite[] _leftDown;

    [Header("아래")]
    [SerializeField] private Sprite[] _down;

    [Header("우측아래")]
    [SerializeField] private Sprite[] _rightDown;

    public Sprite[] GetSprites(EDashDustDirection direction)
    {
        switch (direction)
        {
            case EDashDustDirection.LeftUp: return _leftUp;
            case EDashDustDirection.Up: return _up;
            case EDashDustDirection.RightUp: return _rightUp;
            case EDashDustDirection.Left: return _left;
            case EDashDustDirection.Right: return _right;
            case EDashDustDirection.LeftDown: return _leftDown;
            case EDashDustDirection.Down: return _down;
            case EDashDustDirection.RightDown: return _rightDown;
        }

        return null;
    }
}
