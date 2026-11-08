using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class CKeyBind : MonoBehaviour
{

    [Header("참조")]
    [SerializeField] private CDataManger _data;
    [SerializeField] private InputActionAsset _inputActions;

    [Header("변경할 동작")]
    [SerializeField] private EKeyAction _keyAction;
    [SerializeField] private string _actionName;

    // 변경을 실행할 액션
    private InputAction _action;
    private InputActionRebindingExtensions.RebindingOperation _operation;
    private CKeyBindingData _binding;

    public event Action OnRebindEnded;

    private bool _isEnable;

    public bool IsRebinding => _operation != null;

    private void Start()
    {
        ApplySavedBindind();
    }

    // SO에 저장된 기본 키와 보조 키를 실제 입력에 적용
    public void ApplySavedBindind()
    {
        foreach (EKeyAction keyAction in System.Enum.GetValues(typeof(EKeyAction)))
        {
            // 인풋 매니저에서 쓰는 값, ESC는 고정값
            if (keyAction == EKeyAction.Move || keyAction == EKeyAction.ESC)
            {
                continue;
            }

            CKeyBindingData binding = _data.CurrentSetting.GetKeyBinding(keyAction);

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
        _binding = _data.CurrentSetting.GetKeyBinding(_keyAction);

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

        // 조건설정
        _operation = _action.PerformInteractiveRebinding(bindingIndex)
            .WithExpectedControlType("Button")
            .WithControlsExcluding("<Mouse>/position")
            .WithControlsExcluding("<Mouse>/delta")
            .WithControlsExcluding("<Mouse>/scroll")
            .WithCancelingThrough("<Keyboard>/escape")

        // 입력 후 실행함수 등록 
        .OnComplete(operation =>
        {
            string keyPath = _action.bindings[bindingIndex].effectivePath;

            ClearDuplicateBindind(slot, keyPath);

            _data.CurrentSetting.SetKeyBinding(_keyAction, slot, keyPath);

            FinishRebind();
            _data.SaveSettings();
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

        OnRebindEnded?.Invoke();
    }

    public void CancelRebind()
    {
        if (_operation != null)
        {
            _operation.Cancel();
        }
    }

    // 모든 동작의 기본키 / 보조키에서 중복 확인
    private void ClearDuplicateBindind(EKeySlot changedSlot, string keyPath)
    {
        foreach (EKeyAction keyAction in Enum.GetValues(typeof(EKeyAction)))
        {
            // Move ESC키 제외
            if (keyAction == EKeyAction.Move || keyAction == EKeyAction.ESC)
            {
                continue;
            }

            CKeyBindingData binding = _data.CurrentSetting.GetKeyBinding(keyAction);
            InputAction action = _inputActions.FindAction(binding._actionName, throwIfNotFound: true);

            ClearDuplicateBind(
                keyAction,
                EKeySlot.Primary,
                action,
                binding._primaryIndex,
                changedSlot,
                keyPath);

            ClearDuplicateBind(
                keyAction,
                EKeySlot.Secondary,
                action,
                binding._secondaryIndex,
                changedSlot,
                keyPath);
        }
    }

    // 한 바인딩을 검사하고 중복이면 해제
    private void ClearDuplicateBind(
        EKeyAction keyAction,
        EKeySlot keySlot,
        InputAction action,
        int bindingIndex,
        EKeySlot changedSlot,
        string keyPath)
    {
        // 방금 변경한 바인딩 자신은 유지
        if (keyAction == _keyAction && keySlot == changedSlot)
        {
            return;
        }

        string registeredPath = action.bindings[bindingIndex].effectivePath;

        if (!string.Equals(
            registeredPath,
            keyPath,
            StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        // 실제 입력 해제
        action.ApplyBindingOverride(bindingIndex, "");

        // 저장할 설정 데이터도 해제
        _data.CurrentSetting.SetKeyBinding(keyAction, keySlot, "");
    }

}
