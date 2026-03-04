using System;
using System.Diagnostics;
using Debug = UnityEngine.Debug;

namespace Utilities.DebugUtils
{
    /// <summary>
    /// Enum for switching debug level to set color and log level.
    /// </summary>
    public enum DebugLevel
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
        /// Wrapped debug function. Does not work in the official release version.
        /// </summary>
        /// <param name="message">The message want to be logged. Same as using UnityEngine.Debug.Log().</param>
        /// <param name="level">The <see cref="DebugLevel"/> of log.</param>
        /// <exception cref="ArgumentException">Occur when <see cref="DebugLevel"/> wrong.</exception>
        [Conditional("UNITY_EDITOR")]
        public static void Log(object message, DebugLevel level = DebugLevel.Info)
        {
            switch (level)
            {
                case DebugLevel.Fatal:
                    Debug.LogError($"{ArtifactHead}<color=#DC143C>{message}</color>");
                    break;
                case DebugLevel.Error:
                    Debug.LogError($"{ArtifactHead}<color=#FF4500>{message}</color>");
                    break;
                case DebugLevel.Warning:
                    Debug.LogWarning($"{ArtifactHead}<color=#FFA500>{message}</color>");
                    break;
                case DebugLevel.Info:
                    Debug.Log($"{ArtifactHead}<color=#6495ED>{message}</color>");
                    break;
                case DebugLevel.Hint:
                    Debug.Log($"{ArtifactHead}<color=#48D1CC>{message}</color>");
                    break;
                case DebugLevel.WorksWell:
                    Debug.Log($"{ArtifactHead}<color=#32CD32>{message}</color>");
                    break;
                default:
                    throw new ArgumentException("Wrong Debug Level type");
            }
        }

        #endregion
    }
}