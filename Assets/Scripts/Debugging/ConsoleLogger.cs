using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConsoleLogger
{
    public static void LogError(string source, string errorMessage)
    {
        Debug.LogError($"Source: {source}\n{errorMessage}");
    }

    public static void LogWarning(string source, string warningMessage)
    {
        Debug.LogWarning($"Source: {source}\n{warningMessage}");
    }

    public static void LogMessage(string source, string message)
    {
        Debug.Log($"Source: {source}\n{message}");
    }
}
