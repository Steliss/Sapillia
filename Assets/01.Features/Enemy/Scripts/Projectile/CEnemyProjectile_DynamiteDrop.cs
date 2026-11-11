using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyProjectile_DynamiteDrop : CEnemyProjectile
{
    #region 내부 변수
    private float _jumpTime = 0f;
    private float _jumpTimeMax = 1f;
    #endregion

    protected override void Update()
    {
        if (!_rb.simulated)
        {
            return;
        }

        _attackTime -= Time.deltaTime;
        Drop();

        if (_attackTime > 0f)
        {
            return;
        }

        // 폭발하며 데미지를 주기
        _ani.Play(_hashEnd);
        _rb.simulated = false;
        _jumpTime = 0f;

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
            Vector2 dir = hits[i].transform.position - transform.position;
            SDamageInfo info = new SDamageInfo(Mathf.RoundToInt(_damage), dir.normalized);

            player.TakeDamage(info);
        }

    }


    protected override void OnTriggerEnter2D(Collider2D collision)
    {

    }

    private void Drop()
    {
        if (_jumpTime > _jumpTimeMax)
        {
            // 바닥에 떨어짐.
            _ani.transform.localPosition = Vector3.zero;
            return;
        }

        _jumpTime += Time.deltaTime;
        float t = _jumpTime / _jumpTimeMax;
        float y = Mathf.Lerp(20f, 0f, t);

        Vector2 drop = _ani.transform.localPosition;
        drop.y = y;
        _ani.transform.localPosition = drop;
    }


}
