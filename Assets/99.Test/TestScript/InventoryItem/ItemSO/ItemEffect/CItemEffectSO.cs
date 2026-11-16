using UnityEngine;


public abstract class CItemEffectSO : ScriptableObject
{
    public abstract void Apply(CPlayerGamingDataSO player);

    public virtual object[] GetDescriptionValues()
    {
        return System.Array.Empty<object>();
    }
}
