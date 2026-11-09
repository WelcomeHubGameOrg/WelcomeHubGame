using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class StairsPlayerController : MonoBehaviour
{
    [Header("Animation")]
    [Tooltip("How much the player squashes when stepping (0 = none).")]
    [SerializeField] private float squashAmount = 0.2f;
    [SerializeField] private float squashRecovery = 12f;

    private StairsGameManager gameManager;
    private int currentColumn = 1;
    private float tickTimer;

    private Vector3 baseScale;
    private Vector3 targetPosition;
    private float squash;

    void Start()
    {
        gameManager = FindAnyObjectByType<StairsGameManager>();
        baseScale = transform.localScale;
        UpdatePlayerPosition();
        transform.position = targetPosition; // start exactly in place
    }

    void Update()
    {
        Animate();

        if (!gameManager.GameStarted)
        {
            // nothing works before the Start button is pressed
            tickTimer = gameManager.GetTickTime();
            return;
        }

        if (gameManager.GameEnded || gameManager.IsPaused)
        {
            return;
        }

        int direction = ReadInput();
        if (direction < 0)
        {
            MoveLeft();
        }
        else if (direction > 0)
        {
            MoveRight();
        }

        if (gameManager.GameEnded || !gameManager.AutoAdvance)
        {
            return;
        }

        // auto mode: the board climbs by itself, the player only steers
        tickTimer -= Time.deltaTime;
        if (tickTimer <= 0f)
        {
            Advance();
        }
    }

    // Small squash when stepping. Time.deltaTime is 0 while paused, so it freezes by itself.
    void Animate()
    {
        squash = Mathf.MoveTowards(squash, 0f, squashRecovery * Time.deltaTime);
        float s = squash * squashAmount;
        transform.localScale = new Vector3(baseScale.x * (1f + s), baseScale.y * (1f - s), baseScale.z);
    }

    // -1 = left, 1 = right, 0 = nothing. Keyboard, mouse click and touch all work
    // pressing the left half of the screen goes left, the right half goes right
    int ReadInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null)
        {
            if (keyboard.aKey.wasPressedThisFrame || keyboard.leftArrowKey.wasPressedThisFrame)
            {
                return -1;
            }

            if (keyboard.dKey.wasPressedThisFrame || keyboard.rightArrowKey.wasPressedThisFrame)
            {
                return 1;
            }
        }

        Pointer pointer = Pointer.current;
        if (pointer != null && pointer.press.wasPressedThisFrame)
        {
            // clicking a button (the pause button) must not also move the player
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                return 0;
            }

            return pointer.position.ReadValue().x < Screen.width * 0.5f ? -1 : 1;
        }

        return 0;
    }

    public void MoveLeft()
    {
        Move(-1);
    }

    public void MoveRight()
    {
        Move(1);
    }

    void Move(int direction)
    {
        if (!gameManager.GameStarted || gameManager.GameEnded || gameManager.IsPaused)
        {
            return;
        }

        int previousColumn = currentColumn;
        currentColumn = Mathf.Clamp(currentColumn + direction, 0, 2);
        UpdatePlayerPosition();

        if (currentColumn != previousColumn)
        {
            gameManager.PlayMoveSound();
            squash = Mathf.Max(squash, 0.5f);
        }

        if (gameManager.AutoAdvance)
        {
            // walking sideways into an obstacle on your own row also counts as a hit
            if (gameManager.BottomRowHasObstacle(currentColumn))
            {
                gameManager.GameOver();
            }
        }
        else
        {
            // original behaviour: every move is also a step
            Advance();
        }
    }

    void UpdatePlayerPosition()
    {
        targetPosition = new Vector3(currentColumn - 1, -2f, -1f);
        transform.position = targetPosition;
    }

    void Advance()
    {
        gameManager.MoveBoard();

        if (gameManager.BottomRowHasObstacle(currentColumn))
        {
            gameManager.GameOver();
        }
        else
        {
            gameManager.PlayStepSound();
            squash = 1f;
            gameManager.AddScore();
        }

        tickTimer = gameManager.GetTickTime();
    }
}