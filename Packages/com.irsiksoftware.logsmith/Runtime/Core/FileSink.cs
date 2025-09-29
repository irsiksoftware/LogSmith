using System;
using System.IO;
using System.Text;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// File sink implementation for writing logs to disk.
    /// </summary>
    public class FileSink : ILogSink, IDisposable
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private StreamWriter _writer;
        private bool _disposed;

        public string Name => "File";

        public FileSink(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            _filePath = filePath;
            InitializeWriter();
        }

        public void Write(LogMessage message)
        {
            if (_disposed) return;

            lock (_lock)
            {
                try
                {
                    if (_writer == null)
                    {
                        InitializeWriter();
                    }

                    var formattedMessage = $"[{message.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{message.Level}] [{message.Category}] {message.Message}";
                    _writer?.WriteLine(formattedMessage);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[LogSmith] FileSink write failed: {ex.Message}");
                }
            }
        }

        public void Flush()
        {
            if (_disposed) return;

            lock (_lock)
            {
                try
                {
                    _writer?.Flush();
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[LogSmith] FileSink flush failed: {ex.Message}");
                }
            }
        }

        public void Dispose()
        {
            if (_disposed) return;

            lock (_lock)
            {
                _disposed = true;

                try
                {
                    _writer?.Flush();
                    _writer?.Dispose();
                    _writer = null;
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[LogSmith] FileSink dispose failed: {ex.Message}");
                }
            }
        }

        private void InitializeWriter()
        {
            try
            {
                // Ensure directory exists
                var directory = Path.GetDirectoryName(_filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // Open file in append mode with UTF8 encoding
                _writer = new StreamWriter(_filePath, append: true, encoding: Encoding.UTF8)
                {
                    AutoFlush = false // Manual flush for better performance
                };
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[LogSmith] FileSink initialization failed: {ex.Message}");
                _writer = null;
            }
        }
    }
}