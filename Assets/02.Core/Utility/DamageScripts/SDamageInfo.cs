using UnityEngine;
public struct SDamageInfo
{
    public int Damage;
    public Vector2 IncomingDirection;
    public bool HasDirection;

    public SDamageInfo(int damage)
    {
        Damage = damage;
        IncomingDirection = Vector2.zero;
        HasDirection = false;
    }

    public SDamageInfo(int damage, Vector2 incomingDirection)
    {
        Damage = damage;
        IncomingDirection = incomingDirection.normalized;
        HasDirection = true;
    }
}