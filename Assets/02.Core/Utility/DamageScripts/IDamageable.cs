using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage);
    
}

public interface IDirectionalDamageable : IDamageable
{
    void TakeDamage(int damage, Vector2 attackOrigin);
}

public interface IDamageInfoReceiver : IDamageable
{
    void TakeDamage(SDamageInfo damageInfo);
}


