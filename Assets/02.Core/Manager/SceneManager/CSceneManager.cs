using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CSceneManager : MonoBehaviour
{
    [SerializeField] private CSceneListSO _sceneList;
    [SerializeField] private Image _fadeImage;

    private CancellationTokenSource _fadeCts;
    private GameObject _backGroundInstance; 
    private bool _isChanging;


    private void Awake()
    {
        if (_sceneList == null)
        {
            CLog.Warn($"{_sceneList} 인스펙터 연결 필요");
        }

        Color color = _fadeImage.color;
        color.a = 0f;
        _fadeImage.color = color;
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

        try
        {
            if (scene.UseFade)
            {
                Color color = scene.FadeColor;
                color.a = _fadeImage.color.a;
                _fadeImage.color = color;

                CreateBackGround(scene.BackGroundPrefab);
                SetBackGroundActive(true);

                await FadeAsync(1f, scene.FadeOutDuration);
            }

            ChangeScene(scene);

            await UniTask.NextFrame();

            if (scene.UseFade)
            {
                await FadeAsync(0f, scene.FadeInDuration);

                SetBackGroundActive(false);
            }
        }
        finally
        {
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

