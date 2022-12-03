using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Scripts.Helpers.Attributes;
using UnityEngine;

namespace Scripts.Helpers
{
    /// <summary>
    /// Replacement for calls to Debug from Unity. This Logger makes middle class between Unity Debug and our needs.
    /// Logger works without being in the scene, but with default values, so its better to place it into persisting scene
    /// to be able to set the settings to project needs.
    /// Benefits of this Logger are:
    /// 1) for release, only setting the Logger to ELogSeverity.Release causes, that no logs are being logged into Android,
    /// unless they are explicitly set to be logged on release.
    /// 2) Simple message is translated into format <project name>:[<calling class>] <calling method>: <message> where
    /// project name and calling class are configurable.
    ///
    /// Example:
    /// class Test : MonoBehaviour
    /// {
    ///     private Start()
    ///     {
    ///         Logger.Log("Some message");
    ///     }
    /// }
    ///
    /// Results in logged message>> sa-oculus: [Test.cs] Start: Some message
    /// </summary>
    public class Logger : MonoBehaviour
    {
        public enum ELogSeverity
        {
            Debug = 1,
            Release = 2,
            None = 100,
        }

        public enum EClassDescriptionLevel
        {
            None = 0,
            WholePath = 1,
            JustClassName = 2,
        }

        [SerializeField] private bool logProjectName;

        [ShowWhen(nameof(logProjectName), true)] 
        [SerializeField]
        private string projectName;

        [SerializeField] private ELogSeverity severityLevel = ELogSeverity.Debug;
        [SerializeField] private EClassDescriptionLevel classDescriptionLevel = EClassDescriptionLevel.WholePath;

        private static ELogSeverity _severityLevel = ELogSeverity.Debug;
        private static EClassDescriptionLevel _classDescriptionLevel = EClassDescriptionLevel.WholePath;
        private static bool _logProjectName;
        private static string _projectName;
        private static readonly ELogSeverity _defaultSeverity;

        static Logger()
        {
#if DEBUG || DEBUG_BUILD || DEVELOPMENT_BUILD
            _defaultSeverity = ELogSeverity.Debug;
#else
        _defaultSeverity = ELogSeverity.Release;
#endif
        }

        public static void Log(string message, ELogSeverity logSeverity = ELogSeverity.Debug, UnityEngine.Object logObject = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "")
        {
            if (logSeverity >= _defaultSeverity)
            {
                Debug.Log(ResolveMessage(callerFilePath, callerMemberName, message), logObject);
            }
        }

        public static void LogWarning(string message, ELogSeverity logSeverity = ELogSeverity.Debug, UnityEngine.Object logObject = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "")
        {
            if (logSeverity >= _defaultSeverity)
            {
                Debug.LogWarning(ResolveMessage(callerFilePath, callerMemberName, message), logObject);
            }
        }

        public static void LogError(string message, ELogSeverity logSeverity = ELogSeverity.Debug, UnityEngine.Object logObject = null,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "")
        {
            if (logSeverity >= _defaultSeverity)
            {
                Debug.LogError(ResolveMessage(callerFilePath, callerMemberName, message), logObject);
            }
        }

        private void OnValidate()
        {
            _severityLevel = (ELogSeverity)Mathf.Max((int)severityLevel, (int)_defaultSeverity);
            _classDescriptionLevel = classDescriptionLevel;
            _logProjectName = logProjectName;
            _projectName = projectName;
        }

        private static string ResolveMessage(string callerFilePath, string callerMemberName, string message)
        {
            string projectName = _logProjectName && !string.IsNullOrEmpty(_projectName)
                ? $"{_projectName}: "
                : "";

            return $"{projectName}[{ResolveClassName(callerFilePath)}] {callerMemberName}: {message}";
        }

        private static string ResolveClassName(string callerFilePath) => _classDescriptionLevel switch
        {
            EClassDescriptionLevel.None => "",
            EClassDescriptionLevel.WholePath => callerFilePath,
            EClassDescriptionLevel.JustClassName => callerFilePath.Split('\\').Last(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}