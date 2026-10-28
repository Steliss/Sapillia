using System;



// 값이 필요 없는 커맨드
public interface IInputCommand
{
    void Execute();
}


// 값이 필요한 커맨드
public interface IInputCommand<T>
{
    void Execute(T value);
}

// ESC
public interface IESCCommand
{
    void ExecuteESC();
}


// 값이 필요없는 행동
public class CActionCommand : IInputCommand
{
    private readonly Action _action;

    public CActionCommand(Action action)
    {
        _action = action;
    }

    public void Execute()
    {
        _action?.Invoke();
    }
}


// 값이 필요한 행동
public class CValueCommand<T> : IInputCommand<T>
{
    private readonly Action<T> _action;

    public CValueCommand(Action<T> action)
    {
        _action = action;
    }

    public void Execute(T value)
    {
        _action?.Invoke(value);
    }
}