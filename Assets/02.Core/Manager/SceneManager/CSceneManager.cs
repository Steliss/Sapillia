using Cysharp.Threading.Tasks;
using System.Threading;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CSceneManager : MonoBehaviour
{
    [SerializeField] private CSceneListSO _sceneList;
    [SerializeField] private CFlowData _flowData;
    [SerializeField] private CLocalizationManager _localization;

    [Header("페이드 관련")]
    [SerializeField] private GameObject _fadeCanvas;
    [SerializeField] private Image _fadeImage;
    [SerializeField] private TextMeshProUGUI _floorText;
    [SerializeField] private TextMeshProUGUI _MapNameText;
    [SerializeField] private TextMeshProUGUI _MapDescText1;
    [SerializeField] private TextMeshProUGUI _MapDescText2;
    

    private CancellationTokenSource _fadeCts;
    private GameObject _backGroundInstance; 
    private bool _isChanging;

    public bool IsChanging => _isChanging;


    private void Awake()
    {
        if (_sceneList == null)
        {
            CLog.Warn($"{_sceneList} 인스펙터 연결 필요");
        }
        if (_flowData == null)
        {
            CLog.Warn($"{_flowData} 인스펙터 연결 필요");
        }
        if (_localization == null)
        {
            CLog.Warn($"{_localization} 인스펙터 연결 필요");
        }

        Color color = _fadeImage.color;
        color.a = 0f;
        _fadeImage.color = color;
        _fadeCanvas.SetActive(false);
    }

    // 씬 전환 함수
    private void ChangeScene(CSceneInfo scene)
    {
        if (string.IsNullOrWhiteSpace(scene.SceneName))
        {
            CLog.Error($"씬 목록 SO에 씬 이름이 등록되지 않았습니다. ID: {scene.SceneID}");
            return;
        }

        // 해당 씬을 로드할 수 있는지 확인
        if (!Application.CanStreamedLevelBeLoaded(scene.SceneName))
        {
            CLog.Error($"씬을 로드할 수 없습니다. ID: {scene.SceneID}, 이름: '{scene.SceneName}'");
            return;
        }

        SceneManager.LoadScene(scene.SceneName);

    }

    /// <summary>
    ///  씬 전환 호출 함수 
    /// </summary>
    /// <param name="sceneID"></param>
    /// <returns></returns>
    public async UniTask ChangeScreenAsync(EScene sceneID)
    {
        if (_isChanging)
        {
            return;
        }

        CSceneInfo scene = _sceneList.GetSceneName(sceneID);

        if (scene == null)
        {
            CLog.Log("Scene : Null");
            return;
        }

        _isChanging = true;
        _fadeCanvas.SetActive(true);

        try
        {
            if (scene.UseFade)
            {
                Color color = scene.FadeColor;
                color.a = _fadeImage.color.a;
                _fadeImage.color = color;

                CreateBackGround(scene.BackGroundPrefab);
                SetLoadingText(true);
                SetBackGroundActive(true);

                await FadeAsync(1f, scene.FadeOutDuration);
            }

            ChangeScene(scene);

            await UniTask.NextFrame();

            if (scene.UseFade)
            {
                await FadeAsync(0f, scene.FadeInDuration);

                _flowData.CurrentFlow.SetCurrentScene(sceneID);
                SetBackGroundActive(false);
                SetLoadingText(false);
            }
        }
        finally
        {
            _fadeCanvas.SetActive(false);
            _isChanging = false;
        }
    }

    // 배경 프리팹 생성
    private void CreateBackGround(GameObject prefab)
    {
        if (prefab == null)
        {
            return;
        }

        _backGroundInstance = Instantiate(prefab, transform);
    }

    // 배경 프리팹 활성화 
    private void SetBackGroundActive(bool isActive)
    {
        if (_backGroundInstance == null)
        {
            return;
        }

        if(isActive)
        {
            _backGroundInstance.SetActive(isActive);
        }
        else
        {
            Destroy(_backGroundInstance);
        }
    }

    // 타이틀 로딩 씬 설명
    private void SetLoadingText(bool isActive)
    {
        if (isActive)
        {
            if (_flowData.CurrentFlow.CurrentScene == EScene.Menu)
            {
                _floorText.text = _localization.GetText("scene.enmpty.text");
                _MapNameText.text = _localization.GetText("scene.enmpty.text");
                _MapDescText1.text = _localization.GetText("scene.enmpty.text");
                _MapDescText2.text = _localization.GetText("scene.enmpty.text");
            }
            else if (_flowData.CurrentFlow.CurrentScene == EScene.Town)
            {
                _floorText.text = _localization.GetText("scene.stage1.floorname");
                _MapNameText.text = _localization.GetText("scene.stage1.mapname");
                _MapDescText1.text = _localization.GetText("scene.stage1.desc1");
                _MapDescText2.text = _localization.GetText("scene.stage1.desc2");
            }
            else if(_flowData.CurrentFlow.CurrentScene == EScene.stage1)
            {
                _floorText.text = _localization.GetText("scene.stage1.floorname");
                _MapNameText.text = _localization.GetText("scene.stage1.mapname");
                _MapDescText1.text = _localization.GetText("scene.stage1.desc1");
                _MapDescText2.text = _localization.GetText("scene.stage1.desc2");
            }
            else if (_flowData.CurrentFlow.CurrentScene == EScene.stage2)
            {
                _floorText.text = _localization.GetText("scene.stage2.floorname");
                _MapNameText.text = _localization.GetText("scene.stage2.mapname");
                _MapDescText1.text = _localization.GetText("scene.stage2.desc1");
                _MapDescText2.text = _localization.GetText("scene.stage2.desc2");
            }
            else if (_flowData.CurrentFlow.CurrentScene == EScene.stage3)
            {
                _floorText.text = _localization.GetText("scene.stage3.floorname");
                _MapNameText.text = _localization.GetText("scene.stage3.mapname");
                _MapDescText1.text = _localization.GetText("scene.stage3.desc1");
                _MapDescText2.text = _localization.GetText("scene.stage3.desc2");
            }
        }
        else
        {
            _floorText.text = _localization.GetText("scene.enmpty.text");
            _MapNameText.text = _localization.GetText("scene.enmpty.text");
            _MapDescText1.text = _localization.GetText("scene.enmpty.text");
            _MapDescText2.text = _localization.GetText("scene.enmpty.text");
        }
    }


    // 페이드 설정
    private async UniTask FadeAsync(float targetAlpha, float duration)
    {
        float startAlpha = _fadeImage.color.a;
        float deltaTime = 0f;
        Color color = _fadeImage.color;

        _fadeImage.raycastTarget = true;

        while (deltaTime < duration)
        {
            await UniTask.NextFrame();

            deltaTime += Time.unscaledDeltaTime;

            color = _fadeImage.color;
            color.a = Mathf.Lerp(
                startAlpha,
                targetAlpha,
                deltaTime / duration
            );

            _fadeImage.color = color;
        }

        color.a = targetAlpha;
        _fadeImage.color = color;

        _fadeImage.raycastTarget = targetAlpha > 0f ? true : false;
    }


    private void OnDestroy()
    {
        _fadeCts?.Cancel();
    }



}

