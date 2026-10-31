using Cysharp.Threading.Tasks;
using System;
using UnityEngine;


[CreateAssetMenu(menuName = "SO/Enemy/AttackState/Shout", fileName = "Enemy_Attack_Shout_")]
public class CEnemyAttackStateSO_Shout : CEnemyAttackStateSO
{
    #region 인스펙터
    [Header("이펙트 프리팹")]
    [SerializeField] private GameObject _shoutWave;

    [Header("전/후 딜레이")]
    [SerializeField] private float _beforeDelay = 1.5f;
    [SerializeField] private float _afterDelay = 2f;

    [Header("애니메이터 스테이트")]
    [SerializeField] private string _stateShout = "Shout";
    [SerializeField] private string _stateShoutReady = "ShoutReady";
    #endregion

    #region 내부 변수
    private int _hashShout;
    private int _hashShoutReady;
    #endregion

    private void OnEnable()
    {
        _hashShout = Animator.StringToHash(_stateShout);
        _hashShoutReady = Animator.StringToHash(_stateShoutReady);
    }

    public override void StateEnter()
    {
        _mashine.AttackUni.Run(async ct =>
        {
            // 선 딜레이
            _mashine.Ani.Play(_hashShoutReady);
            await UniTask.Delay(TimeSpan.FromSeconds(_beforeDelay), cancellationToken: ct);

            // 후 딜레이
            _mashine.SuperArmorGet();
            _mashine.HpBarUpdate();
            _mashine.Ani.Play(_hashShout);

            GameObject shoutWave = CreateAttackObject
            (
                _shoutWave, 
                _afterDelay, 
                _mashine.transform.position, 
                Quaternion.identity
            );
            shoutWave.transform.localScale = _shoutWave.transform.localScale;
            for (float time = 0f; time < _afterDelay; time += Time.deltaTime)
            {
                shoutWave.transform.localScale = Vector3.one * (time * 10f);
                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }

            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public override void StateExit()
    {
        _mashine.AttackUni.Cancel();
    }

    public override void StateUpdate()
    {
    }
}
