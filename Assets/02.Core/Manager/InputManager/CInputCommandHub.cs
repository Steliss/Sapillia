using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


// 입력 종류
public enum EInput
{
    Move,
    Space,
    MouseR,
    MouseL,
    F,
    E,
    R
}

public class CInputCommandHub : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private CInputManager _inputManager;
    #endregion

    #region 내부변수
    private readonly Dictionary<EInput, IInputCommand> _commands = new Dictionary<EInput, IInputCommand>();
    private readonly Dictionary<EInput, IInputCommand<Vector2>> _vectorCommands = new Dictionary<EInput, IInputCommand<Vector2>>();
    private readonly Dictionary<EInput, IInputCommand<float>> _floatCommands = new Dictionary<EInput, IInputCommand<float>>();

    private bool _subscribed;
    #endregion


    private void OnEnable()
    {
        BindInputEvent();
    }

    private void OnDisable()
    {
        UnbindInputEvent();
    }


    #region 입력 구독
    private void BindInputEvent()
    {
        if (_subscribed || _inputManager == null)
        {
            return;
        }

        _inputManager.OnMove += HandleMove;
        _inputManager.OnSpace += HandleSpace;
        _inputManager.OnMouseR += HandleMouseR;
        _inputManager.OnMouseL += HandleMouseL;
        _inputManager.OnE += HandleOnE;
        _inputManager.OnF += HandleOnF;
        _inputManager.OnR += HandleOnR;

        _subscribed = true;
    }

    private void UnbindInputEvent()
    {
        if (!_subscribed)
        {
            return;
        }

        if (_inputManager != null)
        {
            _inputManager.OnMove -= HandleMove;
            _inputManager.OnSpace -= HandleSpace;
            _inputManager.OnMouseR -= HandleMouseR;
            _inputManager.OnMouseL -= HandleMouseL;
            _inputManager.OnE -= HandleOnE;
            _inputManager.OnF -= HandleOnF;
            _inputManager.OnR -= HandleOnR;
        }

        _subscribed = false;
    }
    #endregion


    #region 입력 처리
    private void HandleMove(Vector2 direction)
    {
        Execute(EInput.Move, direction);
    }

    // ※
    private void HandleSpace(float value)
    {
        Execute(EInput.Space, value);
    }
    /*
    private void HandleSpace()
    {
        Execute(EInput.Space);
    }
    */
    private void HandleMouseR()
    {
        Execute(EInput.MouseR);
    }
    private void HandleMouseL()
    {
        Execute(EInput.MouseL);
    }
    private void HandleOnE()
    {
        Execute(EInput.E);
    }
    private void HandleOnF()
    {
        Execute(EInput.F);
    }
    private void HandleOnR()
    {
        Execute(EInput.R);
    }
    #endregion


    #region 커맨드 등록
    public void Register(EInput key, IInputCommand command)
    {
        if (command == null)
        {
            return;
        }

        Unregister(key);
        _commands[key] = command;
    }

    public void Register(EInput key, IInputCommand<Vector2> command)
    {
        if (command == null)
        {
            return;
        }

        Unregister(key);
        _vectorCommands[key] = command;
    }

    public void Register(EInput key, IInputCommand<float> command)
    {
        if (command == null)
        {
            return;
        }

        Unregister(key);
        _floatCommands[key] = command;
    }
    #endregion


    #region 커맨드 해제
    public void Unregister(EInput key)
    {
        _commands.Remove(key);
        _vectorCommands.Remove(key);
        _floatCommands.Remove(key);
    }

    // 자신이 등록한 커맨드와 같을 때만 해제
    public void Unregister(EInput key, IInputCommand command)
    {
        if (_commands.TryGetValue(key, out var registered)
            && ReferenceEquals(registered, command))
        {
            _commands.Remove(key);
        }
    }

    public void Unregister(EInput key, IInputCommand<Vector2> command)
    {
        if (_vectorCommands.TryGetValue(key, out var registered)
            && ReferenceEquals(registered, command))
        {
            _vectorCommands.Remove(key);
        }
    }

    public void Unregister(EInput key, IInputCommand<float> command)
    {
        if (_floatCommands.TryGetValue(key, out var registered)
            && ReferenceEquals(registered, command))
        {
            _floatCommands.Remove(key);
        }
    }
    #endregion


    #region 커맨드 실행
    public void Execute(EInput key)
    {
        if (_commands.TryGetValue(key, out var command))
        {
            command.Execute();
        }
    }

    public void Execute(EInput key, Vector2 value)
    {
        if (_vectorCommands.TryGetValue(key, out var command))
        {
            command.Execute(value);
        }
    }

    public void Execute(EInput key, float value)
    {
        if (_floatCommands.TryGetValue(key, out var command))
        {
            command.Execute(value);
        }
    }
    #endregion



}