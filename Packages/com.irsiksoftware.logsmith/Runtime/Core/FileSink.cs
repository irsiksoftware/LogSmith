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
        private readonly bool _enableRotation;
        private readonly long _maxFileSizeBytes;
        private readonly int _retentionCount;
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
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException(nameof(filePath));

            _filePath = filePath;
            _templateEngine = templateEngine ?? new MessageTemplateEngine();
            _enableRotation = enableRotation;
            _maxFileSizeBytes = maxFileSizeMB * 1024L * 1024L; // Convert MB to bytes
            _retentionCount = retentionCount;

            InitializeWriter();
        }

        public void Write(LogMessage message)
        {
            if (_disposed) return;

            lock (_lock)
            {
                try
                {
                    // Check if rotation is needed before writing
                    if (_enableRotation && ShouldRotate())
                    {
                        RotateLogFile();
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

        private bool ShouldRotate()
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    return false;
                }

                var fileInfo = new FileInfo(_filePath);
                return fileInfo.Length >= _maxFileSizeBytes;
            }
            catch
            {
                return false;
            }
        }

        private void RotateLogFile()
        {
            try
            {
                // Close current writer
                _writer?.Flush();
                _writer?.Dispose();
                _writer = null;

                // Generate archived file name with timestamp (including milliseconds to avoid collisions)
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
                var directory = Path.GetDirectoryName(_filePath);
                var fileName = Path.GetFileNameWithoutExtension(_filePath);
                var extension = Path.GetExtension(_filePath);
                var archivedFileName = $"{fileName}_{timestamp}{extension}";
                var archivedFilePath = Path.Combine(directory, archivedFileName);

                // If archived file already exists (unlikely with milliseconds, but handle it)
                int counter = 1;
                while (File.Exists(archivedFilePath))
                {
                    archivedFileName = $"{fileName}_{timestamp}_{counter}{extension}";
                    archivedFilePath = Path.Combine(directory, archivedFileName);
                    counter++;
                }

                // Move current log to archived file
                if (File.Exists(_filePath))
                {
                    File.Move(_filePath, archivedFilePath);
                }

                // Clean up old archived files based on retention policy
                if (_retentionCount > 0)
                {
                    CleanupOldLogFiles(directory, fileName, extension);
                }

                // Reinitialize writer with fresh file
                InitializeWriter();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[LogSmith] FileSink rotation failed: {ex.Message}");
            }
        }

        private void CleanupOldLogFiles(string directory, string baseFileName, string extension)
        {
            try
            {
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    return;
                }

                // Find all archived log files matching the pattern
                var searchPattern = $"{baseFileName}_*{extension}";
                var archivedFiles = Directory.GetFiles(directory, searchPattern)
                    .Select(f => new FileInfo(f))
                    .OrderByDescending(f => f.LastWriteTime)
                    .ToList();

                // Delete files beyond retention count
                for (int i = _retentionCount; i < archivedFiles.Count; i++)
                {
                    try
                    {
                        archivedFiles[i].Delete();
                    }
                    catch (Exception ex)
                    {
                        UnityEngine.Debug.LogWarning($"[LogSmith] Failed to delete old log file {archivedFiles[i].Name}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[LogSmith] Cleanup of old log files failed: {ex.Message}");
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