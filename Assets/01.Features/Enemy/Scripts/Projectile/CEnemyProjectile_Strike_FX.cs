using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyProjectile_Strike_FX : CEnemyProjectile
{
    #region 내부 변수
    private bool _isOnes = false;
    #endregion

    private void OnEnable()
    {
        _isOnes = false;
    }

    protected override void Update()
    {
        if (_isOnes)
        {
            return;
        }

        _attackTime -= Time.deltaTime;

        if (_attackTime > 0f)
        {
            return;
        }

        // 폭발하며 데미지를 주기
        _ani.Play(_hashEnd);
        _isOnes = true;

        Collider2D[] hits = Physics2D.OverlapBoxAll
            (transform.position, new Vector2(2f, 1f), 0f, _targetLayer);

        for (int i = 0; i < hits.Length; i++)
        {
            IDamageInfoReceiver player = hits[i].GetComponentInChildren<IDamageInfoReceiver>();

            if (player == null)
            {
                return;
            }
            CLog.Log("데미지를 줌");

            SDamageInfo info = new SDamageInfo(Mathf.RoundToInt(_damage), _dir);

            player.TakeDamage(info);
        }

    }


    protected override void OnTriggerEnter2D(Collider2D collision)
    {

    }


}
