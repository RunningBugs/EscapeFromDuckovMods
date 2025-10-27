using System;
using System.Collections.Generic;
using UnityEngine;

namespace DailyQuestMod.Helpers
{
    /// <summary>
    /// Helper class for logging that prevents spam
    /// Provides logOnce functionality to only log unique messages once
    /// </summary>
    public static class LogHelper
    {
        private static HashSet<string> loggedMessages = new HashSet<string>();
        private static Dictionary<string, int> logCounts = new Dictionary<string, int>();
        private static Dictionary<string, float> lastLogTime = new Dictionary<string, float>();

        /// <summary>
        /// Log a message only once (first occurrence)
        /// </summary>
        public static void LogOnce(string message, string category = "DailyQuestMod")
        {
            string key = $"{category}:{message}";
            if (!loggedMessages.Contains(key))
            {
                loggedMessages.Add(key);
                Debug.Log($"[{category}] {message}");
            }
        }

        /// <summary>
        /// Log a warning only once (first occurrence)
        /// </summary>
        public static void LogWarningOnce(string message, string category = "DailyQuestMod")
        {
            string key = $"{category}:WARN:{message}";
            if (!loggedMessages.Contains(key))
            {
                loggedMessages.Add(key);
                Debug.LogWarning($"[{category}] {message}");
            }
        }

        /// <summary>
        /// Log an error only once (first occurrence)
        /// </summary>
        public static void LogErrorOnce(string message, string category = "DailyQuestMod")
        {
            string key = $"{category}:ERROR:{message}";
            if (!loggedMessages.Contains(key))
            {
                loggedMessages.Add(key);
                Debug.LogError($"[{category}] {message}");
            }
        }

        /// <summary>
        /// Log a message with throttling (only once per interval in seconds)
        /// </summary>
        public static void LogThrottled(string message, float intervalSeconds = 5f, string category = "DailyQuestMod")
        {
            string key = $"{category}:{message}";
            float now = Time.time;

            if (!lastLogTime.ContainsKey(key) || (now - lastLogTime[key]) >= intervalSeconds)
            {
                lastLogTime[key] = now;
                Debug.Log($"[{category}] {message}");
            }
        }

        /// <summary>
        /// Log with rate limiting - shows first N occurrences, then throttles
        /// </summary>
        public static void LogRateLimited(string message, int maxCount = 3, float throttleInterval = 30f, string category = "DailyQuestMod")
        {
            string key = $"{category}:{message}";

            if (!logCounts.ContainsKey(key))
            {
                logCounts[key] = 0;
            }

            logCounts[key]++;
            int count = logCounts[key];

            // Log first maxCount occurrences
            if (count <= maxCount)
            {
                Debug.Log($"[{category}] {message}");

                // On the maxCount-th occurrence, add a notice
                if (count == maxCount)
                {
                    Debug.Log($"[{category}] (This message will now be throttled - shown once per {throttleInterval}s)");
                }
            }
            // After maxCount, throttle
            else
            {
                LogThrottled($"{message} (x{count})", throttleInterval, category);
            }
        }

        /// <summary>
        /// Log with count - shows how many times this message occurred
        /// Only logs every N occurrences after the first
        /// </summary>
        public static void LogEveryN(string message, int n = 100, string category = "DailyQuestMod")
        {
            string key = $"{category}:{message}";

            if (!logCounts.ContainsKey(key))
            {
                logCounts[key] = 0;
            }

            logCounts[key]++;
            int count = logCounts[key];

            // Always log the first one
            if (count == 1)
            {
                Debug.Log($"[{category}] {message}");
            }
            // Then log every N occurrences
            else if (count % n == 0)
            {
                Debug.Log($"[{category}] {message} (occurred {count} times)");
            }
        }

        /// <summary>
        /// Clear all logged message history (useful for testing or reset)
        /// </summary>
        public static void ClearHistory()
        {
            loggedMessages.Clear();
            logCounts.Clear();
            lastLogTime.Clear();
            Debug.Log("[LogHelper] Message history cleared");
        }

        /// <summary>
        /// Get statistics about logged messages
        /// </summary>
        public static void PrintStats(string category = "DailyQuestMod")
        {
            Debug.Log($"[{category}] LogHelper Stats:");
            Debug.Log($"[{category}]   Unique messages logged once: {loggedMessages.Count}");
            Debug.Log($"[{category}]   Messages with counts: {logCounts.Count}");
            Debug.Log($"[{category}]   Messages with throttle timers: {lastLogTime.Count}");
        }

        /// <summary>
        /// Enable/disable debug mode that logs even throttled messages
        /// </summary>
        public static bool DebugMode { get; set; } = false;

        /// <summary>
        /// Log only if debug mode is enabled
        /// </summary>
        public static void LogDebug(string message, string category = "DailyQuestMod")
        {
            if (DebugMode)
            {
                Debug.Log($"[{category}] [DEBUG] {message}");
            }
        }
    }
}
