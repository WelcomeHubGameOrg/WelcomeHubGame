using UnityEngine;
using UnityEngine.InputSystem;

public class StairsPlayerController : MonoBehaviour
{
    private StairsGameManager gameManager;
    private int currentColumn = 1;
    private bool gameOver = false;

    void Start()
    {
        gameManager = FindAnyObjectByType<StairsGameManager>();
    }

    void Update()
    {
        if (gameOver)
        {
            return;
        }
        if (Keyboard.current.aKey.wasPressedThisFrame ||
        Keyboard.current.leftArrowKey.wasPressedThisFrame)
        {
            MoveLeft();
        }

        if (Keyboard.current.dKey.wasPressedThisFrame ||
        Keyboard.current.rightArrowKey.wasPressedThisFrame)
        {
            MoveRight();
        }
    }

    void MoveLeft()
    {
        if (currentColumn > 0)
        {
            currentColumn--;
        }
        UpdatePlayerPosition();
        MoveAndCheck();
    }

    void MoveRight()
    {
        if (currentColumn < 2)
        {
            currentColumn++;
        }
        UpdatePlayerPosition();
        MoveAndCheck();
    }

    void UpdatePlayerPosition()
    {
        transform.position = new Vector3(currentColumn - 1, -2f, -1f);
    }

    void MoveAndCheck()
    {
        gameManager.MoveBoard();
        
        if(gameManager.BottomRowHasObstacle(currentColumn))
        {
            gameOver = true;
            gameManager.GameOver();
        }
        else
        {
            gameManager.AddScore();
        }
    }
}
