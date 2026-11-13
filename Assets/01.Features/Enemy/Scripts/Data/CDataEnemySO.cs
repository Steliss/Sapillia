using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/Data", fileName = "Enemy_Data_")]
public class CDataEnemySO : CDataSO
{
    [SerializeField, Min(0f)] private float _hp;
    [SerializeField, Min(0f)] private float _superArmor;
    [SerializeField, Min(0f)] private float _defense;
    [SerializeField, Min(0f)] private float _attack;
    [SerializeField, Min(0f)] private float _speed;
    [Header("슈퍼아머 획득 확률")]
    [SerializeField, Range(0f, 100f)] private float _superArmorRate = 0f;
    [Header("목숨 갯수")]
    [SerializeField, Min(0)] private int _extraLife = 0;

    [Header("드롭 재화")]
    [SerializeField, Min(0f)] private int _exp;
    [SerializeField, Min(0f)] private int _money;

    public float Hp => _hp;
    public float SuperArmor => _superArmor;
    public float Defense => _defense;
    public float Attack => _attack;
    public float Speed => _speed;
    public float SuperArmorRate => _superArmorRate;
    public int ExtraLife => _extraLife;
    public int Exp => _exp;
    public int Money => _money;
}
