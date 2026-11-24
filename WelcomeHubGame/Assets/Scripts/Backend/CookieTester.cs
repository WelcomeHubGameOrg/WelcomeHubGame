using UnityEngine;

public class CookieTester : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Testing BrowserCookies...");

        // 1. Check if a cookie from the last page reload (F5) still exists
        string previousSessionUser = BrowserCookies.Get("persistent_user");
        if (!string.IsNullOrEmpty(previousSessionUser))
        {
            Debug.Log($"✅ Persistence check! Found cookie from previous session: {previousSessionUser}");
        }
        else
        {
            Debug.Log("ℹ️ No cookie from previous session yet (reload page with F5 after this run to test persistence).");
        }

        // 2. Write a new cookie (including special characters & random value)
        string testKey = "test_player";
        string testValue = "Player_ÄÖÜ_" + Random.Range(100, 999);
        Debug.Log($"Setting cookie '{testKey}' = '{testValue}'...");
        BrowserCookies.Set(testKey, testValue, expirationDays: 7);

        // 3. Read the cookie value back
        string loadedValue = BrowserCookies.Get(testKey);
        if (loadedValue == testValue)
        {
            Debug.Log($"✅ Success! Read cookie value: {loadedValue}");
        }
        else
        {
            Debug.LogError($"❌ Failed! Expected '{testValue}', but got '{loadedValue}'");
        }

        // 4. Check existence (Has)
        if (BrowserCookies.Has(testKey))
        {
            Debug.Log($"✅ Has('{testKey}') returned true!");
        }
        else
        {
            Debug.LogError($"❌ Has('{testKey}') returned false!");
        }

        // 5. Test session cookie (expirationDays = 0)
        Debug.Log("Setting session cookie 'session_token'...");
        BrowserCookies.Set("session_token", "tok_" + Random.Range(1000, 9999), expirationDays: 0);
        string sessionToken = BrowserCookies.Get("session_token");
        if (!string.IsNullOrEmpty(sessionToken))
        {
            Debug.Log($"✅ Session cookie read: {sessionToken}");
        }
        else
        {
            Debug.LogError("❌ Session cookie could not be read!");
        }

        // 6. Delete the cookie and verify the fallback value
        Debug.Log($"Deleting cookie '{testKey}'...");
        BrowserCookies.Delete(testKey);
        string afterDelete = BrowserCookies.Get(testKey, "FALLBACK_OK");
        if (!BrowserCookies.Has(testKey) && afterDelete == "FALLBACK_OK")
        {
            Debug.Log("✅ Cookie successfully deleted and default fallback returned!");
        }
        else
        {
            Debug.LogError($"❌ Delete failed! Value is still: {afterDelete}");
        }

        // 7. Save a persistent cookie for the next reload test (F5)
        string nextPersistentUser = "PersistentUser_" + Random.Range(100, 999);
        BrowserCookies.Set("persistent_user", nextPersistentUser, expirationDays: 30);
        Debug.Log($"Saved '{nextPersistentUser}' to 'persistent_user' for next reload test.");
    }
}