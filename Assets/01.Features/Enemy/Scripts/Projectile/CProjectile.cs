using UnityEngine;

public class CProjectile : MonoBehaviour
{
    [SerializeField] private float _speed;
    [SerializeField] private LayerMask _targetLayer;

    private Animator _ani;

    private void Awake()
    {
        _ani = GetComponent<Animator>();
    }



    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & _targetLayer) != 0)
        {
            CLog.Log("플레이어 타격");
        }
    }


}
