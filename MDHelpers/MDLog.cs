using System;
using System.Collections.Generic;
using System.Diagnostics;
using static Godot.StringExtensions;
using Directory = Godot.Directory;
using Error = Godot.Error;
using File = Godot.File;

public enum MDLogLevel
{
    Trace,
    Debug,
    Info,
    Warn,
    Error,
    Fatal,
    Log // Always logs
}

public struct MDLogProperties
{
    // Minimum LogLevel required to write this log to file
    public MDLogLevel FileLogLevel;

    // Minimum LogLevel required to write this log to the console
    public MDLogLevel ConsoleLogLevel;
}

/*
 * MDLog
 *
 * Provides methods of logging different categories at different levels.
 * If no log properties are added for a category, logs for that category will show in console and be written to file.
 */
public static class MDLog
{
    private const string LOG_DIR = "user://logs/";
    private const string EXT_LOG = ".log";
    private const string LOG_CAT = "Log";

    // Initialize internal data and set the directory to store log files
    public static void Initialize()
    {
        LogProperties = new Dictionary<string, MDLogProperties>();
        InitLogFile();
    }

    // Logs the message (supports formatting) in accordance with the LogProperties set for the specified log category
    public static void Log(string CategoryName, MDLogLevel LogLevel, string Message, params object[] args)
    {
        bool LogFile = true;
        bool LogConsole = true;
        if (LogProperties.ContainsKey(CategoryName))
        {
            MDLogProperties LogProps = LogProperties[CategoryName];
            LogFile = LogProps.FileLogLevel <= LogLevel;
            LogConsole = LogProps.ConsoleLogLevel <= LogLevel;
        }

        if (LogFile || LogConsole)
        {
            string FullMessage = "[" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "] "
                + "[" + CategoryName + "::" + LogLevel.ToString() + "] "
                + string.Format(Message, args);

            if (LogFile)
            {
                LogToFile(FullMessage);
            }

            if (LogConsole)
            {
                LogToConsole(FullMessage);
            }
        }

        if (LogLevel == MDLogLevel.Fatal)
        {
            DebugBreak();
        }
    }

    // Sames os Log() expect it only logs if Condition == true
    public static void CLog(bool Condition, string CategoryName, MDLogLevel LogLevel, string Message, params object[] args)
    {
        if (Condition)
        {
            Log(CategoryName, LogLevel, Message, args);
        }
    }

    // Adds log category properties to be referenced when making logs of that type
    public static void AddLogCategoryProperties(string CategoryName, ref MDLogProperties LogProps)
    {
        LogProperties[CategoryName] = LogProps;
    }

    // Prints the message to the console
    private static void LogToConsole(string Message)
    {
        Console.WriteLine(Message);
    }

    // Writes the message to the log file
    private static void LogToFile(string Message)
    {
        OpenLogFile();
        LogFile.StoreLine(Message);
        LogFile.Close();
    }

    // Creates our time-stamped log file and opens it for writing
    private static void InitLogFile()
    {
        FullLogFilePath = LOG_DIR + DateTime.Now.ToString("yyyy.MM.dd-HH.mm.ss") + EXT_LOG;
        if (CreateLogDirectoryIfNotExists(LOG_DIR))
        {
            LogFile = new File();
            LogFile.Open(FullLogFilePath, (int)File.ModeFlags.Write);
            Log(LOG_CAT, MDLogLevel.Info, "Created log file {0}", FullLogFilePath);
            LogFile.Close();
        }
        else
        {
            Console.WriteLine("Failed to create log directory.");
        }
    }

    // Opens the log file for writing
    private static void OpenLogFile()
    {
        LogFile.Open(FullLogFilePath, (int)File.ModeFlags.ReadWrite);
        LogFile.SeekEnd();
    }

    // Ensures the directory for log files exists
    private static bool CreateLogDirectoryIfNotExists(string FileDir)
    {
        Directory dir = new Directory();
        if (!dir.DirExists(FileDir))
        {
            return dir.MakeDirRecursive(FileDir) == Error.Ok;
        }

        return true;
    }

    // Breaks the debugger
    [Conditional("DEBUG")]
    private static void DebugBreak()
    {
        if(Debugger.IsAttached)
        {
            Debugger.Break();
        }
    }

    private static string FullLogFilePath;
    private static File LogFile;
    private static Dictionary<string, MDLogProperties> LogProperties;
}