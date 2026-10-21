using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/State_Attack", fileName = "Enemy_Attack_")]
public class CEnemyStateSO_Attack : ScriptableObject, IEnemyState
{
    [SerializeField] private float _beforeDelay = 2f;
    [SerializeField] private float _afterDelay = 1f;
    [SerializeField] private GameObject _projectile;
    [SerializeField] private GameObject _warningLine;
    [SerializeField] private float _rushDistanse = 5f;
    [SerializeField] private float _attackRange = 3f;
    
    private CStateMashine _mashine;
    private CUniTaskOnes _uni = new CUniTaskOnes();

    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        _uni.Cancel();
        _uni.Run(async ct =>
        {
            // 선딜레이
            _mashine.Ani.Play(_mashine.HashAttReady);

            Vector3 myPos = _mashine.transform.position;
            Vector3 tarPos = _mashine.transform.position + (Vector3.right * _rushDistanse);
            Vector3 dir = tarPos - myPos;
            float z = (Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg) - 90f;
            Quaternion angle = Quaternion.Euler(0, 0, z);

            if (dir.x < 0)
            {
                _mashine.Sprite.flipX = true;
            }
            else
            {
                _mashine.Sprite.flipX = false;
            }

            GameObject line1 = null;
            GameObject line2 = null;
            //GameObject line3 = null;
            //GameObject line4 = null;


            if (_warningLine != null && CObjectPoolManager.Instance != null)
            {
                line1 = CObjectPoolManager.Instance.TryGetObjectPool(_warningLine, _beforeDelay + 1f);
                line2 = CObjectPoolManager.Instance.TryGetObjectPool(_warningLine, _beforeDelay + 1f);

                //line3 = CObjectPoolManager.Instance.TryGetObjectPool(_warningLine, _beforeDelay + 1f);
                //line4 = CObjectPoolManager.Instance.TryGetObjectPool(_warningLine, _beforeDelay + 1f);

                Vector3 pos1 = myPos + Vector3.up * (_rushDistanse / 2f);
                Vector3 pos2 = myPos + Vector3.up * (_rushDistanse / 2f);
                pos1.x += 1f;
                pos2.x -= 1f;

                line1.transform.RotateAround(myPos, Vector3.forward, z);
                line2.transform.RotateAround(myPos, Vector3.forward, z);

                /*
                line1.transform.position = myPos + (dir.normalized * (_rushDistanse / 2f));
                line1.transform.rotation = angle;
                line2.transform.position = myPos + (dir.normalized * (_rushDistanse / 2f));
                line2.transform.rotation = angle;
                */
            }


            await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay), false, PlayerLoopTiming.Update, ct);


            // 공격
            _mashine.Ani.Play(_mashine.HashAttack);
            _mashine.Rigidbody.AddForce(Vector2.right * _rushDistanse, ForceMode2D.Impulse);
            

            GameObject go = null; 

            if (_projectile != null && CObjectPoolManager.Instance != null)
            {
                go = CObjectPoolManager.Instance.TryGetObjectPool(_projectile, _afterDelay);
                go.transform.localRotation = Quaternion.identity;
                go.transform.rotation = angle;
                go.transform.position = _mashine.gameObject.transform.position + dir.normalized;
                go.transform.parent = _mashine.gameObject.transform;
            }

            
            // 후딜레이
            await UniTask.Delay(TimeSpan.FromSeconds(_afterDelay), false, PlayerLoopTiming.Update, ct);
            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public void StateExit()
    {
        _uni.Cancel();
    }

    public void StateUpdate()
    {
        
    }

}
