using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

// ReSharper disable CheckNamespace

namespace Artifact.UnityUtils.Utilities.DebugUtils
{
    /// <summary>
    /// Enum for switching debug level to set color and log level.
    /// </summary>
    public enum DebugLogLevel
    {
        Fatal,
        Error,
        Warning,
        Info,
        Hint,
        WorksWell,
    }

    /// <summary>
    /// Debug wrapper of Unity Debug. With package header and debug-level-based color.
    /// <br/>
    /// It will be automatically disabled in the built release version.
    /// </summary>
    public static class ArtifactDebug
    {
        #region Fields

        /// <summary>
        /// String header to indicate that log is come from Artifact Unity Utils.
        /// </summary>
        private const string ArtifactHead = "<color=purple>Artifact Unity Utils</color>: ";

        #endregion

        #region Static Methods

        /// <summary>
        /// Wrapped debug log function for package internal usage.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <param name="logLevel">The <see cref="DebugLogLevel"/> of log.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
        [Conditional("UNITY_EDITOR")]
        public static void PackageLog(object message, DebugLogLevel logLevel = DebugLogLevel.Info)
        {
            switch (logLevel)
            {
                case DebugLogLevel.Fatal:
                    Debug.LogError($"{ArtifactHead}<color=#DC143C>{message}</color>");
                    break;
                case DebugLogLevel.Error:
                    Debug.LogError($"{ArtifactHead}<color=#FF4500>{message}</color>");
                    break;
                case DebugLogLevel.Warning:
                    Debug.LogWarning($"{ArtifactHead}<color=#FFA500>{message}</color>");
                    break;
                case DebugLogLevel.Info:
                    Debug.Log($"{ArtifactHead}<color=#6495ED>{message}</color>");
                    break;
                case DebugLogLevel.Hint:
                    Debug.Log($"{ArtifactHead}<color=#48D1CC>{message}</color>");
                    break;
                case DebugLogLevel.WorksWell:
                    Debug.Log($"{ArtifactHead}<color=#32CD32>{message}</color>");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel,
                        ToLogString("Wrong Debug Level type"));
            }
        }

        /// <summary>
        /// Wrapped debug log function.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <param name="logLevel">The <see cref="DebugLogLevel"/> of log.</param>
        /// <exception cref="ArgumentOutOfRangeException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, DebugLogLevel logLevel = DebugLogLevel.Info)
        {
            switch (logLevel)
            {
                case DebugLogLevel.Fatal:
                    Debug.LogError($"<color=#DC143C>{message}</color>");
                    break;
                case DebugLogLevel.Error:
                    Debug.LogError($"<color=#FF4500>{message}</color>");
                    break;
                case DebugLogLevel.Warning:
                    Debug.LogWarning($"<color=#FFA500>{message}</color>");
                    break;
                case DebugLogLevel.Info:
                    Debug.Log($"<color=#6495ED>{message}</color>");
                    break;
                case DebugLogLevel.Hint:
                    Debug.Log($"<color=#48D1CC>{message}</color>");
                    break;
                case DebugLogLevel.WorksWell:
                    Debug.Log($"<color=#32CD32>{message}</color>");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel,
                        ToLogString("Wrong Debug Level type"));
            }
        }

        /// <summary>
        /// Wrapped debug logerror function.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <exception cref="ArgumentOutOfRangeException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
        [Conditional("UNITY_EDITOR")]
        public static void LogError(object message)
            => Log(message, DebugLogLevel.Error);

        /// <summary>
        /// Wrapped debug logwarning function.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <exception cref="ArgumentOutOfRangeException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
        [Conditional("UNITY_EDITOR")]
        public static void LogWarning(object message)
            => Log(message, DebugLogLevel.Warning);

        /// <summary>
        /// Make one message to ArtifactLof type string. Used for throw exception or others.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <param name="logLevel">The <see cref="DebugLogLevel"/> of log.</param>
        /// <returns>The string after process.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
        public static string ToLogString(object message, DebugLogLevel logLevel = DebugLogLevel.Info)
            => $"{ArtifactHead}" + logLevel switch
            {
                DebugLogLevel.Fatal => $"<color=#DC143C>{message}</color>",
                DebugLogLevel.Error => $"<color=#FF4500>{message}</color>",
                DebugLogLevel.Warning => $"<color=#FFA500>{message}</color>",
                DebugLogLevel.Info => $"<color=#6495ED>{message}</color>",
                DebugLogLevel.Hint => $"<color=#48D1CC>{message}</color>",
                DebugLogLevel.WorksWell => $"<color=#32CD32>{message}</color>",
                _ => throw new ArgumentOutOfRangeException(nameof(logLevel), logLevel,
                    $"{ArtifactHead}<color=#DC143C>Wrong Debug Level type</color>")
            };

        #endregion
    }
}