using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-200)]
public class CPlayerRunSession : MonoBehaviour
{
    #region 인스펙터

    [Header("스테이지 단독 테스트")]
    [SerializeField] private bool _autoStartForTest;
    [SerializeField] private CCostumeDataSO _testCostume;

    #endregion

    #region 프로퍼티

    public CPlayerGamingDataSO PlayerData { get; private set; }

    public CCostumeDataSO RunCostume { get; private set; }

    public bool IsAdventureActive => PlayerData != null;

    #endregion


    private void Awake()
    {
        // 씬 전환 시 데이터 유지
        DontDestroyOnLoad(gameObject);

        // 스테이지 단독 테스트용
        if (_autoStartForTest)
        {
            if (!TryBeginAdventure(_testCostume))
            {
                CLog.Warn("[RunSession] 테스트 초기화 실패");
            }
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void Start()
    {
        // 최초 실행 씬의 플레이어도 연결
        if (PlayerData != null)
        {
            BindPlayersInScene(SceneManager.GetActiveScene());
        }
    }

    private void Update()
    {
        if (PlayerData == null)
        {
            return;
        }

        float deltaTime = Time.deltaTime;

        // MP 자연 회복
        PlayerData.RecoverMPperTime(deltaTime);

        // 대쉬 충전
        PlayerData.ReChargeDashperTime(deltaTime);
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void OnDestroy()
    {
        ClearPlayerData();
    }



    #region 모험 관리

    public bool TryBeginAdventure(CCostumeDataSO costume)
    {
        // 이미 모험 진행 중
        if (PlayerData != null)
        {
            CLog.Warn("[RunSession] 이미 모험이 진행 중입니다.");
            return false;
        }

        if (costume == null)
        {
            CLog.Warn("[RunSession] Costume 데이터 누락");
            return false;
        }

        // 1. 런타임 플레이어 데이터 생성
        CPlayerGamingDataSO data = new CPlayerGamingDataSO();

        // 2. 기본 능력치 초기화
        data.Initalize();

        // 3. 코스튬 추가 능력치 적용
        data.SetCostumeBonus(costume.Bonus);

        // 4. 최대 HP, MP, 대쉬 충전
        data.FillResourceToMax();

        // 5. 런타임 데이터 보관
        PlayerData = data;

        // 6. 외형 적용용 코스튬 참조
        RunCostume = costume;

        CLog.Log("[RunSession] 플레이어 데이터 생성 완료");

        return true;
    }

    public void EndAdventure()
    {
        // 런타임 데이터 제거
        ClearPlayerData();

        // 모험 코스튬 참조 해제
        RunCostume = null;

        CLog.Log("[RunSession] 모험 종료");
    }

    private void ClearPlayerData()
    {
        if (PlayerData == null)
        {
            return;
        }

        PlayerData = null;
    }

    #endregion

    #region 플레이어 연결

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (PlayerData == null)
        {
            return;
        }

        BindPlayersInScene(scene);
    }

    private void BindPlayersInScene(Scene scene)
    {
        if (!scene.IsValid() || !scene.isLoaded)
        {
            return;
        }

        if (PlayerData == null)
        {
            return;
        }

        // 씬에 배치된 모든 PlayerController 검색
        GameObject[] roots = scene.GetRootGameObjects();

        foreach (GameObject root in roots)
        {
            CPlayerController[] controllers = root.GetComponentsInChildren<CPlayerController>(true);

            foreach (CPlayerController controller in controllers)
            {
                BindPlayer(controller);
            }
        }
    }

    public void BindPlayer(CPlayerController controller)
    {
        if (controller == null)
        {
            CLog.Warn("[RunSession] PlayerController 누락");
            return;
        }

        if (PlayerData == null)
        {
            CLog.Warn("[RunSession] PlayerData가 생성되지 않았습니다.");
            return;
        }

        // 중복 연결 방지
        if (ReferenceEquals(controller.PlayerData, PlayerData))
        {
            return;
        }

        // 플레이어에게 데이터 전달
        controller.Bind(PlayerData, RunCostume);

        CLog.Log($"[RunSession] {controller.name} 데이터 연결 완료");
    }

    #endregion
}