using UnityEngine;

public class CFlowData : MonoBehaviour
{
    [SerializeField] private CSettings _defaultSetting = new CSettings();
    [SerializeField] private CSettings _currentSetting = new CSettings();
    [SerializeField] private CFlowGame _defalutFlow = new CFlowGame();
    [SerializeField] private CFlowGame _currentFlow = new CFlowGame();

    public CSettings CurrentSetting => _currentSetting;

    public CFlowGame CurrentFlow => _currentFlow;

    private void Update()
    {
        if(Input.GetKeyUp(KeyCode.Alpha1))
        {
            CSaveData.Save(_currentSetting, "currentFlow");
        }

        if (Input.GetKeyUp(KeyCode.Alpha2))
        {
            CSaveData.Load(_currentSetting, "currentFlow");
        }

    }


}