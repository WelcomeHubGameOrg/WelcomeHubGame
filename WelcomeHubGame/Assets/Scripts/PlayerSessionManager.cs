using System.Threading.Tasks;
using UnityEngine;

/// <summary>Stores the player session across scenes and launches.</summary>
[DefaultExecutionOrder(-100)]
public class PlayerSessionManager : MonoBehaviour
{
    /// <summary>The single instance, set in <c>Awake</c>.</summary>
    public static PlayerSessionManager Instance { get; private set; }

    private const string PlayerNameKey = "session.playerName";
    private const string PlayerGuidKey = "session.playerGuid";
    
    private void Awake()
    {
        if (Instance && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>Whether a player is saved on this device.</summary>
    public bool HasPlayerSession()
    {
        return PlayerPrefs.HasKey(PlayerGuidKey)
               && PlayerPrefs.HasKey(PlayerNameKey);
    }

    /// <summary>Loads the saved player.</summary>
    /// <returns>The player, or <c>null</c> if none is saved.</returns>
    public Player GetPlayer()
    {
        if (!HasPlayerSession())
            return null;
        
        var playerGuid = PlayerPrefs.GetString(PlayerGuidKey);
        var playerName = PlayerPrefs.GetString(PlayerNameKey);
        
        var player = new Player(playerGuid, playerName); 
        
        return player;
    }
    
    /// <summary>Registers a new guest and saves it, replacing any existing session.</summary>
    /// <param name="playerName">The name the player entered.</param>
    /// <returns>The player, or <c>null</c> if registration failed.</returns>
    public async Task<Player> RegisterNewPlayerAsync(string playerName)
    {
        var player = await MinigameApiClient.RegisterGuestAsync(playerName);
        
        if (player == null)
            return null;
        
        PlayerPrefs.SetString(PlayerNameKey, player.username);
        PlayerPrefs.SetString(PlayerGuidKey, player.guid);
        PlayerPrefs.Save();
        
        return player;
    }
}
