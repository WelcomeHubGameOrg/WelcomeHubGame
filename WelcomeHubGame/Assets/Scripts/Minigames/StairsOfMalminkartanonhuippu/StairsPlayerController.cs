using UnityEngine;
using UnityEngine.InputSystem;

public class StairsPlayerController : MonoBehaviour
{
    private StairsGameManager gameManager;
    private int currentColumn = 0;
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
        currentColumn = 0;
        transform.position = new Vector3(-0.5f, -2f, -1f);
        MoveAndCheck();
    }

    void MoveRight()
    {
        currentColumn = 1;
        transform.position = new Vector3(0.5f, -2f, -1f);
        MoveAndCheck();
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
