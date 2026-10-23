using UnityEngine;

public class CFlowData : MonoBehaviour
{
    [SerializeField] private CSettings _defaultSetting = new CSettings();
    [SerializeField] private CSettings _currentSetting = new CSettings();
    [SerializeField] private CFlowGame _defalutFlow = new CFlowGame();
    [SerializeField] private CFlowGame _currentFlow = new CFlowGame();

    public CSettings CurrentSetting => _currentSetting;
    public CSettings DefaultSetting => _defaultSetting;

    public CFlowGame CurrentFlow => _currentFlow;
    public CFlowGame DefalutFlow => _defalutFlow;
}