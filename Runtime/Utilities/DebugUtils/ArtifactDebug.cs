using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Utilities.DebugUtils
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
        /// <exception cref="ArgumentException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
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
                    throw new ArgumentException("Wrong Debug Level type");
            }
        }

        /// <summary>
        /// Wrapped debug log function.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <param name="logLevel">The <see cref="DebugLogLevel"/> of log.</param>
        /// <exception cref="ArgumentException">Occur when <see cref="DebugLogLevel"/> wrong.</exception>
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
                    throw new ArgumentException("Wrong Debug Level type");
            }
        }

        /// <summary>
        /// Wrapped debug logerror function.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        public static void LogError(object message)
            => Log(message, DebugLogLevel.Error);

        /// <summary>
        /// Wrapped debug logwarning function.
        /// <br/>
        /// It will be automatically disabled in the built release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        public static void LogWarning(object message)
            => Log(message, DebugLogLevel.Warning);

        #endregion
    }
}