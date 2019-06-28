using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// by Kevin.Zhang, 1/23/2017
/// a log tool with filtering
/// </summary>
public class LogMask
{
    public const int NONE = 0;
    public const int VERBOSE = 1;
    public const int WARNING = 2;
    public const int ERROR = 4;
    public const int ALL = 7;
}

public class LogUtil
{
    public enum LogType
    {
        Verbose,
        Warning,
        Error,
    }

    private static HashSet<string> _mTagFilters = new HashSet<string>();
    private static int _mLogMask = LogMask.VERBOSE | LogMask.WARNING | LogMask.ERROR;

    public static void Log(string tag, string format, params object[] args)
    {
        if ((_mLogMask & LogMask.VERBOSE) > 0)
        {
            LogWithTag(LogType.Verbose, tag, format, args);
        }
    }

    public static void LogNoTag(string format, params object[] args)
    {
        Log(null, format, args);
    }

    public static void LogWarning(string tag, string format, params object[] args)
    {
        if ((_mLogMask & LogMask.WARNING) > 0)
        {
            LogWithTag(LogType.Warning, tag, format, args);
        }
    }

    public static void LogWarningNoTag(string format, params object[] args)
    {
        LogWarning(null, format, args);
    }

    public static void LogError(string tag, string format, params object[] args)
    {
        if ((_mLogMask & LogMask.ERROR) > 0)
        {
            LogWithTag(LogType.Error, tag, format, args);
        }
    }

    public static void LogErrorNoTag(string format, params object[] args)
    {
        LogError(null, format, args);
    }

    private static void LogWithTag(LogType type, string tag, string format, params object[] args)
    {
        string logMessage = null;
        if (string.IsNullOrEmpty(tag))
        {
            logMessage = string.Format(format, args);
        }
        else if (!_mTagFilters.Contains(tag))
        {
            logMessage = string.Format("###" + tag + "### " + format, args);
        }

        if (!string.IsNullOrEmpty(logMessage))
        {
            if (type == LogType.Verbose)
                Debug.Log(logMessage);
            else if (type == LogType.Warning)
                Debug.LogWarning(logMessage);
            else if (type == LogType.Error)
                Debug.LogError(logMessage);
        }
    }

    //log with tag that in tags will not be printed
    //log will be printed according to mask
    public static void SetFilters(int mask, string[] tags)
    {
        _mLogMask = mask;

        _mTagFilters.Clear();
        if (tags != null)
        {
            for (int i = 0; i < tags.Length; ++i)
            {
                _mTagFilters.Add(tags[i]);
            }
        }
    }
}

