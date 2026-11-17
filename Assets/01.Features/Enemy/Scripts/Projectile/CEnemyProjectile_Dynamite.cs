using UnityEngine;

public class CEnemyProjectile_Dynamite : CEnemyProjectile
{
    #region 내부 변수
    private float _jumpTime = 0f;
    private float _jumpTimeMax = 1f;
    private string _soundIng = "AttackDynamiteFuse";
    private string _soundEnd = "HitDynamite";
    private CSoundManager _soundManager;
    #endregion

    private void OnEnable()
    {
        if (_soundManager == null)
        {
            if (CManagerDontDestroy.Instance != null)
            {
                _soundManager = CManagerDontDestroy.Instance.Sound;
            }
            else
            {
                return;
            }
        }
        _soundManager?.PlayEffect(_soundIng, transform.position);
    }

    protected override void Update()
    {
        if (!_rb.simulated)
        {
            return;
        }

        _attackTime -= Time.deltaTime;
        MoveAndJump();

        if (_attackTime > 0f)
        {
            return;
        }

        // 폭발하며 데미지를 주기
        _ani.Play(_hashEnd);
        _soundManager?.PlayEffect(_soundEnd, transform.position);
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

    private void MoveAndJump()
    {
        // 위로 올라갔다가 떨어지기

        // 목표 위치로 이동
        transform.position = Vector2.Lerp(transform.position, _dir, Time.deltaTime * 2f);

        if (_jumpTime > _jumpTimeMax)
        {
            // 바닥에 떨어짐.
            _ani.transform.localPosition = Vector3.zero;
            return;
        }

        _jumpTime += Time.deltaTime;
        float t = _jumpTime / _jumpTimeMax;
        float y = Mathf.Sin(t * Mathf.PI);

        Vector2 jump = _ani.transform.localPosition;
        jump.y = y;
        _ani.transform.localPosition = jump;
    }


}
