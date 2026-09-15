using System.Collections.Generic;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Map state preservation")]
    public List<MapButtonData> mapState = new List<MapButtonData>();
    
    [Header("Minigame pools")] 
    public List<MinigameData> AvailableMinigames;
    public int completedMinigames = 0;
    public int failedMinigames = 0;

    public MinigameData currentMinigame { get; private set; }
    
    // this should be changed into an enum probably
    [SerializeField] string mainMapScene = "MainMap";

    private void Awake()
    {
        if (!Instance) {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void LaunchMinigame(MinigameData minigame)
    {
        currentMinigame = minigame;
        SceneManager.LoadScene(minigame.sceneName);
    }

    public void ReturnToMap(bool isWon)
    {
        if (currentMinigame != null)
        {
            if (isWon)
                completedMinigames++;
            else
                failedMinigames++;
        }

        currentMinigame = null;
        SceneManager.LoadScene(mainMapScene);
    }
}
