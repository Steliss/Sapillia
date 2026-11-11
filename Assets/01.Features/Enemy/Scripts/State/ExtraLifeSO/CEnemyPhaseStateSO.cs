using Cysharp.Threading.Tasks;
using System;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/PhaseState/Boss01", fileName = "Enemy_Phase_Boss01")]
public class CEnemyPhaseStateSO : ScriptableObject, IEnemyState
{
    [SerializeField] private GameObject _jumpEffect;

    private CStateMashine _mashine;
    private string _aniJumpReady = "BigJumpReady";
    private string _aniJump = "BigJump";
    private int _hashJumpReady;
    private int _hashJump;

    private void OnEnable()
    {
        _hashJump = Animator.StringToHash(_aniJump);
        _hashJumpReady = Animator.StringToHash(_aniJumpReady);
    }

    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
    }

    public void StateEnter()
    {
        // 점프준비
        _mashine.AttackUni.Run(async ct =>
        {
            _mashine.Ani.Play(_hashJumpReady);

            await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ct);

            // 점프
            Transform myTr = _mashine.transform;
            _mashine.Ani.Play(_hashJump);
            _mashine.Collider.enabled = false;
            _mashine.Nav.enabled = false;
            GameObject effect = CObjectPoolManager.Instance.GetPoolObject(_jumpEffect, 1f);
            effect.transform.position = _mashine.transform.position;

            Transform spTr = _mashine.Sprite.transform;
            for (float time = 0f; time < 1f; time += Time.deltaTime)
            {
                float t = time / 1f;
                float y = Mathf.Lerp(0f, 20f, t);
                Vector2 pos = new Vector2(0f, y);
                spTr.localPosition = pos;
                await UniTask.Yield(ct);
            }

            CEnemySpawner_Boss spawner = _mashine.GetComponentInParent<CEnemySpawner_Boss>();
            if (spawner != null)
            {
                spawner.EnemyDie();
            }
            _mashine.gameObject.SetActive(false);
        });
    }

    public void StateExit()
    {
        
    }

    public void StateUpdate()
    {
        
    }

}
