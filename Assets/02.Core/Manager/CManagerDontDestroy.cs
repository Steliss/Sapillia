using UnityEngine;
using System.Threading;
using Cysharp.Threading.Tasks;

public class CManagerDontDestroy : MonoBehaviour
{
    [SerializeField] private CSceneManager _sceneManager;
    [SerializeField] private CInputCommandHub _inputCommandHub;
    [SerializeField] private CDataManger _dataManger;


    public CSceneManager SceneManager => _sceneManager;
    public CInputCommandHub InputCommandHub => _inputCommandHub;
    public CDataManger DataManger => _dataManger;
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

        Instance = this;

        // 씬이 변경되어도 유지
        DontDestroyOnLoad(gameObject);

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


}
