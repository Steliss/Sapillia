using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyProjectile_Strike : CEnemyProjectile_Homing
{
    [SerializeField] private GameObject _fx;
    [SerializeField] private float _fxCoolTimeMax = 0.3f;

    private CObjectPoolManager _objectPool;
    private float _fxCoolTime = 0f;

    private void OnEnable()
    {
        if (_objectPool == null && CObjectPoolManager.Instance != null)
        {
            _objectPool = CObjectPoolManager.Instance;
        }
    }

    protected override void Update()
    {
        _fxCoolTime += Time.deltaTime;
        if (_fxCoolTime > _fxCoolTimeMax )
        {
            _fxCoolTime = 0f;
            GameObject go = _objectPool?.GetPoolObject(_fx, 1f);
            go.transform.position = transform.position;

            CEnemyProjectile_Strike_FX fx = go.GetComponent<CEnemyProjectile_Strike_FX>();
            if (fx != null)
            {
                fx.DamageSet(_damage, _dir, 0.1f);
            }

        }
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        
    }



}
