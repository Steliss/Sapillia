using UnityEngine;
public static class CDamageUtility
{
   public static void DealDamage(IDamageable target, SDamageInfo damageInfo)
    {
        if(target ==null || damageInfo.Damage <= 0)
        {
            return;
        }

        if(target is IDamageInfoReceiver reciver)
        {
            reciver.TakeDamage(damageInfo);
        }

        target.TakeDamage(damageInfo.Damage);
    }
}
