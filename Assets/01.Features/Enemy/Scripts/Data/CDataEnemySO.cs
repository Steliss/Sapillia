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

    public float Hp => _hp;
    public float SuperArmor => _superArmor;
    public float Defense => _defense;
    public float Attack => _attack;
    public float Speed => _speed;
    public float SuperArmorRate => _superArmorRate;
}
