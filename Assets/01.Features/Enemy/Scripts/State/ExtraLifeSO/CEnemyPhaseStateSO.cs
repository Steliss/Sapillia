using UnityEngine;
using UnityEngine.UI;


[CreateAssetMenu(menuName = "SO/Enemy/PhaseState/Boss01", fileName = "Enemy_ExtraLife_Boss01")]
public class CEnemyPhaseStateSO : ScriptableObject, IEnemyState
{
    private CStateMashine _mashine;
    private Image _extraLiveUI;

    public void Init(CStateMashine mashine)
    {
        _mashine = mashine;
        _extraLiveUI = _mashine.GetComponentInChildren<Image>();
    }

    public void StateEnter()
    {
        if (_mashine.ExtraLife > 0)
        {
            // 2페이즈 진입 패턴



            // 목숨 깎기 -> UI 적용
        }
        else
        {
            // 사망하기 연출
        }
    }

    public void StateExit()
    {
        
    }

    public void StateUpdate()
    {
        
    }

}
