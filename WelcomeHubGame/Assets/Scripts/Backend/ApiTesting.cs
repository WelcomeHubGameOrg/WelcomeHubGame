using UnityEngine;

public class ApiTester : MonoBehaviour
{
    private async void Start()
    {
        Debug.Log("Testing Backend-Connector...");

        string testUsername = "TestPlayer_" + Random.Range(100, 999);
        Debug.Log($"Registriere Test-User: {testUsername}...");
        
        Player player = await MinigameApiClient.RegisterGuestAsync(testUsername);

        if (!string.IsNullOrEmpty(player.getGUID()))
        {
            Debug.Log($"✅ Success! Got GUID: {player.getGUID()}");

            Debug.Log("Sending Test-Score...");
            System.DateTime startTime = System.DateTime.UtcNow.AddMinutes(-2); // 2 minutes
            System.DateTime endTime = System.DateTime.UtcNow;

            bool success = await MinigameApiClient.SubmitScoreAsync(player.getGUID(), 1337, 1, startTime, endTime);
            if (success)
            {
                Debug.Log("✅ Score successfully saved!");
            }

            // 3. Test: Highscores abrufen
            Debug.Log("Loading Highscores...");
            string highscores = await MinigameApiClient.GetHighscoresAsync();
            Debug.Log($"✅ Highscores received:\n{highscores}");


            Debug.Log("Getting top 10 highscores");
            string dailyScores = await MinigameApiClient.GetHighscoresAsync(minigameId: 1, limit: 10, filter: "daily");
            Debug.Log($"✅ Highscores received:\n{dailyScores}");
            
            Debug.Log("Getting all users");
            string foundUsers = await MinigameApiClient.GetUsersAsync(limit: 20, filter: "Manu");
            Debug.Log($"✅ Users received:\n{foundUsers}");            

            Debug.Log("Getting all minigames");
            string allMinigames = await MinigameApiClient.GetMinigamesAsync(filter: "adsf");
            Debug.Log($"✅ Minigames received:\n{allMinigames}");            

        }
        else
        {
            Debug.LogError("❌ Registration failed!");
        }
    }
}