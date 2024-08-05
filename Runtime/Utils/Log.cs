using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor.VersionControl;
using UnityEngine;


public static class Log
{
    public static void Write(string msg)
    {
        var logFilePath = Path.Combine(Application.persistentDataPath, "gameLog.txt");
        File.AppendAllText(logFilePath, $"{System.DateTime.Now}: {msg}\n");
    }
}

