using Cinemachine;
using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using TMPro;
using UnityEngine;

public class CEnemyBoss01Production : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private float _zoomInTime;
    [SerializeField] private float _zoomOutTime;

    [Header("카메라")]
    [SerializeField] private CinemachineVirtualCamera _zoomInCamera;
    [SerializeField] private CinemachineVirtualCamera _zoomOutCamera;
    [Header("시네마틱 UI")]
    [SerializeField] private CanvasGroup _cinematic;
    [SerializeField] private Animator _aniProduction;
    [SerializeField] private Canvas _textBubble;
    [SerializeField] private TextMeshProUGUI[] _texts;
    [Header("애니메이터 해시용")]
    [SerializeField] private string _aniJump;
    [SerializeField] private string _aniPhase2;
    [SerializeField] private string _aniDie;

    #endregion

    #region 내부 변수
    private CUniTaskOnes _uni = new CUniTaskOnes();
    private int _hashJump;
    private int _hashPhase2;
    private int _hashDie;
    #endregion

    private void Awake()
    {
        _hashJump = Animator.StringToHash(_aniJump);
        _hashPhase2 = Animator.StringToHash(_aniPhase2);
        _hashDie = Animator.StringToHash(_aniDie);
    }


    public async UniTask StartProduction(CancellationToken ct)
    {
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

        _aniProduction.Play(_hashJump);

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

}
