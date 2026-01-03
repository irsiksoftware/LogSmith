using System;
using System.IO;
using System.Linq;

namespace IrsikSoftware.LogSmith.Core
{
    /// <summary>
    /// Service responsible for file rotation and retention management.
    /// Provides functionality to check if a log file should be rotated based on size,
    /// archive files with timestamp-based naming, and clean up old archived files.
    /// </summary>
    /// <remarks>
    /// This service handles only the file system operations for rotation and cleanup.
    /// It does not manage file handles or writers - those should be managed by the caller.
    ///
    /// Thread Safety: This class is not thread-safe. Callers must ensure that rotation
    /// operations are synchronized (e.g., within FileSink's lock).
    ///
    /// Typical usage pattern:
    /// 1. Call ShouldRotate() to check if rotation is needed
    /// 2. Close any open file handles to the target file
    /// 3. Call RotateFile() to perform the rotation and cleanup
    /// 4. Reopen file handles as needed
    /// </remarks>
    public class FileRotationService
    {
        private readonly long _maxFileSizeBytes;
        private readonly int _retentionCount;

        /// <summary>
        /// Initializes a new instance of the FileRotationService.
        /// </summary>
        /// <param name="maxFileSizeBytes">Maximum file size in bytes before rotation is triggered.</param>
        /// <param name="retentionCount">Number of archived files to retain. Use 0 to keep all archived files.</param>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when maxFileSizeBytes is less than or equal to 0.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when retentionCount is less than 0.</exception>
        public FileRotationService(long maxFileSizeBytes, int retentionCount)
        {
            if (maxFileSizeBytes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxFileSizeBytes), "Max file size must be greater than 0");

            if (retentionCount < 0)
                throw new ArgumentOutOfRangeException(nameof(retentionCount), "Retention count cannot be negative");

            _maxFileSizeBytes = maxFileSizeBytes;
            _retentionCount = retentionCount;
        }

        /// <summary>
        /// Checks if the specified file should be rotated based on its current size.
        /// </summary>
        /// <param name="filePath">The path to the file to check.</param>
        /// <returns>True if the file exists and exceeds the maximum size threshold; otherwise, false.</returns>
        /// <remarks>
        /// Returns false if the file does not exist or if an error occurs while checking.
        /// This method is safe to call frequently as it only performs a file size check.
        /// </remarks>
        public bool ShouldRotate(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return false;
                }

                var fileInfo = new FileInfo(filePath);
                return fileInfo.Length >= _maxFileSizeBytes;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Rotates the specified log file by archiving it with a timestamp-based name
        /// and cleans up old archived files based on the retention policy.
        /// </summary>
        /// <param name="filePath">The path to the file to rotate.</param>
        /// <remarks>
        /// This method performs the following steps:
        /// 1. Generates an archived file name with timestamp format: {filename}_{YYYYMMDD-HHmmss-fff}{extension}
        /// 2. Moves the current file to the archived location
        /// 3. Deletes archived files beyond the retention count (if retentionCount > 0)
        ///
        /// IMPORTANT: The caller must close any open file handles to the target file before calling this method.
        ///
        /// If the archived file name already exists, a counter suffix is added to ensure uniqueness.
        /// Errors during rotation are logged but do not throw exceptions to prevent disrupting logging operations.
        /// </remarks>
        public void RotateFile(string filePath)
        {
            try
            {
                if (!File.Exists(filePath))
                {
                    return;
                }

                // Generate archived file name with timestamp (including milliseconds to avoid collisions)
                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss-fff");
                var directory = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileNameWithoutExtension(filePath);
                var extension = Path.GetExtension(filePath);
                var archivedFileName = $"{fileName}_{timestamp}{extension}";
                var archivedFilePath = Path.Combine(directory, archivedFileName);

                // If archived file already exists (unlikely with milliseconds, but handle it)
                var counter = 1;
                while (File.Exists(archivedFilePath))
                {
                    archivedFileName = $"{fileName}_{timestamp}_{counter}{extension}";
                    archivedFilePath = Path.Combine(directory, archivedFileName);
                    counter++;
                }

                // Move current log to archived file
                File.Move(filePath, archivedFilePath);

                // Clean up old archived files based on retention policy
                if (_retentionCount > 0)
                {
                    CleanupOldLogFiles(directory, fileName, extension);
                }
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[LogSmith] FileRotationService rotation failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Deletes archived log files that exceed the retention count.
        /// </summary>
        /// <param name="directory">The directory containing the log files.</param>
        /// <param name="baseFileName">The base name of the log file (without extension or timestamp).</param>
        /// <param name="extension">The file extension (e.g., ".log").</param>
        /// <remarks>
        /// This method finds all archived files matching the pattern {baseFileName}_*{extension},
        /// sorts them by last write time (newest first), and deletes files beyond the retention count.
        /// Individual file deletion errors are logged but do not stop the cleanup process.
        /// </remarks>
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
                for (var i = _retentionCount; i < archivedFiles.Count; i++)
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
    }
}
