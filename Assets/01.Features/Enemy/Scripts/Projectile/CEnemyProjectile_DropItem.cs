using UnityEngine;

public class CEnemyProjectile_DropItem : MonoBehaviour
{
    public enum EState
    {
        None,
        Fall,
        Move,
        End
    }

    #region 인스펙터
    [Header("팔로우 옵션")]
    [SerializeField] private float _followSpeed = 5f;
    [SerializeField] private LayerMask _targetMask;
    [SerializeField] private float _searchRadian = 10f;
    [SerializeField] private float _getDistance = 1f;
    [Header("드랍 옵션")]
    [SerializeField] private Vector2 _fallRandomRange = new Vector2(5f, 3f);
    [SerializeField] private float _jumpRandomHigMin = 1f;
    [SerializeField] private float _jumpRandomHigMax = 5f;
    [SerializeField] private float _jumpRandomTimeMin = 1f;
    [SerializeField] private float _jumpRandomTimeMax = 2f;
    [Header("애니메이터")]
    [SerializeField] private Animator _ani;
    [SerializeField] private string _aniIng = "Ing";
    [SerializeField] private string _aniEnd = "End";
    #endregion


    #region 내부 변수
    private Transform _target;
    private float _exp;
    private float _jumpTime;
    private float _jumpTimeMax;
    private float _jumpHig;
    private Vector2 _fallPos;
    private int _hashIng;
    private int _hashEnd;
    private EState _state = EState.None;
    private float _lifeTime = 1f;
    #endregion

    private void Awake()
    {
        if (_ani == null)
        {
            _ani = gameObject.GetComponentInChildren<Animator>();
        }
        _hashEnd = Animator.StringToHash(_aniEnd);
        _hashIng = Animator.StringToHash(_aniIng);
    }

    private void OnEnable()
    {
        _jumpTime = 0f;
        _jumpTimeMax = UnityEngine.Random.Range(_jumpRandomTimeMin, _jumpRandomTimeMax);
        _jumpHig = UnityEngine.Random.Range(_jumpRandomHigMin, _jumpRandomHigMax);
    }

    private void OnDisable()
    {
        _target = null;
        _state = EState.None;
    }

    private void Update()
    {
        switch (_state)
        {
            case EState.None:
                return;

            case EState.Fall:
                JumpMove();
                break;

            case EState.Move:
                TargetToMove();
                break;

            case EState.End:
                StartLifeTimer();
                break;
        }

        SearchTarget();
    }



    private void JumpMove()
    {
        // 위로 올라갔다가 떨어지기

        // 목표 위치로 이동
        transform.position = Vector2.Lerp(transform.position, _fallPos, Time.deltaTime * 2f);

        if (_jumpTime > _jumpTimeMax)
        {
            // 바닥에 떨어짐.
            _ani.transform.localPosition = Vector3.zero;
            _state = EState.Move;
            return;
        }

        _jumpTime += Time.deltaTime;
        float t = _jumpTime / _jumpTimeMax;
        float y = Mathf.Sin(t * Mathf.PI) * _jumpHig;

        Vector2 jump = _ani.transform.localPosition;
        jump.y = y;
        _ani.transform.localPosition = jump;
    }

    private void SearchTarget()
    {
        if (_target != null)
        {
            return;
        }

        Collider2D hit = Physics2D.OverlapCircle(transform.position, _searchRadian, _targetMask);
        if (hit != null)
        {
            _target = hit.transform;
        }
    }

    private void TargetToMove()
    {
        if (_target == null)
        {
            return;
        }

        Vector2 to = _target.position;
        Vector2 my = transform.position;
        float sqrDistance = Vector2.SqrMagnitude(to - my);
        float sqrGetDistance = _getDistance * _getDistance;

        transform.position = Vector2.MoveTowards(my, to, Time.deltaTime * _followSpeed);

        if (sqrDistance < sqrGetDistance)
        {
            // ※ 연동 획득 함수 추가해야함
            _state = EState.End;
            _ani.Play(_hashEnd);
        }
    }

    public void StartLifeTimer()
    {
        _lifeTime -= Time.deltaTime;
        if ( _lifeTime < 0 )
        {
            gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// 보상 정보와 위치 담기 경험치, 돈, 사파이어, 주사위 
    /// </summary>
    /// <param name="exp">보상 수치</param>
    /// <param name="pos">위치</param>
    public void SetExp(float exp, Vector2 pos)
    {
        _exp = exp;
        Vector2 fallPos = _fallRandomRange * 0.5f;
        Vector2 randomPos = new Vector2
            (
                UnityEngine.Random.Range(-fallPos.x, fallPos.x),
                UnityEngine.Random.Range(-fallPos.y, fallPos.y)
            );
        _fallPos = pos + randomPos;
        _state = EState.Fall;
        _ani.Play(_hashIng);
        _lifeTime = 1f;
    }

}
