using System;
using System.Text;
using System.Security.Cryptography;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Provides asynchronous methods to communicate with the Node.js/MariaDB backend API.
/// Handles user registration, score submission (including security hashing), and data retrieval.
/// </summary>
public static class MinigameApiClient
{
    private const string BaseUrl = "https://backend-krdv.onrender.com/api";
    private const string ApiKey = "upJvJBeIHvhJ5zknVgKzaV4Qzzry1byl";
    private const string GameSecret = "MetropoliaWelcomingGameSuperSecret2026";

    /// <summary>
    /// Registers a new guest user on the backend.
    /// </summary>
    /// <param name="username">The display name chosen by the player.</param>
    /// <returns>
    /// A new <see cref="Player"/> object containing the backend-generated GUID and username on success. 
    /// Returns <c>null</c> if the network request or registration fails.
    /// </returns>
    public static async Task<Player> RegisterGuestAsync(string username)
    {
        string jsonBody = JsonUtility.ToJson(new GuestRequest { username = username });
        var request = CreatePostRequest($"{BaseUrl}/users/guest", jsonBody);

        await request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            try
            {
                var response = JsonUtility.FromJson<GuestResponse>(request.downloadHandler.text);
                if (response == null || string.IsNullOrEmpty(response.guid))
                {
                    Debug.LogError("Registration successful, but received empty or invalid JSON from backend.");
                    return null;
                }
                Player newPlayer = new Player(response.guid, username);
                return newPlayer;
            }
            catch(ArgumentException e)
            {
                Debug.LogError($"Failed to parse JSON response: {e.Message}\nRaw text: {request.downloadHandler.text}");
                return null;
            }

        }
        else
        {
            Debug.LogError($"Error during registration: {request.error}");
            return null;
        }
    }

    /// <summary>
    /// Submits a new highscore for a specific player and minigame. 
    /// Automatically generates the required SHA-256 security hash to prevent cheating.
    /// </summary>
    /// <param name="guid">The unique identifier of the player (retrieved from registration).</param>
    /// <param name="score">The achieved score.</param>
    /// <param name="minigame_id">The database ID of the minigame.</param>
    /// <param name="startedAt">Optional timestamp when the game started. Pass <c>null</c> if not tracked.</param>
    /// <param name="endedAt">Optional timestamp when the game ended. Pass <c>null</c> if not tracked.</param>
    /// <returns>Returns <c>true</c> if the score was saved successfully, otherwise <c>false</c>.</returns>
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

    /// <summary>
    /// Fetches highscores from the database with optional filtering and limits.
    /// </summary>
    /// <param name="minigameId">Optional. The specific minigame ID to fetch scores for. Pass <c>null</c> to fetch scores across all games.</param>
    /// <param name="limit">Optional. Maximum number of scores to return. Pass <c>null</c> to use the backend default (e.g., 100).</param>
    /// <param name="filter">Optional. Timeframe filter (e.g., "daily", "monthly", "year", "all"). Pass <c>null</c> to use the backend default.</param>
    /// <returns>A JSON string containing the scores on success, or <c>null</c> if the request fails.</returns>
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
                Debug.LogError($"Fehler beim Laden der Highscores: {request.error}\n{request.downloadHandler.text}");
                return null;
            }
        }
    }

    /// <summary>
    /// Fetches a list of available minigames.
    /// </summary>
    /// <param name="filter">Optional filter criteria for minigames. Pass <c>null</c> to fetch all.</param>
    /// <returns>A JSON string containing the minigames on success, or <c>null</c> if the request fails.</returns>
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

    /// <summary>
    /// Fetches a list of registered users/players.
    /// </summary>
    /// <param name="limit">Optional. Maximum number of users to return. Pass <c>null</c> to use the backend default.</param>
    /// <param name="filter">Optional. Text to search for within usernames (e.g., "Alex"). Pass <c>null</c> or "none" to disable search.</param>
    /// <returns>A JSON string containing the users on success, or <c>null</c> if the request fails.</returns>
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

    /// <summary>
    /// Helper method to construct a standard POST UnityWebRequest with JSON payload and API Key header.
    /// </summary>
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

    /// <summary>
    /// Helper method to compute a SHA-256 hash for score verification.
    /// </summary>
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
    [Serializable] private class ScoreRequest { public string user_id; public int score; public string hash; public int minigame_id; public string started_at; public string ended_at; }
}