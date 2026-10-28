using UnityEngine;

public abstract class CDataSO : ScriptableObject
{
    [SerializeField] private string _id;
    public string Id => _id;
}