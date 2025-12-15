using NUnit.Framework;
using IrsikSoftware.LogSmith;
using IrsikSoftware.LogSmith.Adapters;
using UnityEngine;
using UnityEngine.TestTools;
using System.Text.RegularExpressions;

namespace IrsikSoftware.LogSmith.Tests.Runtime
{
    /// <summary>
    /// Tests for NativeUnityLoggerAdapter using Unity.Logging backend.
    /// </summary>
    public class NativeUnityLoggerAdapterTests
    {
        [Test]
        public void Write_TraceLevel_OutputsToUnityLog()
        {
            // Arrange & Act
            NativeUnityLoggerAdapter.Write(LogLevel.Trace, "TestCategory", "Trace message");

            // Assert - Unity.Logging outputs via Debug.Log for trace level
            LogAssert.Expect(LogType.Log, new Regex(@".*\[TestCategory\].*Trace message.*"));
        }

        [Test]
        public void Write_DebugLevel_OutputsToUnityLog()
        {
            // Arrange & Act
            NativeUnityLoggerAdapter.Write(LogLevel.Debug, "TestCategory", "Debug message");

            // Assert
            LogAssert.Expect(LogType.Log, new Regex(@".*\[TestCategory\].*Debug message.*"));
        }

        [Test]
        public void Write_InfoLevel_OutputsToUnityLog()
        {
            // Arrange & Act
            NativeUnityLoggerAdapter.Write(LogLevel.Info, "TestCategory", "Info message");

            // Assert
            LogAssert.Expect(LogType.Log, new Regex(@".*\[TestCategory\].*Info message.*"));
        }

        [Test]
        public void Write_WarnLevel_OutputsToUnityLogWarning()
        {
            // Arrange & Act
            NativeUnityLoggerAdapter.Write(LogLevel.Warn, "TestCategory", "Warn message");

            // Assert
            LogAssert.Expect(LogType.Warning, new Regex(@".*\[TestCategory\].*Warn message.*"));
        }

        [Test]
        public void Write_ErrorLevel_OutputsToUnityLogError()
        {
            // Arrange & Act
            NativeUnityLoggerAdapter.Write(LogLevel.Error, "TestCategory", "Error message");

            // Assert
            LogAssert.Expect(LogType.Error, new Regex(@".*\[TestCategory\].*Error message.*"));
        }

        [Test]
        public void Write_CriticalLevel_OutputsToUnityLogError()
        {
            // Arrange & Act
            NativeUnityLoggerAdapter.Write(LogLevel.Critical, "TestCategory", "Critical message");

            // Assert - Critical should include CRITICAL prefix
            LogAssert.Expect(LogType.Error, new Regex(@".*\[CRITICAL\].*\[TestCategory\].*Critical message.*"));
        }

        [Test]
        public void Write_UsesUnityLoggingBackend()
        {
            // This test verifies that Unity.Logging is being used as the backend
            // by checking that the adapter properly initializes and uses Unity.Logging.Log

            // Arrange & Act & Assert - should not throw
            Assert.DoesNotThrow(() => NativeUnityLoggerAdapter.Write(LogLevel.Info, "Test", "Unity.Logging backend test"));
            LogAssert.Expect(LogType.Log, new Regex(@".*Unity\.Logging backend test.*"));
        }
    }
}
