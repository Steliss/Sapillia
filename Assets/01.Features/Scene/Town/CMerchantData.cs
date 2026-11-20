using UnityEngine;

[System.Serializable]
public class CMerchantData
{
    [SerializeField] EMarchant _type;
    [SerializeField] private AnimatorOverrideController _controller;
    [SerializeField] private int bargain = 10;
    [SerializeField] private int _money = 2000;

    public EMarchant Type => _type;
    public AnimatorOverrideController Controller => _controller;
    public int Bargain => bargain;
    public int Money => _money;

    public void AddMoney(int value)
    {
        _money += value;
    }
}