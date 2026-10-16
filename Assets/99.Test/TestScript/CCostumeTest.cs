using UnityEngine;

public class CCostumeTest : MonoBehaviour
{
    #region Inspector

    [Header("Manager")]
    [SerializeField] private CCostumeManager _costumeManager;
    [SerializeField] private CPlayerCostume _playerCostume;

    [Header("Costume")]
    [SerializeField] private CCostumeDataSO _defaultCostume;
    [SerializeField] private CCostumeDataSO _armorCostume;

    #endregion

    private void OnEnable()
    {
        if (_costumeManager != null)
        {
            _costumeManager.OnCostumeChanged += OnCostumeChanged;
        }
    }

    private void Start()
    {
        if (_costumeManager != null && _playerCostume != null)
        {
            _playerCostume.ApplyCostume(_costumeManager.CurrentCostume);
        }
    }

    private void OnDisable()
    {
        if (_costumeManager != null)
        {
            _costumeManager.OnCostumeChanged -= OnCostumeChanged;
        }
    }

    // 버튼에서 호출
    public void ChangeCostume()
    {
        if (_costumeManager == null || _defaultCostume == null || _armorCostume == null)
        {
            return;
        }

        // 현재 코스튬에 따라 변경
        CCostumeDataSO nextCostume = _costumeManager.CurrentCostume == _armorCostume ? _defaultCostume : _armorCostume;

        _costumeManager.SelectCostume(nextCostume);
    }

    private void OnCostumeChanged(CCostumeDataSO costume)
    {
        if (_playerCostume == null)
        {
            return;
        }

        _playerCostume.ApplyCostume(costume);

        CLog.Log($"코스튬 변경 : {costume.CostumeName}");
    }
}
