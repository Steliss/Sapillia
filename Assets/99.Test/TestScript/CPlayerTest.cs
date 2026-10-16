using UnityEngine;


public class CPlayerTest : MonoBehaviour
{
    [SerializeField] private CPlayerGamingDataSO _data;

    [ContextMenu("DEBUG / Test Player Data")]
    private void TestPlayerData()
    {
        if (!Application.isPlaying)
        {
            CLog.Warn("Play Mode에서 테스트 하세요.");
            return;
        }

        // �⺻ �ڿ� �Һ�
        _data.TakeDamage(20);
        _data.TryUseMP(30);
        _data.TryConsumeDash();

        // �ɷ�ġ ����
        SPlayerStats stats = _data.Stats;

        stats.Movement_Speed += 1f;
        stats.Attack_Speed += 0.2f;
        stats.Critical_Rate += 5f;

        _data.SetStats(stats);

        // �Ӽ� ���ݷ� ����
        _data.AddAttributePower(EAttribute.Fire, 10);

        // ���� �� ��ȭ
        _data.AddEXP(150);
        _data.AddMoney(200);

        // ���� ���� ���
        CLog.Log(
            $"Level: {_data.Level}\n" +
            $"HP: {_data.CurrentHP}/{_data.Stats.MaxHP}\n" +
            $"MP: {_data.CurrentMP}/{_data.Stats.MaxMP}\n" +
            $"Dash: {_data.CurrentDashCount}/{_data.Stats.DashCount}\n" +
            $"Movement: {_data.Stats.Movement_Speed}\n" +
            $"Fire: {_data.GetAttributePower(EAttribute.Fire)}\n" +
            $"Money: {_data.Money}"
        );

        // ��ü �ɷ�ġ�� JSON ���·� ���
        CLog.Log(JsonUtility.ToJson(_data.Stats, true));
    }

    [ContextMenu("DEBUG / Reset Player Data")]
    private void ResetPlayerData()
    {
        if (!Application.isPlaying)
            return;

        _data.Initalize();
        CLog.Log("Player Data Reset");
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
