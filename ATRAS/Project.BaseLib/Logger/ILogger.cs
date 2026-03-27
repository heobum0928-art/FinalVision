using System;
using System.Runtime.CompilerServices;

namespace Project.BaseLib.Logger
{
    public interface IFormatLogger
    {
        void Debug(string message, params object[] values);
        void Info(string message, params object[] values);
        void Warn(string message, params object[] values);
        void Error(string message, params object[] values);
    }

    public interface ILogger
    {
        ILogger CopyLogger(string newName);
        bool AddAppender(string appenderName);

        bool IsDebugEnabled { get; }
        bool IsInfoEnabled { get; }
        bool IsWarnEnabled { get; }
        bool IsErrorEnabled { get; }

        MessageDelegate Debug(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0);
        MessageDelegate Info(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0);
        MessageDelegate Warn(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0);
        MessageDelegate Error(DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0);

        void Error(Exception ex, string message = "", DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = "", [CallerLineNumber] int lineNumber = 0);

        void Debug(object messageObject, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0);
        void Info(object messageObject, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0);
        void Warn(object messageObject, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0);
        void Error(object messageObject, DateTime? datetime = null, [CallerMemberName] string memberName = null, [CallerFilePath] string filePath = null, [CallerLineNumber] int lineNumber = 0);
    }
}
