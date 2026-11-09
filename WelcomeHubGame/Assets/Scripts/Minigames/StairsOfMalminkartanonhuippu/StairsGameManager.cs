using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StairsGameManager : MonoBehaviour
{
    private const string MutedKey = "StairsMuted";

    private StairsRow[] rows = new StairsRow[5];
    private int score = 0;
    private int emptyRowStreak = 0;
    private int obstacleRowStreak = 0;
    private RectTransform hudBarRect;
    private RectTransform hudTextRect;
    private Camera hudCamera;
    private const int MaxScore = 347;
    public bool GameStarted { get; private set; }
    public bool GameEnded { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsMuted { get; private set; }
    public int MaxScoreValue => MaxScore;
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text scoreText;

    [Header("Win screen (optional, reuses the game over screen if empty)")]
    [SerializeField] private GameObject winScreen;

    [Header("Live HUD (optional, created automatically if empty)")]
    [SerializeField] private TMP_Text liveScoreText;
    [SerializeField] private Image progressFill; // Image Type must be "Filled"

    [Header("Movement")]
    [Tooltip("ON: the board climbs by itself and A/D only change lane. OFF: every A/D press is one step (original behaviour).")]
    [SerializeField] private bool autoAdvance = true;

    [Header("Difficulty: every `Score Step` points the game gets harder")]
    [SerializeField] private int scoreStep = 50;
    [Tooltip("Seconds per automatic step at the start (only used when Auto Advance is on).")]
    [SerializeField] private float firstStepTime = 0.9f;
    [SerializeField] private float stepTimeDecrease = 0.1f;
    [SerializeField] private float fastestStepTime = 0.4f;
    [Tooltip("Chance that a new row has a cat at the start.")]
    [SerializeField] private float startObstacleChance = 0.5f;
    [SerializeField] private float obstacleChanceIncrease = 0.05f;
    [SerializeField] private float maxObstacleChance = 0.9f;
    [Tooltip("At most this many rows in a row can be empty, so the screen never stays empty for long.")]
    [SerializeField] private int maxEmptyRowsInARow = 2;
    [Tooltip("At most this many rows in a row can have a cat, so there is always room to breathe (breaks long diagonal chains).")]
    [SerializeField] private int maxObstacleRowsInARow = 2;

    [Header("Animation")]
    [Tooltip("How long one step of the board takes (seconds). Keep it well below the fastest step time.")]
    [SerializeField] private float rowSlideTime = 0.12f;

    [Header("Audio (all optional)")]
    [Tooltip("Set the music clip and tick Loop on this AudioSource. Play On Awake should be OFF.")]
    [SerializeField] private AudioSource musicSource;
    [Tooltip("If empty, an AudioSource is created automatically.")]
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private AudioClip loseClip;
    [SerializeField] private AudioClip winClip;
    [SerializeField] private AudioClip stepClip;
    [SerializeField] private AudioClip moveClip;
    [SerializeField] private AudioClip clickClip;

    public bool AutoAdvance => autoAdvance;

    void Awake()
    {
        IsMuted = PlayerPrefs.GetInt(MutedKey, 0) == 1;
        AudioListener.volume = IsMuted ? 0f : 1f;

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }

        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas != null)
        {
            // fixes the UI layout (scaling, overlapping texts); see StairsUiLayout
            if (canvas.GetComponent<StairsUiLayout>() == null)
            {
                canvas.gameObject.AddComponent<StairsUiLayout>();
            }

            // pause button, pause menu and info page; see StairsPauseMenu
            if (canvas.GetComponent<StairsPauseMenu>() == null)
            {
                canvas.gameObject.AddComponent<StairsPauseMenu>();
            }
        }
    }

    void OnDestroy()
    {
        // never leave the game frozen if this scene is closed while paused / game over
        Time.timeScale = 1f;
    }

#if !UNITY_EDITOR
    // browser tab / window lost focus: pause automatically
    void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
        {
            Pause();
        }
    }
#endif

    public void StartGame()
    {
        GameStarted = true;
        CreateBoard();
        CreateHudIfMissing();
        UpdateHud();

        if (musicSource != null)
        {
            musicSource.Play();
        }
    }

    // ---- pause / sound ----

    public void Pause()
    {
        if (!GameStarted || GameEnded || IsPaused)
        {
            return;
        }

        IsPaused = true;
        Time.timeScale = 0f;

        if (musicSource != null)
        {
            musicSource.Pause();
        }
    }

    public void Resume()
    {
        if (!IsPaused)
        {
            return;
        }

        IsPaused = false;
        Time.timeScale = 1f;

        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    public void SetMuted(bool muted)
    {
        IsMuted = muted;
        AudioListener.volume = muted ? 0f : 1f;
        PlayerPrefs.SetInt(MutedKey, muted ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void PlayStepSound() { PlaySfx(stepClip); }
    public void PlayMoveSound() { PlaySfx(moveClip); }
    public void PlayClick() { PlaySfx(clickClip); }

    // ---- board ----

    void CreateBoard()
    {
        for (int row = 0; row < 5; row++)
        {
            CreateRow(row);
        }
    }

    void CreateRow(int rowNumber)
    {
        GameObject rowObject = new GameObject("Row " + rowNumber);
        StairsRow row = rowObject.AddComponent<StairsRow>();
        rowObject.transform.position = new Vector3(0, 2 - rowNumber, 0);

        for (int column = 0; column < 3; column++)
        {
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.transform.SetParent(rowObject.transform);
            cell.transform.localPosition = new Vector3(column - 1, 0, 0);

            // nothing uses physics, so the colliders are only wasted work
            Collider cellCollider = cell.GetComponent<Collider>();
            if (cellCollider != null)
            {
                Destroy(cellCollider);
            }
        }

        if (rowNumber != 4)
        {
            bool mustHaveObstacle = emptyRowStreak >= maxEmptyRowsInARow;
            bool mustBeEmpty = obstacleRowStreak >= maxObstacleRowsInARow;

            if (!mustBeEmpty && (mustHaveObstacle || Random.value < GetObstacleChance()))
            {
                int obstacleColumn = Random.Range(0, 3);
                CreateObstacle(row, obstacleColumn);
                emptyRowStreak = 0;
                obstacleRowStreak++;
            }
            else
            {
                emptyRowStreak++;
                obstacleRowStreak = 0;
            }
        }

        rows[rowNumber] = row;
    }

    int GetLevel()
    {
        return score / Mathf.Max(1, scoreStep);
    }

    float GetObstacleChance()
    {
        return Mathf.Min(maxObstacleChance, startObstacleChance + GetLevel() * obstacleChanceIncrease);
    }

    void CreateObstacle(StairsRow row, int column)
    {
        if (obstaclePrefab == null)
        {
            Debug.LogWarning("StairsGameManager: Obstacle Prefab is not assigned.");
            return;
        }

        GameObject obstacle = Instantiate(obstaclePrefab);
        obstacle.transform.SetParent(row.transform);
        obstacle.transform.localPosition = new Vector3(column - 1, 0, -1);

        foreach (Collider obstacleCollider in obstacle.GetComponentsInChildren<Collider>())
        {
            Destroy(obstacleCollider);
        }

        foreach (Collider2D obstacleCollider2D in obstacle.GetComponentsInChildren<Collider2D>())
        {
            Destroy(obstacleCollider2D);
        }

        row.SetObstacle(obstacle);
    }

    public void MoveBoard()
    {
        // the old bottom row slides out and removes itself
        rows[4].SlideTo(new Vector3(0, 2 - 5, 0), rowSlideTime, true);

        for (int row = 4; row > 0; row--)
        {
            rows[row] = rows[row - 1];
            rows[row].SlideTo(new Vector3(0, 2 - row, 0), rowSlideTime, false);
        }

        // the new top row slides in from above
        CreateRow(0);
        rows[0].transform.position = new Vector3(0, 3, 0);
        rows[0].SlideTo(new Vector3(0, 2, 0), rowSlideTime, false);
    }

    public bool BottomRowHasObstacle(int column)
    {
        return rows[4].obstacle != null && Mathf.RoundToInt(rows[4].obstacle.transform.localPosition.x + 1) == column;
    }

    // Seconds between automatic steps. Gets shorter every `scoreStep` points.
    public float GetTickTime()
    {
        return Mathf.Max(fastestStepTime, firstStepTime - GetLevel() * stepTimeDecrease);
    }

    public void AddScore()
    {
        score++;
        UpdateHud();

        if (score >= MaxScore)
        {
            Win();
        }
    }

    public int GetScore()
    {
        return score;
    }

    // Builds the score text and the progress bar in code, so nothing has to be wired up in the editor.
    void CreateHudIfMissing()
    {
        Canvas canvas = FindAnyObjectByType<Canvas>();
        if (canvas == null)
        {
            return;
        }

        if (progressFill == null)
        {
            Sprite white = CreateWhiteSprite();

            GameObject bar = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
            bar.transform.SetParent(canvas.transform, false);
            Image barImage = bar.GetComponent<Image>();
            barImage.sprite = white;
            barImage.color = new Color(1f, 1f, 1f, 0.25f); // light track so it is visible on the dark photo
            barImage.raycastTarget = false; // must not block clicks on the game
            RectTransform barRect = bar.GetComponent<RectTransform>();
            hudBarRect = barRect; // positioned just above the frame in LateUpdate
            bar.transform.SetAsFirstSibling(); // behind the start / game over screens

            GameObject fill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
            fill.transform.SetParent(bar.transform, false);
            progressFill = fill.GetComponent<Image>();
            progressFill.sprite = white;
            progressFill.color = new Color(0.3f, 0.8f, 0.4f, 1f);
            progressFill.raycastTarget = false;
            progressFill.type = Image.Type.Filled;
            progressFill.fillMethod = Image.FillMethod.Horizontal;
            progressFill.fillOrigin = 0;
            RectTransform fillRect = progressFill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
        }

        if (liveScoreText == null)
        {
            GameObject textObject = new GameObject("LiveScoreText", typeof(RectTransform));
            textObject.transform.SetParent(canvas.transform, false);
            liveScoreText = textObject.AddComponent<TextMeshProUGUI>();
            liveScoreText.color = Color.white;
            liveScoreText.alignment = TextAlignmentOptions.Bottom;
            liveScoreText.enableAutoSizing = true;
            liveScoreText.fontSizeMin = 18f;
            liveScoreText.fontSizeMax = 40f;
            liveScoreText.raycastTarget = false; // must not block clicks on the game
            hudTextRect = liveScoreText.rectTransform; // positioned just above the bar in LateUpdate
            textObject.transform.SetAsFirstSibling(); // behind the start / game over screens
        }
    }

    // Keeps the progress bar and the score right above the frame around the board, on any screen size.
    void LateUpdate()
    {
        if (hudBarRect == null && hudTextRect == null)
        {
            return;
        }

        if (hudCamera == null)
        {
            hudCamera = Camera.main;
        }

        if (hudCamera == null)
        {
            return;
        }

        float halfWidth = StairsUiLayout.FrameWidth * 0.5f;
        float frameTop = StairsUiLayout.FrameHeight * 0.5f;

        PlaceInWorldArea(hudBarRect, -halfWidth, halfWidth, frameTop + 0.10f, frameTop + 0.35f);
        PlaceInWorldArea(hudTextRect, -2.2f, 2.2f, frameTop + 0.40f, frameTop + 1.00f);
    }

    void PlaceInWorldArea(RectTransform rect, float left, float right, float bottom, float top)
    {
        if (rect == null)
        {
            return;
        }

        Vector3 min = hudCamera.WorldToViewportPoint(new Vector3(left, bottom, 0f));
        Vector3 max = hudCamera.WorldToViewportPoint(new Vector3(right, top, 0f));
        rect.anchorMin = new Vector2(min.x, min.y);
        rect.anchorMax = new Vector2(max.x, max.y);
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        rect.localScale = Vector3.one;
    }

    Sprite CreateWhiteSprite()
    {
        Texture2D texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
    }

    void UpdateHud()
    {
        if (liveScoreText != null)
        {
            liveScoreText.text = score + " / " + MaxScore;
        }

        if (progressFill != null)
        {
            progressFill.fillAmount = (float)score / MaxScore;
        }
    }

    void StopMusic()
    {
        if (musicSource != null)
        {
            musicSource.Stop();
        }
    }

    void PlaySfx(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    void ShowEndScreen(GameObject screen, string message)
    {
        if (scoreText != null)
        {
            scoreText.text = message;
        }

        if (screen != null)
        {
            screen.SetActive(true);
        }
        else
        {
            Debug.LogWarning("StairsGameManager: no end screen assigned.");
        }
    }

    public void Win()
    {
        if (GameEnded)
        {
            return;
        }

        GameEnded = true;
        StopMusic();
        PlaySfx(winClip);
        Time.timeScale = 0;

        string message = "You reached the top!\nScore: " + score + " / " + MaxScore;

        if (winScreen != null)
        {
            winScreen.SetActive(true);
        }
        else
        {
            // no win screen assigned yet: reuse the game over screen with a different text
            ShowEndScreen(gameOverScreen, message);
        }
    }

    public void GameOver()
    {
        if (GameEnded)
        {
            return;
        }

        GameEnded = true;
        StopMusic();
        PlaySfx(loseClip);
        ShowEndScreen(gameOverScreen, "Game Over\nScore: " + score + " / " + MaxScore);
        Time.timeScale = 0;
    }
}