using UnityEngine;
using UnityEngine.InputSystem;

public class CKeyBind : MonoBehaviour
{

    [Header("참조")]
    [SerializeField] private CDataManger _flowData;
    [SerializeField] private InputActionAsset _inputActions;

    [Header("변경할 동작")]
    [SerializeField] private EKeyAction _keyAction;
    [SerializeField] private string _actionName;

    // 변경을 실행할 액션
    private InputAction _action;
    private InputActionRebindingExtensions.RebindingOperation _operation;
    private CKeyBindingData _binding;

    private bool _isEnable;

    private void Start()
    {
        // ※ TODO: json 저장 _flowData 불러오기 필요

        ApplySavedBindings();
    }

    // SO에 저장된 기본 키와 보조 키를 실제 입력에 적용
    public void ApplySavedBindings()
    {
        foreach (EKeyAction keyAction in System.Enum.GetValues(typeof(EKeyAction)))
        {
            // 인풋 매니저에서 쓰는 값, ESC는 고정값
            if (keyAction == EKeyAction.Move || keyAction == EKeyAction.ESC)
            {
                continue;
            }

            CKeyBindingData binding = _flowData.CurrentSetting.GetKeyBinding(keyAction);

            InputAction action = _inputActions.FindAction(binding._actionName, throwIfNotFound: true);

            action.ApplyBindingOverride(binding._primaryIndex, binding._primary);
            action.ApplyBindingOverride(binding._secondaryIndex, binding._secondary);
        }
    }

    // 변경할 동작과 기본·보조 여부를 전달받음
    public void RebindKey(EKeyAction keyAction, bool primary = true)
    {
        // 작업 중에는 변경 대상을 덮어쓰지 않음
        if (_operation != null)
        {
            return;
        }

        _keyAction = keyAction;
        _binding = _flowData.CurrentSetting.GetKeyBinding(_keyAction);

        _actionName = _binding._actionName;
        _action = _inputActions.FindAction(_actionName, throwIfNotFound: true);

        if (primary)
        {
            StartRebind(EKeySlot.Primary, _binding._primaryIndex);
        }
        else
        {
            StartRebind(EKeySlot.Secondary, _binding._secondaryIndex);
        }
    }

    // 유니티 뉴 인풋 키 변경 함수 
    private void StartRebind(EKeySlot slot, int bindingIndex)
    {
        if (_operation != null)
        {
            return;
        }

        _isEnable = _action.enabled;
        _action.Disable();

        _operation = _action.PerformInteractiveRebinding(bindingIndex)
            .WithExpectedControlType("Button")
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Mouse>/scroll")
            .WithCancelingThrough("<Keyboard>/escape")
            .OnComplete(operation =>
            {
                string keyPath = _action.bindings[bindingIndex].effectivePath;
            
                _flowData.CurrentSetting.SetKeyBinding(_keyAction, slot, keyPath);
            
                FinishRebind();
            
                // ※ TODO: 변경내용 json 저장, _flowData 저장 필요
            })
            .OnCancel(operation =>
            {
                FinishRebind();
            });

        _operation.Start();

    }

    // 작업 종료후 원상복귀
    private void FinishRebind()
    {
        _operation.Dispose();
        _operation = null;

        if (_isEnable)
        {
            _action.Enable();
        }
    }


    // ※ : 타 코드에서 실행 종료시 방어 코드 필요하면 추가

}
