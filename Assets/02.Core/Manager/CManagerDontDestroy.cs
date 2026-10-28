using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CManagerDontDestroy : MonoBehaviour
{
    [SerializeField] private CSceneManager _sceneManager;
    [SerializeField] private CInputCommandHub _inputCommandHub;
    [SerializeField] private CDataManger _dataManger;
    [SerializeField] private CLocalizationManager _localizationData;
    [SerializeField] private bool _isLog = true;

    public CSceneManager SceneManager => _sceneManager;
    public CInputCommandHub InputCommandHub => _inputCommandHub;
    public CDataManger DataManger => _dataManger;
    public CLocalizationManager LocalizationData => _localizationData;
    public static CManagerDontDestroy Instance { get; private set; }

    private void Awake()
    {
        // 중복 매니저 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (_sceneManager == null)
        {
            CLog.Log("_sceneManager : Null");
        }

        if (_dataManger == null)
        {
            CLog.Log("_dataManger : Null");
        }

        if (_inputCommandHub == null)
        {
            CLog.Log("_inputCommandHub : Null");
        }

        if (_localizationData == null)
        {
            CLog.Log("_localizationData : Null");
        }
        Instance = this;

        // 씬이 변경되어도 유지
        DontDestroyOnLoad(gameObject);

    }

    // Inspector에서 값을 바꾸면 반영
    private void OnValidate()
    {
        ApplyLogSetting();
    }
    private void ApplyLogSetting()
    {
        CLog._isEnable = _isLog;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // 객체없이 호출을 위해 static
    /// <summary>
    /// 인풋매니저 동기 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async UniTask<CSceneManager> SceneManagerAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => Instance != null && Instance.SceneManager != null, cancellationToken: token);

        token.ThrowIfCancellationRequested();

        CLog.Log("SceneManager 생성완료");
        return Instance.SceneManager;
    }

    /// <summary>
    /// 씬매니저 동기 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async UniTask<CInputCommandHub> InputCommandHubAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => Instance != null && Instance.InputCommandHub != null, cancellationToken: token);

        token.ThrowIfCancellationRequested();

        CLog.Log("InputCommandHub 생성완료");
        return Instance.InputCommandHub;
    }

    /// <summary>
    /// 데이터매니저 동기 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async UniTask<CDataManger> DataMangerAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => Instance != null && Instance.DataManger != null, cancellationToken: token);

        token.ThrowIfCancellationRequested();

        CLog.Log("DataManger 생성완료");
        return Instance.DataManger;
    }

    /// <summary>
    /// 번역 매니저 동기 함수
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public static async UniTask<CLocalizationManager> LocalizationDataAsync(CancellationToken token)
    {
        await UniTask.WaitUntil(() => Instance != null && Instance.LocalizationData != null, cancellationToken: token);

        token.ThrowIfCancellationRequested();

        CLog.Log("DataManger 생성완료");
        return Instance.LocalizationData;
    }

}
