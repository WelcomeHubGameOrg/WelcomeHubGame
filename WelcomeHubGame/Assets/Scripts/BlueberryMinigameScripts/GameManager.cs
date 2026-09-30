using UnityEngine;
using TMPro; // used instead of UnityEngine.UI.Text for nicer text rendering

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [System.Serializable]
    public class LevelConfig
    {
        public string levelName = "Level";
        public GameObject content;      // root GameObject holding this level's platforms/bushes/rocks (SetActive toggled per level)
        public Transform spawnPoint;    // where the player appears when this level (re)starts
        public int targetScore = 35;    // berries needed to clear this level
        public float totalTime = 60f;   // seconds allowed for this level
    }

    [Header("Levels")]
    public LevelConfig[] levels;

    [Header("Penalty")]
    public int poisonTargetPenalty = 5; // +5 to this attempt's target for each bad berry eaten

    [Header("Player")]
    [SerializeField] private Rigidbody2D playerRb; // teleported to each level's spawn point, velocity reset on (re)load

    [Header("UI References")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private GameObject resultPanel;       // shown when a level attempt fails (time runs out) — its Retry button is wired to RestartGame()
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private GameObject gameCompletePanel; // shown ONLY after the LAST level is cleared — separate from resultPanel

    private int currentLevelIndex = 0;
    private float currentTime;
    private int currentScore;
    private int currentTargetScore;
    private bool isGameOver;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        if (resultPanel) resultPanel.SetActive(false);
        if (gameCompletePanel) gameCompletePanel.SetActive(false);

        if (levels == null || levels.Length == 0)
        {
            Debug.LogError($"[{nameof(GameManager)}] No levels configured in the Inspector — nothing to play.", this);
            return;
        }

        LoadLevel(0);
    }

    void Update()
    {
        if (isGameOver) return;

        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            EndLevelFail("Time is up! Try again.");
        }
        UpdateUI();
    }

    public void AddGoodBerry()
    {
        if (isGameOver) return;
        currentScore++;
        CheckWin();
        UpdateUI();
    }

    public void AddBadBerryPenalty()
    {
        if (isGameOver) return;
        currentTargetScore += poisonTargetPenalty; // this attempt now needs more berries
        CheckWin();
        UpdateUI();
    }

    private void CheckWin()
    {
        if (currentScore < currentTargetScore) return;

        if (currentLevelIndex >= levels.Length - 1)
            EndGameComplete();
        else
            LoadLevel(currentLevelIndex + 1);
    }

    // Activates only this level's content, resets its bushes, respawns the player at its
    // spawn point, and resets the timer/score/target for a fresh attempt. Used both to
    // advance to the next level and to retry the current one after a failed attempt.
    // Nothing here loads a new Unity scene — every level lives in this same scene and is
    // just toggled active/inactive.
    private void LoadLevel(int index)
    {
        currentLevelIndex = index;
        LevelConfig level = levels[index];

        foreach (LevelConfig lvl in levels)
            if (lvl.content) lvl.content.SetActive(false);
        if (level.content) level.content.SetActive(true);

        if (level.content)
        {
            // SetActive doesn't re-run Start(), so bushes need an explicit reset for a clean attempt
            foreach (Bush bush in level.content.GetComponentsInChildren<Bush>(true))
                bush.ResetBush();
        }

        if (playerRb)
        {
            playerRb.linearVelocity = Vector2.zero;
            if (level.spawnPoint) playerRb.position = level.spawnPoint.position;
        }

        currentTargetScore = level.targetScore;
        currentTime = level.totalTime;
        currentScore = 0;
        isGameOver = false;

        if (resultPanel) resultPanel.SetActive(false);
        if (gameCompletePanel) gameCompletePanel.SetActive(false);

        UpdateUI();
    }

    private void UpdateUI()
    {
        if (timeText) timeText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        if (scoreText) scoreText.text = $"Berries: {currentScore} / {currentTargetScore}";
        if (levelText && levels != null && levels.Length > 0)
            levelText.text = $"Level {currentLevelIndex + 1} / {levels.Length}";
    }

    private void EndLevelFail(string message)
    {
        isGameOver = true;
        if (resultPanel)
        {
            resultPanel.SetActive(true);
            if (resultText) resultText.text = message;
        }
    }

    private void EndGameComplete()
    {
        isGameOver = true;
        if (gameCompletePanel) gameCompletePanel.SetActive(true);
    }

    // Wired to the Retry button on resultPanel — retries the SAME level from scratch (not the whole game).
    public void RestartGame()
    {
        LoadLevel(currentLevelIndex);
    }

    // Optional: wire to a "Play Again" button on gameCompletePanel to replay from Level 1.
    public void RestartFullGame()
    {
        LoadLevel(0);
    }
}
