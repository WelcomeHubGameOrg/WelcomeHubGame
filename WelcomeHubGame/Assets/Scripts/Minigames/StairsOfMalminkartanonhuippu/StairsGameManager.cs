using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StairsGameManager : MonoBehaviour
{
    private const string MutedKey = "StairsMuted";
    private const string AutoAdvanceKey = "StairsAutoAdvance";

    private StairsRow[] rows = new StairsRow[5];
    private int score = 0;
    private int emptyRowStreak = 0;
    private int obstacleRowStreak = 0;
    private float elapsedTime = 0f;      // Step Mode only
    private bool timerRunning = false;   // starts with the first step
    private int lastShownTenths = -1;
    private RectTransform hudBarRect;
    private RectTransform hudTextRect;
    private Camera hudCamera;
    private const int MaxScore = 7;
    public bool GameStarted { get; private set; }
    public bool GameEnded { get; private set; }
    public bool IsPaused { get; private set; }
    public bool IsMuted { get; private set; }
    public bool HasWon { get; private set; }
    public string WinMessage { get; private set; }
    public int CountdownValue { get; private set; } // 3, 2, 1 while counting down after Resume, otherwise 0
    public bool IsCountingDown => CountdownValue > 0;
    private float countdownTimer;
    public int MaxScoreValue => MaxScore;
    [SerializeField] private GameObject obstaclePrefab;
    [Tooltip("Optional: more obstacle types. Every new obstacle uses a random prefab out of this list plus the Obstacle Prefab above.")]
    [SerializeField] private GameObject[] extraObstaclePrefabs;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text scoreText;

    [Header("Win screen (optional, reuses the game over screen if empty)")]
    [SerializeField] private GameObject winScreen;

    [Header("Live HUD (optional, created automatically if empty)")]
    [SerializeField] private TMP_Text liveScoreText;
    [SerializeField] private Image progressFill;

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
    [Tooltip("Played on every number of the 3-2-1 after Resume (Auto Climb).")]
    [SerializeField] private AudioClip countdownClip;

    public bool AutoAdvance => autoAdvance;

    void Awake()
    {
        autoAdvance = PlayerPrefs.GetInt(AutoAdvanceKey, autoAdvance ? 1 : 0) == 1; // last chosen mode
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
        if (!IsPaused || IsCountingDown)
        {
            return;
        }

        if (autoAdvance)
        {
            // Auto Climb: 3-2-1 first, the game stays frozen until it is over
            CountdownValue = 3;
            countdownTimer = 1f;
            PlaySfx(countdownClip);
            return;
        }

        FinishResume();
    }

    void FinishResume()
    {
        IsPaused = false;
        Time.timeScale = 1f;

        if (musicSource != null)
        {
            musicSource.UnPause();
        }
    }

    // Mode choice on the start screen
    public void SetAutoAdvance(bool value)
    {
        if (GameStarted)
        {
            return;
        }

        autoAdvance = value;
        PlayerPrefs.SetInt(AutoAdvanceKey, value ? 1 : 0);
        PlayerPrefs.Save();
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

    GameObject PickObstaclePrefab()
    {
        int extraCount = extraObstaclePrefabs != null ? extraObstaclePrefabs.Length : 0;
        int index = Random.Range(0, extraCount + 1);

        if (index == 0 || extraObstaclePrefabs[index - 1] == null)
        {
            return obstaclePrefab;
        }

        return extraObstaclePrefabs[index - 1];
    }

    float GetObstacleChance()
    {
        return Mathf.Min(maxObstacleChance, startObstacleChance + GetLevel() * obstacleChanceIncrease);
    }

    void CreateObstacle(StairsRow row, int column)
    {
        GameObject prefab = PickObstaclePrefab();

        if (prefab == null)
        {
            Debug.LogWarning("StairsGameManager: Obstacle Prefab is not assigned.");
            return;
        }

        GameObject obstacle = Instantiate(prefab);
        obstacle.transform.SetParent(row.transform);
        obstacle.transform.localPosition = new Vector3(column - 1, 0, -1);

        row.SetObstacle(obstacle);
    }

    public void MoveBoard()
    {
        rows[4].DestroyRow();

        for (int row = 4; row > 0; row--)
        {
            rows[row] = rows[row - 1];

            rows[row].transform.position = new Vector3(0, 2 - row, 0);
        }
        CreateRow(0);
    }

    public bool BottomRowHasObstacle(int column)
    {
        return rows[4].obstacle != null && Mathf.RoundToInt(rows[4].obstacle.transform.localPosition.x + 1) == column;
    }

    // Seconds between automatic steps
    public float GetTickTime()
    {
        return Mathf.Max(fastestStepTime, firstStepTime - GetLevel() * stepTimeDecrease);
    }

    // Step Mode only: the timer starts with the first step and stops at game over / win.

    void Update()
    {
        // 3-2-1 after Resume. Runs on unscaled time because the game is still frozen.
        if (CountdownValue > 0)
        {
            countdownTimer -= Time.unscaledDeltaTime;

            if (countdownTimer <= 0f)
            {
                CountdownValue--;
                countdownTimer = 1f;

                if (CountdownValue == 0)
                {
                    FinishResume();
                }
                else
                {
                    PlaySfx(countdownClip);
                }
            }

            return;
        }

        if (autoAdvance || !timerRunning || GameEnded || IsPaused)
        {
            return;
        }

        elapsedTime += Time.deltaTime;

        int tenths = Mathf.FloorToInt(elapsedTime * 10f);
        if (tenths != lastShownTenths)
        {
            lastShownTenths = tenths;
            UpdateHud();
        }
    }

    public float GetElapsedTime()
    {
        return elapsedTime;
    }

    static string FormatTime(float time)
    {
        int minutes = Mathf.FloorToInt(time / 60f);
        float seconds = time - minutes * 60f;
        return minutes + ":" + seconds.ToString("00.0", System.Globalization.CultureInfo.InvariantCulture);
    }

    // extra line for the end screens, only in Step Mode
    string TimeLine()
    {
        return autoAdvance ? "" : "\nTime: " + FormatTime(elapsedTime);
    }

    public void AddScore()
    {
        score++;
        timerRunning = true;
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
            string hudText = score + " / " + MaxScore;
            if (!autoAdvance)
            {
                hudText += "   " + FormatTime(elapsedTime);
            }

            liveScoreText.text = hudText;
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

            if (winScreen != null)
            {
                // a win screen
                winScreen.SetActive(true);
            }
            else
            {
                // otherwise StairsPauseMenu shows its own win screen (with a Menu button)
                WinMessage = "Score: " + score + " / " + MaxScore + TimeLine();
                HasWon = true;
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
        ShowEndScreen(gameOverScreen, "Game Over\nScore: " + score + " / " + MaxScore + TimeLine());
        Time.timeScale = 0;
    }
}