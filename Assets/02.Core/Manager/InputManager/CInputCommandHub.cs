using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class CInputCommandHub : MonoBehaviour
{
    #region 인스펙터
    [SerializeField] private CInputManager _inputManager;
    #endregion

    #region 내부변수
    private readonly Dictionary<EKeyAction, IInputCommand> _commands = new Dictionary<EKeyAction, IInputCommand>();
    private readonly Dictionary<EKeyAction, IInputCommand<Vector2>> _vectorCommands = new Dictionary<EKeyAction, IInputCommand<Vector2>>();
    private readonly Dictionary<EKeyAction, IInputCommand<float>> _floatCommands = new Dictionary<EKeyAction, IInputCommand<float>>();

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

        _inputManager.OnMove += Move;
        _inputManager.OnDash += Dash;
        _inputManager.OnAttack += Attack;
        _inputManager.OnSpecialAttack += SpecialAttack;
        _inputManager.OnRapidCasting1 += RapidCasting1;
        _inputManager.OnRapidCasting2 += RapidCasting2;
        _inputManager.OnInteraction += Interaction;
        _inputManager.OnUsePotion += UsePotion;
        _inputManager.OnOpenMap += OpenMap;
        _inputManager.OnInventory += Inventory;
        _inputManager.OnPlayerStats += PlayerStats;
        _inputManager.OnTalent += Talent;
        _inputManager.OnPresetSetting += PresetSetting;
        _inputManager.OnChatting += Chatting;
        _inputManager.OnReloading += Reloading;
        _inputManager.OnPing += Ping;
        _inputManager.OnChoosePotion += ChoosePotion;
        _inputManager.OnBefore += Before;
        _inputManager.OnNext += Next;
        _inputManager.OnESC += ESC;

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
            _inputManager.OnMove -= Move;
            _inputManager.OnDash -= Dash;
            _inputManager.OnAttack -= Attack;
            _inputManager.OnSpecialAttack -= SpecialAttack;
            _inputManager.OnRapidCasting1 -= RapidCasting1;
            _inputManager.OnRapidCasting2 -= RapidCasting2;
            _inputManager.OnInteraction -= Interaction;
            _inputManager.OnUsePotion -= UsePotion;
            _inputManager.OnOpenMap -= OpenMap;
            _inputManager.OnInventory -= Inventory;
            _inputManager.OnPlayerStats -= PlayerStats;
            _inputManager.OnTalent -= Talent;
            _inputManager.OnPresetSetting -= PresetSetting;
            _inputManager.OnChatting -= Chatting;
            _inputManager.OnReloading -= Reloading;
            _inputManager.OnPing -= Ping;
            _inputManager.OnChoosePotion -= ChoosePotion;
            _inputManager.OnBefore -= Before;
            _inputManager.OnNext -= Next;
            _inputManager.OnESC -= ESC;
        }

        _subscribed = false;
    }
    #endregion

    #region 입력 처리
    private void Move(Vector2 direction)
    {
        Execute(EKeyAction.Move, direction);
    }

    private void Dash(float value)
    {
        Execute(EKeyAction.Dash, value);
    }

    private void Attack(float value)
    {
        Execute(EKeyAction.Attack, value);
    }

    private void SpecialAttack(float value)
    {
        Execute(EKeyAction.SpecialAttack,value);
    }

    private void RapidCasting1()
    {
        Execute(EKeyAction.RapidCasting1);
    }

    private void RapidCasting2()
    {
        Execute(EKeyAction.RapidCasting2);
    }

    private void Interaction()
    {
        Execute(EKeyAction.Interaction);
    }

    private void UsePotion()
    {
        Execute(EKeyAction.UsePotion);
    }

    private void OpenMap()
    {
        Execute(EKeyAction.OpenMap);
    }

    private void Inventory()
    {
        Execute(EKeyAction.Inventory);
    }

    private void PlayerStats()
    {
        Execute(EKeyAction.PlayerStats);
    }

    private void Talent()
    {
        Execute(EKeyAction.Talent);
    }

    private void PresetSetting()
    {
        Execute(EKeyAction.PresetSetting);
    }

    private void Chatting()
    {
        Execute(EKeyAction.Chatting);
    }

    private void Reloading()
    {
        Execute(EKeyAction.Reloading);
    }

    private void Ping()
    {
        Execute(EKeyAction.Ping);
    }

    private void ChoosePotion()
    {
        Execute(EKeyAction.ChoosePotion);
    }

    private void Before()
    {
        Execute(EKeyAction.Before);
    }

    private void Next()
    {
        Execute(EKeyAction.Next);
    }

    private void ESC()
    {
        Execute(EKeyAction.ESC);
    }
    #endregion


    #region 커맨드 등록
    public void Register(EKeyAction key, IInputCommand command)
    {
        if (command == null)
        {
            return;
        }

        Unregister(key);
        _commands[key] = command;
    }

    public void Register(EKeyAction key, IInputCommand<Vector2> command)
    {
        if (command == null)
        {
            return;
        }

        Unregister(key);
        _vectorCommands[key] = command;
    }

    public void Register(EKeyAction key, IInputCommand<float> command)
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
    public void Unregister(EKeyAction key)
    {
        _commands.Remove(key);
        _vectorCommands.Remove(key);
        _floatCommands.Remove(key);
    }

    // 자신이 등록한 커맨드와 같을 때만 해제
    public void Unregister(EKeyAction key, IInputCommand command)
    {
        if (_commands.TryGetValue(key, out var registered)
            && ReferenceEquals(registered, command))
        {
            _commands.Remove(key);
        }
    }

    public void Unregister(EKeyAction key, IInputCommand<Vector2> command)
    {
        if (_vectorCommands.TryGetValue(key, out var registered)
            && ReferenceEquals(registered, command))
        {
            _vectorCommands.Remove(key);
        }
    }

    public void Unregister(EKeyAction key, IInputCommand<float> command)
    {
        if (_floatCommands.TryGetValue(key, out var registered)
            && ReferenceEquals(registered, command))
        {
            _floatCommands.Remove(key);
        }
    }
    #endregion


    #region 커맨드 실행
    public void Execute(EKeyAction key)
    {
        if (_commands.TryGetValue(key, out var command))
        {
            command.Execute();
        }
    }

    public void Execute(EKeyAction key, Vector2 value)
    {
        if (_vectorCommands.TryGetValue(key, out var command))
        {
            command.Execute(value);
        }
    }

    public void Execute(EKeyAction key, float value)
    {
        if (_floatCommands.TryGetValue(key, out var command))
        {
            command.Execute(value);
        }
    }
    #endregion



}