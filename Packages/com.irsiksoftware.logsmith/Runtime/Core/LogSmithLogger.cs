using System;
using IrsikSoftware.LogSmith.Adapters;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Primary logger implementation routing through Unity's native logging system.
    /// </summary>
    internal class LogSmithLogger : ILog
    {
        private readonly ILogRouter _router;
        private readonly string _category;

        public LogSmithLogger(ILogRouter router, string category = "Default")
        {
            _router = router ?? throw new ArgumentNullException(nameof(router));
            _category = category;
        }

        public void Trace(string message) => Log(LogLevel.Trace, message);
        public void Debug(string message) => Log(LogLevel.Debug, message);
        public void Info(string message) => Log(LogLevel.Info, message);
        public void Warn(string message) => Log(LogLevel.Warn, message);
        public void Error(string message) => Log(LogLevel.Error, message);
        public void Critical(string message) => Log(LogLevel.Critical, message);

        public ILog WithCategory(string category)
        {
            return new LogSmithLogger(_router, category);
        }

        private void Log(LogLevel level, string message)
        {
            var logMessage = new LogMessage
            {
                Level = level,
                Category = _category,
                Message = message,
                Timestamp = DateTime.UtcNow,
                Frame = UnityEngine.Time.frameCount,
                ThreadId = System.Threading.Thread.CurrentThread.ManagedThreadId,
                ThreadName = System.Threading.Thread.CurrentThread.Name ?? string.Empty
            };

            _router.Route(logMessage);
        }
    }
}