using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class CEnemyBoss01Production : MonoBehaviour
{
    #region 인스펙터

    [Header("돌 공격")]
    [SerializeField] private GameObject _fallingRock;
    [SerializeField] private float _fallDamage;
    [SerializeField] private Vector2 _fallMinPos = new Vector2(12f, 15f);
    [SerializeField] private Vector2 _fallMaxPos = new Vector2(35f, 26f);
    [SerializeField] private Vector2 _fallInterbalPos = new Vector2(3, 1.5f);
    [SerializeField] private float _jjumpTime = 10f;
    [Header("카메라")]
    [SerializeField] private float _zoomInTime;
    [SerializeField] private float _zoomOutTime;
    [SerializeField] private CinemachineVirtualCamera _zoomInCamera;
    [SerializeField] private CinemachineVirtualCamera _zoomOutCamera;
    [Header("시네마틱 UI")]
    [SerializeField] private CanvasGroup _cinematic;
    [SerializeField] private Canvas _textBubble;
    [SerializeField] private TextMeshProUGUI[] _texts;
    [Header("애니메이터")]
    [SerializeField] private GameObject _flag;
    [SerializeField] private Animator _ani;
    [SerializeField] private SpriteRenderer _sp;
    [SerializeField] private string _aniJump = "Jump";
    [SerializeField] private string _aniPhase2 = "Landing";
    [SerializeField] private string _aniDie = "Die";
    [SerializeField] private string _aniLandingDie = "LandingDie";
    [SerializeField] private string _aniIdle = "Idle";
    [SerializeField] private string _aniJumpJump = "JumpJump";
    [SerializeField] private string _aniJumpJumpReady = "JumpJumpReady";
    #endregion

    #region 내부 변수
    private CUniTaskOnes _uni = new CUniTaskOnes();
    private int _hashJump;
    private int _hashPhase2;
    private int _hashDie;
    private int _hashIdle;
    private int _hashJumpJump;
    private int _hashJumpJumpReady;
    private int _hashLandingDie;

    private Transform _target;
    private CObjectPoolManager _op;
    private CinemachineBasicMultiChannelPerlin _noise;
    #endregion

    private void Awake()
    {
        _hashJump = Animator.StringToHash(_aniJump);
        _hashPhase2 = Animator.StringToHash(_aniPhase2);
        _hashDie = Animator.StringToHash(_aniDie);
        _hashIdle = Animator.StringToHash(_aniIdle);
        _hashJumpJumpReady = Animator.StringToHash(_aniJumpJumpReady);
        _hashJumpJump = Animator.StringToHash(_aniJumpJump);
        _hashLandingDie = Animator.StringToHash(_aniLandingDie);

        _noise = _zoomOutCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
    }

    private void Start()
    {
    }

    private void OnDisable()
    {
        _uni.Cancel();
    }

    private void Update()
    {
        if (_noise.m_AmplitudeGain > 0)
        {
            _noise.m_AmplitudeGain -= Time.deltaTime * 5f;
        }
        else
        {
            _noise.m_AmplitudeGain = 0;
        }

        if (_target != null)
        {
            bool isLeft = _target.position.x < _ani.transform.position.x;
            if (_sp.flipX != isLeft)
            {
                _sp.flipX = isLeft;
            }
        }

    }

    public void CameraShaking(float power)
    {
        _noise.m_AmplitudeGain = power;
    }


    public async UniTask StartProduction(CancellationToken ct, Transform target)
    {
        _target = target;
        _zoomInCamera.Priority = 12;

        for (float time = 0f; time < _zoomInTime; time += Time.deltaTime)
        {
            float t = time / _zoomInTime;
            _cinematic.alpha = t;

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _textBubble.gameObject.SetActive(true);

        for (int i = 0; i < 2; i++)
        {
            _texts[i].gameObject.SetActive(true);
            int count = 0;
            float t = 0f;
            _texts[i].maxVisibleCharacters = count;
            _texts[i].ForceMeshUpdate();
            for (float time = 0f; time < 5f; time += Time.deltaTime)
            {
                t += Time.deltaTime;
                if (t > 0.1f)
                {
                    count++;
                    t = 0f;
                    _texts[i].maxVisibleCharacters = count;
                    _texts[i].ForceMeshUpdate();
                }



                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            _texts[i].gameObject.SetActive(false);
        }
        _textBubble.gameObject.SetActive(false);

        _ani.Play(_hashJump);

        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: ct);


        FadeOut().Forget();
        _zoomInCamera.Priority = 1;

        
    }

    private async UniTask FadeOut()
    {
        for (float time = 0f; time < 1f; time += Time.deltaTime)
        {
            float t = 1 - time;
            _cinematic.alpha = t;

            await UniTask.Yield(PlayerLoopTiming.Update);
        }
    }

    public async UniTask PhaseProduction(CancellationToken ct, bool isLeftStart)
    {
        _zoomOutCamera.Priority = 11;
        await UniTask.Delay(TimeSpan.FromSeconds(0.8f), cancellationToken: ct);
        _ani.Play(_hashPhase2);
        await UniTask.Delay(TimeSpan.FromSeconds(0.8f), cancellationToken: ct);

        CameraShaking(3f);

        // 낙석 웨이브
        int x = Mathf.RoundToInt((_fallMaxPos.x - _fallMinPos.x) / _fallInterbalPos.x);
        int y = Mathf.RoundToInt((_fallMaxPos.y - _fallMinPos.y) / _fallInterbalPos.y);
        for (int i = 0; i < x - 1; i++)
        {
            for (int j = 0; j <= y; j++)
            {
                Vector2 fallPos = _fallMinPos;
                if (isLeftStart)
                {
                    fallPos.x += _fallInterbalPos.x * i;
                }
                else
                {
                    fallPos.x = _fallMaxPos.x;
                    fallPos.x -= _fallInterbalPos.x * i;
                }
                fallPos.y += _fallInterbalPos.y * j + UnityEngine.Random.Range(-0.2f, 0.2f);
                fallPos.x += UnityEngine.Random.Range(-0.2f, 0.2f);

                FallingRock(fallPos);
            }
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: ct);
        }


        // 연속 낙석
        for (int i = 0; i < 17; i++)
        {
            _ani.Play(_hashJumpJumpReady);
            await UniTask.Delay(TimeSpan.FromSeconds(0.4f), cancellationToken: ct);

            Vector2 randomPos1 = new Vector2
                (
                    UnityEngine.Random.Range(_fallMinPos.x, _fallMaxPos.x),
                    UnityEngine.Random.Range(_fallMinPos.y, _fallMaxPos.y)
                );
            Vector2 randomPos2 = new Vector2
                (
                    UnityEngine.Random.Range(_fallMinPos.x, _fallMaxPos.x),
                    UnityEngine.Random.Range(_fallMinPos.y, _fallMaxPos.y)
                );
            Vector2 targetRanPos = new Vector2
                (
                    UnityEngine.Random.Range(-2f, 2f),
                    UnityEngine.Random.Range(-1.5f, 1.5f)
                );
            targetRanPos += (Vector2)_target.transform.position;
            _ani.Play(_hashJumpJump);
            FallingRock(randomPos1);
            FallingRock(randomPos2);
            FallingRock(targetRanPos);
            CameraShaking(2f);
            await UniTask.Delay(TimeSpan.FromSeconds(0.3f), cancellationToken: ct);
        }

        _ani.Play(_hashIdle);
        _zoomOutCamera.Priority = 1;
        await UniTask.Delay(TimeSpan.FromSeconds(2f), cancellationToken: ct);

        _ani.Play(_hashJump);

        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ct);

        _uni.Run(async ctt =>
        {
            while (true)
            {
                await UniTask.Delay(TimeSpan.FromSeconds(10f), cancellationToken: ct);
                FallingRock(_target.position);
            }
        });
    }

    private void FallingRock(Vector2 pos)
    {
        if (_op == null)
        {
            _op = CObjectPoolManager.Instance;
        }

        GameObject rock = _op.GetPoolObject(_fallingRock, 3f);
        rock.transform.position = pos;

        CEnemyProjectile_FallingRock projectile = rock.GetComponent<CEnemyProjectile_FallingRock>();
        if (projectile != null)
        {
            projectile.DamageSet(_fallDamage, 2f);
        }
    }

    public async UniTask DieProduction(CancellationToken ct)
    {
        _uni.Cancel();
        _zoomInCamera.Priority = 11;
        for (float time = 0f; time < _zoomInTime; time += Time.deltaTime)
        {
            float t = time / _zoomInTime;
            _cinematic.alpha = t;

            await UniTask.Yield(PlayerLoopTiming.Update, ct);
        }

        _ani.Play(_hashLandingDie);
        await UniTask.Delay(TimeSpan.FromSeconds(1.5f), cancellationToken: ct);
        _textBubble.gameObject.SetActive(true);

        for (int i = 2; i < 4; i++)
        {
            _texts[i].gameObject.SetActive(true);
            int count = 0;
            float t = 0f;
            _texts[i].maxVisibleCharacters = count;
            _texts[i].ForceMeshUpdate();
            for (float time = 0f; time < 5f; time += Time.deltaTime)
            {
                t += Time.deltaTime;
                if (t > 0.1f)
                {
                    count++;
                    t = 0f;
                    _texts[i].maxVisibleCharacters = count;
                    _texts[i].ForceMeshUpdate();
                }



                await UniTask.Yield(PlayerLoopTiming.Update, ct);
            }
            _texts[i].gameObject.SetActive(false);
        }
        _textBubble.gameObject.SetActive(false);

        _ani.Play(_hashDie);

        await UniTask.Delay(TimeSpan.FromSeconds(7f), cancellationToken: ct);
        FadeOut().Forget();
        _zoomInCamera.Priority = 1;
        await UniTask.Delay(TimeSpan.FromSeconds(1f), cancellationToken: ct);
        _flag?.SetActive(true);
    }


}
