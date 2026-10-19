using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/Data", fileName = "Enemy_Data_")]
public class CDataEnemySO : CDataSO
{
    [SerializeField] private float _hp;
    [SerializeField] private float _superArmor;
    [SerializeField] private float _defense;
    [SerializeField] private float _attack;
    [SerializeField] private float _speed;

    public float Hp => _hp;
    public float SuperArmor => _superArmor;
    public float Defense => _defense;
    public float Attack => _attack;
    public float Speed => _speed;
}
