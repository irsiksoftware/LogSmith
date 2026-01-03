using System;
using System.IO;
using System.Linq;
using System.Text;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// File sink implementation for writing logs to disk with rotation support.
    /// </summary>
    public class FileSink : ILogSink, IDisposable
    {
        private readonly string _filePath;
        private readonly object _lock = new object();
        private readonly FileRotationService _rotationService;
        private readonly IMessageTemplateEngine _templateEngine;

        private StreamWriter _writer;
        private bool _disposed;
        private MessageFormat _currentFormat = MessageFormat.Text;

        public string Name => "File";

        /// <summary>
        /// Gets or sets the current message format (Text or JSON).
        /// </summary>
        public MessageFormat CurrentFormat
        {
            get
            {
                lock (_lock)
                {
                    return _currentFormat;
                }
            }
            set
            {
                lock (_lock)
                {
                    _currentFormat = value;
                }
            }
        }

        public FileSink(string filePath, IMessageTemplateEngine templateEngine = null,
            bool enableRotation = true, int maxFileSizeMB = 10, int retentionCount = 5)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentNullException(nameof(filePath));

            _filePath = filePath;
            _templateEngine = templateEngine ?? new MessageTemplateEngine();
            _rotationService = enableRotation
                ? new FileRotationService(maxFileSizeMB * 1024L * 1024L, retentionCount)
                : null;

            InitializeWriter();
        }

        public void Write(LogMessage message)
        {
            if (_disposed)
                return;

            lock (_lock)
            {
                try
                {
                    // Check if rotation is needed before writing
                    if (_rotationService != null && _rotationService.ShouldRotate(_filePath))
                    {
                        // Close current writer before rotation
                        _writer?.Flush();
                        _writer?.Dispose();
                        _writer = null;

                        // Perform rotation (move file and cleanup)
                        _rotationService.RotateFile(_filePath);

                        // Reinitialize writer with fresh file
                        InitializeWriter();
                    }

                    if (_writer == null)
                    {
                        InitializeWriter();
                    }

                    // Format message using template engine
                    var formattedMessage = _templateEngine.Format(message, _currentFormat);
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
            if (_disposed)
                return;

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
            if (_disposed)
                return;

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
