using System;
using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// 뉴 인풋 매니저의 입력만 받아와서 이벤트로 전달
/// </summary>
public class CInputManager : MonoBehaviour
{

    [SerializeField] private InputActionReference _moveAction;
    [SerializeField] private InputActionReference _spaceAction;
    [SerializeField] private InputActionReference _mouseRAction;
    [SerializeField] private InputActionReference _mouseLAction;
    [SerializeField] private InputActionReference _EAction;
    [SerializeField] private InputActionReference _FAction;
    [SerializeField] private InputActionReference _RAction;

    public event Action<Vector2> OnMove;
    public event Action OnMouseR;
    public event Action OnMouseL;
    public event Action OnSpace;
    public event Action OnE;
    public event Action OnF;
    public event Action OnR;



    private void OnEnable()
    {
        // 바인드 연결을 수업처럼 Awake 에서 처리할지여부
        // 해당 스크립트는 매니저 객체에 붙어서 싱글톤 처리를 일일히 해줄 필요는 없음 

        _moveAction.action.performed += MoveInput;
        _moveAction.action.canceled += MoveInputCancled;

        _spaceAction.action.performed += SpaceInput;
        _mouseRAction.action.performed += MouseRInput;
        _mouseLAction.action.performed += MouseLInput;
        _EAction.action.performed += KeyboardEInput;
        _FAction.action.performed += KeyboardFInput;
        _RAction.action.performed += KeyboardRInput;

        _moveAction.action.Enable();
        _spaceAction.action.Enable();
        _mouseRAction.action.Enable();
        _mouseLAction.action.Enable();
        _EAction.action.Enable();
        _FAction.action.Enable();
        _RAction.action.Enable();
    }

    private void OnDisable()
    {
        _moveAction.action.performed -= MoveInput;
        _moveAction.action.canceled -= MoveInputCancled;

        _spaceAction.action.performed -= SpaceInput;
        _mouseRAction.action.performed -= MouseRInput;
        _mouseLAction.action.performed -= MouseLInput;
        _EAction.action.performed -= KeyboardEInput;
        _FAction.action.performed -= KeyboardFInput;
        _RAction.action.performed -= KeyboardRInput;

        _moveAction.action.Disable();
        _spaceAction.action.Disable();
        _mouseRAction.action.Enable();
        _mouseLAction.action.Enable();
        _EAction.action.Enable();
        _FAction.action.Enable();
        _RAction.action.Enable();
    }

    private void MoveInput(InputAction.CallbackContext ctx)
    {
        Vector2 direction = ctx.ReadValue<Vector2>();

        OnMove?.Invoke(direction);
    }

    private void MoveInputCancled(InputAction.CallbackContext ctx)
    {
        Vector2 direction = ctx.ReadValue<Vector2>();

        OnMove?.Invoke(Vector2.zero);
    }

    private void SpaceInput(InputAction.CallbackContext ctx)
    {
        OnSpace?.Invoke();
    }

    private void MouseRInput(InputAction.CallbackContext ctx)
    {
        OnMouseR?.Invoke();
    }

    private void MouseLInput(InputAction.CallbackContext ctx)
    {
        OnMouseL?.Invoke();
    }

    private void KeyboardFInput(InputAction.CallbackContext ctx)
    {
        OnF?.Invoke();
    }

    private void KeyboardEInput(InputAction.CallbackContext ctx)
    {
        OnE?.Invoke();
    }

    private void KeyboardRInput(InputAction.CallbackContext ctx)
    {
        OnR?.Invoke();
    }
}