using UnityEngine;

public static class CLog 
{
    public static bool _isEnable { get; set; } = true;

    public static void Log(string msg)
    {
        if (!_isEnable)
        {
            return;
        }

        Debug.Log(msg);
    }

    public static void Warn(string msg)
    {
        if (!_isEnable)
        {
            return;
        }

        Debug.LogWarning(msg);
    }

    public static void Error(string msg)
    {
        Debug.LogError(msg);
    }

}
