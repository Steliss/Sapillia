using UnityEngine;

public interface IDamageable
{
    void TakeDamage(int damage);
    
}

public interface IDamageInfoReceiver : IDamageable
{
    void TakeDamage(SDamageInfo damageInfo);
}


