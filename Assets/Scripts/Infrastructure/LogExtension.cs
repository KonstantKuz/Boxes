using System;
using UnityEngine;

namespace Infrastructure
{
    public static class LogExtension
    {
        public static void Log(this object initiator, LogType type, string message, Exception exception = null)
        {
            string initiatorName = initiator.GetType().Name;

            switch (type)
            {
                case LogType.Error:
                    Debug.LogError($"[{initiatorName}] {message}");
                    break;
                case LogType.Assert:
                    Debug.LogAssertion($"[{initiatorName}] {message}");
                    break;
                case LogType.Warning:
                    Debug.LogWarning($"[{initiatorName}] {message}");
                    break;
                case LogType.Log:
                    Debug.Log($"[{initiatorName}] {message}");
                    break;
                case LogType.Exception:
                    Debug.LogException(exception);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}
