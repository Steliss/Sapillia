using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CEnemyProjectile_FallingRock : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private Animator _ani;
    [SerializeField] private string _aniEnd;
    [SerializeField] private LayerMask _targetLayer;
    [SerializeField] private Transform _warningCircle;
    [SerializeField] private Transform _rock;
    #endregion

    #region 내부 변수
    private int _hashEnd;
    private float _damage;
    private float _fallingTimeMax = 2f;
    private float _fallingTime;
    private bool _isEnd;
    #endregion

    private void Awake()
    {
        if (!string.IsNullOrWhiteSpace(_aniEnd))
        {
            _hashEnd = Animator.StringToHash(_aniEnd);
        }
    }


    private void Update()
    {
        if (_isEnd)
        {
            return;
        }

        Drop();

    }


    public void DamageSet(float damage, float fallingTime)
    {
        _damage = damage;
        _fallingTime = 0f;
        _fallingTimeMax = fallingTime;
        _isEnd = false;
    }

    private void Drop()
    {
        if (_fallingTime > _fallingTimeMax)
        {
            // 바닥에 떨어짐.
            _rock.localPosition = Vector3.zero;
            _warningCircle.localScale = Vector2.one;
            _ani.Play(_hashEnd);
            _isEnd = true;
            DamageGive();
            return;
        }

        _fallingTime += Time.deltaTime;
        float t = _fallingTime / _fallingTimeMax;
        float y = Mathf.Lerp(_fallingTimeMax * 30f, 0f, t);
        float s = Mathf.Lerp(0f, 1f, t);
        Vector2 scale = new Vector2(s, s);
        _warningCircle.localScale = scale;

        Vector2 drop = _rock.localPosition;
        drop.y = y;
        _rock.localPosition = drop;
    }

    private void DamageGive()
    {
        Collider2D[] hits = Physics2D.OverlapCapsuleAll
           (transform.position, new Vector2(4f, 2f), CapsuleDirection2D.Horizontal, 0f, _targetLayer);

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

}
