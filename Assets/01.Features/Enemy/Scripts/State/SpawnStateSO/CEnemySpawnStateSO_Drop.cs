using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

[CreateAssetMenu(menuName = "SO/Enemy/SpawnState/Drop", fileName = "Enemy_Spawn_Drop_")]
public class CEnemySpawnStateSO_Drop : CEnemySpawnStateSO
{
    #region 인스펙터
    [Header("이펙트 프리팹")]
    [SerializeField] private GameObject _stamp;
    [Header("애니메이터")]
    [SerializeField] private string _aniStart;
    [SerializeField] private string _aniEnd;
    [Header("떨어질 높이")]
    [SerializeField] private float _startHigh = 20f;
    [SerializeField] private float _randomRange = 10f;
    #endregion

    #region 내부 변수
    private int _hashStart;
    private int _hashEnd;
    #endregion

    #region 유니티 이벤트 함수
    private void OnEnable()
    {
        if (!String.IsNullOrWhiteSpace(_aniStart))
        {
            _hashStart = Animator.StringToHash(_aniStart);
        }
        else
        {
            _hashStart = Animator.StringToHash("Air");
        }

        if (!String.IsNullOrWhiteSpace(_aniEnd))
        {
            _hashEnd = Animator.StringToHash(_aniEnd);
        }
        else
        {
            _hashEnd = Animator.StringToHash("Idle");
        }
    }
    #endregion


    public override void StateEnter()
    {
        _mashine.AttackUni.Run(async ct =>
        {
            Transform tr = _mashine.Sprite.transform;
            float ranHigh = _startHigh + UnityEngine.Random.Range
                (-_randomRange * 0.5f, _randomRange * 0.5f);
            Vector2 high = Vector3.up * ranHigh;
            tr.localPosition = high;
            float speed = 0f;
            _mashine.Collider.enabled = false;
            _mashine.Ani.Play(_hashStart);

            while (tr.localPosition.y > 0)
            {
                speed += 9.81f * Time.deltaTime;
                high.y -= speed * Time.deltaTime;

                tr.localPosition = high;

                await UniTask.Yield(PlayerLoopTiming.Update, ct);

            }
            tr.localPosition = Vector3.zero;
            _mashine.Ani.Play(_hashEnd);

            // 이펙트 생성
            if (_stamp != null && CObjectPoolManager.Instance != null)
            {
                GameObject go = CObjectPoolManager.Instance.GetPoolObject(_stamp, 2f);
                Vector2 goPos = tr.position;
                goPos.y -= 0.4f;
                go.transform.position = goPos;
            }

            tr.localPosition = Vector3.zero;
            _mashine.StateChanged(_mashine.IdleState);
        });
    }

    public override void StateExit()
    {
        _mashine.Nav.enabled = true;
        _mashine.Collider.enabled = true;
    }

    public override void StateUpdate()
    {
        
    }

}
