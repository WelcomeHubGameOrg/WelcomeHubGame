using System;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public static class MinigameApiClient
{
    private const string BaseUrl = "https://backend-krdv.onrender.com/api";
    private const string ApiKey = "upJvJBeIHvhJ5zknVgKzaV4Qzzry1byl";
    private const string GameSecret = "MetropoliaWelcomingGameSuperSecret2026";

    // /api/users/guest
    public static async Task<Player> RegisterGuestAsync(string username)
    {
        string jsonBody = JsonUtility.ToJson(new GuestRequest { username = username });
        var request = CreatePostRequest($"{BaseUrl}/users/guest", jsonBody);

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {

            var response = JsonUtility.FromJson<GuestResponse>(request.downloadHandler.text);
            Player newPlayer = new Player(response.guid,username);
            return newPlayer;
        }
        else
        {
            Debug.LogError($"Error during registration: {request.error}");
            return null;
        }
    }
    // /api/scores
    public static async Task<bool> SubmitScoreAsync(string guid, int score, int minigame_id, DateTime? startedAt = null, DateTime? endedAt = null)
    {
    string rawData = guid + score.ToString() + GameSecret;
    string hash = ComputeSha256Hash(rawData);

    string startedAtIso = startedAt.HasValue ? startedAt.Value.ToUniversalTime().ToString("o") : null;
    string endedAtIso = endedAt.HasValue ? endedAt.Value.ToUniversalTime().ToString("o") : null;

    var scoreData = new ScoreRequest 
    { 
        user_id = guid, 
        minigame_id = minigame_id, 
        score = score, 
        hash = hash,
        started_at = startedAtIso,
        ended_at = endedAtIso
    };
    
    string jsonBody = JsonUtility.ToJson(scoreData);

    var request = CreatePostRequest($"{BaseUrl}/scores", jsonBody);
    await request.SendWebRequest();

    if (request.result == UnityWebRequest.Result.Success)
    {
        Debug.Log("Score saved successfully!");
        return true;
    }
    else
    {
        Debug.LogError($"Score creation failed: {request.downloadHandler.text}");
        return false;
    }
}

    public static async Task<string> GetHighscoresAsync(int? minigameId = null, int? limit = null, string filter = null)
    {
        var queryParams = new System.Collections.Generic.List<string>();
        if (minigameId.HasValue) queryParams.Add($"minigame_id={minigameId.Value}");
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (!string.IsNullOrEmpty(filter)) queryParams.Add($"filter={UnityWebRequest.EscapeURL(filter)}");

        string url = $"{BaseUrl}/scores";
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("x-api-key", ApiKey);
            var operation = request.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Fehler beim Laden der Highscores: {request.error} - {request.downloadHandler.text}");
                return null;
            }
        }
    }

    public static async Task<string> GetMinigamesAsync(string filter = null)
    {
        string url = $"{BaseUrl}/minigames";
        if (!string.IsNullOrEmpty(filter))
        {
            url += $"?filter={UnityWebRequest.EscapeURL(filter)}";
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("x-api-key", ApiKey);
            var operation = request.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Fehler beim Laden der Minigames: {request.error}");
                return null;
            }
        }
    }

    public static async Task<string> GetUsersAsync(int? limit = null, string filter = null)
    {
        var queryParams = new System.Collections.Generic.List<string>();
        if (limit.HasValue) queryParams.Add($"limit={limit.Value}");
        if (!string.IsNullOrEmpty(filter)) queryParams.Add($"filter={UnityWebRequest.EscapeURL(filter)}");

        string url = $"{BaseUrl}/users";
        if (queryParams.Count > 0)
        {
            url += "?" + string.Join("&", queryParams);
        }

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.SetRequestHeader("x-api-key", ApiKey);
            var operation = request.SendWebRequest();

            while (!operation.isDone) await Task.Yield();

            if (request.result == UnityWebRequest.Result.Success)
            {
                return request.downloadHandler.text;
            }
            else
            {
                Debug.LogError($"Fehler beim Laden der User: {request.error}");
                return null;
            }
        }
    }

    private static UnityWebRequest CreatePostRequest(string url, string jsonBody)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-api-key", ApiKey);
        return request;
    }

    private static string ComputeSha256Hash(string rawData)
    {
        using (SHA256 sha256Hash = SHA256.Create())
        {
            byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            StringBuilder builder = new StringBuilder();
            foreach (byte b in bytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }

    [Serializable] private class GuestRequest { public string username; }
    [Serializable] private class GuestResponse { public string guid; public string username; }
    [Serializable] private class ScoreRequest { public string user_id; public int score; public string hash; public int minigame_id; public string started_at; public string ended_at;}
}