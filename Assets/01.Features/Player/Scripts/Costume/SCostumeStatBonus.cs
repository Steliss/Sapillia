using UnityEngine;

public struct SCostumeStatBonus
{
    #region 코스튬별 보너스 능력치
    // Armored Rabbit
    [Header("방어력")]
    public int def;

    // Frog / RedClothCat / WizardRabbit
    [Header("속성치")]
    public int Physical;
    public int Fire;
    public int Ice;
    public int Lightning;

    // Pink
    [Header("HP / MP")]
    public int MaxHP;
    public int MaxMP;
    #endregion
}