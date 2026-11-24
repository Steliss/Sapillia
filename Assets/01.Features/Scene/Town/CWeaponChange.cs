using UnityEngine;

public class CWeaponChange : CTriggerPlayer, IInputCommand
{
    [SerializeField] private CWeaponManager _weapon;
    [SerializeField] private EWeaponType _type;

    private CInputCommandHub _input;
    private CDataManger _data;

    private CUniTaskOnes _inputTask = new CUniTaskOnes();
    private CUniTaskOnes _dataTask = new CUniTaskOnes();

    private bool _isNear;

    private void OnEnable()
    {
        _inputTask.Run(async token =>
        {
            _input = await CManagerDontDestroy.InputCommandHubAsync(token);
            _input.Register(EKeyAction.Interaction, this);

            CLog.Log("CWeaponChange _inputTask OnEnable 작업 완료");
        });

        _dataTask.Run(async token =>
        {
            _data = await CManagerDontDestroy.DataMangerAsync(token);
            CLog.Log("CWeaponChange _localizationTask OnEnable 작업 완료");
        });
    }

    private void OnDisable()
    {
        _inputTask.Cancel();

        _input?.Unregister(EKeyAction.Interaction, this);
    }

    protected override void OnPlayerEnter()
    {
        _isNear = true;
    }

    protected override void OnPlayerExit()
    {
        _isNear = false;
    }

    public void Execute()
    {
        if (!_isNear)
        {
            return;
        }

        if(_type == EWeaponType.SwordShield)
        {
            _weapon.TestEquipSwordShield();
        }
        else if(_type == EWeaponType.Crossbow)
        {
            _weapon.TestEquipCrossbow();
        }
        else
        {
            CLog.Warn("무기 없음");
        }

    }
}
