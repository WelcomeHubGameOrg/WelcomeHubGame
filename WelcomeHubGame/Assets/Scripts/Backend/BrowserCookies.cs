using System.Runtime.InteropServices;
using UnityEngine;
    public static class BrowserCookies
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void BrowserCookies_Set(string key, string value, int days, string path);

        [DllImport("__Internal")]
        private static extern string BrowserCookies_Get(string key);
#endif

        /// <summary>
        /// Writes a value to a browser cookie.
        /// expirationDays > 0: expires in the given number of days | 0: session cookie (cleared when the browser closes)
        /// </summary>
        public static void Set(string key, string value, int expirationDays = 30, string path = "/")
        {
            if (string.IsNullOrEmpty(key)) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            BrowserCookies_Set(key, value ?? string.Empty, expirationDays, path);
#else
            // Fallback for testing inside the Unity Editor
            PlayerPrefs.SetString($"CookieFallback_{key}", value ?? string.Empty);
            PlayerPrefs.Save();
#endif
        }

        /// <summary>
        /// Reads a value from a browser cookie. Returns defaultValue if the cookie does not exist.
        /// </summary>
        public static string Get(string key, string defaultValue = "")
        {
            if (string.IsNullOrEmpty(key)) return defaultValue;

#if UNITY_WEBGL && !UNITY_EDITOR
            string value = BrowserCookies_Get(key);
            return string.IsNullOrEmpty(value) ? defaultValue : value;
#else
            return PlayerPrefs.GetString($"CookieFallback_{key}", defaultValue);
#endif
        }

        /// <summary>
        /// Checks whether a cookie with the specified key exists and is not empty.
        /// </summary>
        public static bool Has(string key)
        {
            return !string.IsNullOrEmpty(Get(key));
        }

        /// <summary>
        /// Deletes an existing browser cookie.
        /// </summary>
        public static void Delete(string key, string path = "/")
        {
            if (string.IsNullOrEmpty(key)) return;

#if UNITY_WEBGL && !UNITY_EDITOR
            BrowserCookies_Set(key, string.Empty, -1, path);
#else
            PlayerPrefs.DeleteKey($"CookieFallback_{key}");
            PlayerPrefs.Save();
#endif
        }
    }
