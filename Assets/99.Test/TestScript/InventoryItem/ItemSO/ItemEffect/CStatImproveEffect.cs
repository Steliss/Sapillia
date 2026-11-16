using UnityEngine;

[CreateAssetMenu(fileName = "Effect_", menuName = "SO/Item/Effect/StatImprove")]
public class CStatImproveEffect : CItemEffectSO
{
    [SerializeField, Min(0)] private int _increaseAmount;
    [SerializeField] private EAttribute attribute;

    public override void Apply(CPlayerGamingDataSO player)
    {
        if (player == null)
        {
            return;
        }

        player.AddAttributePower(attribute, _increaseAmount);
    }

    public override object[] GetDescriptionValues()
    {
        return new object[] { _increaseAmount };
    }
}