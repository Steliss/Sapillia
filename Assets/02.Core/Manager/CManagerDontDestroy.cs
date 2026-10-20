using UnityEngine;

public class CManagerDontDestroy : MonoBehaviour
{
    [SerializeField] private CSceneManager _sceneManager;


    public CSceneManager SceneManager => _sceneManager;
    public static CManagerDontDestroy Instance { get; private set; }

    private void Awake()
    {
        // 중복 매니저 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if(_sceneManager == null)
        {
            CLog.Log("_sceneManager : Null");
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
}
