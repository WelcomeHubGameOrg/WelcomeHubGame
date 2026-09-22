using UnityEngine;

public class StairsStartMenu : MonoBehaviour
{
    private StairsGameManager gameManager;

    void Start()
    {
        gameManager = FindAnyObjectByType<StairsGameManager>();
    }
    
    public void StartGame()
    {
        gameObject.SetActive(false);
        gameManager.StartGame();
    }
}
