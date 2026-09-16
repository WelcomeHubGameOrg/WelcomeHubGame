using UnityEngine;
using UnityEngine.UI;
using TMPro; // used instead of UnityEngine.UI.Text for nicer text rendering
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Settings")]
    public int targetScore = 35; // win target (30-40)
    public float totalTime = 60f;
    public int poisonTargetPenalty = 5; // +5 to the target for each bad berry

    [Header("UI References")]
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TMP_Text resultText;
    [SerializeField] private Button restartButton;

    private float currentTime;
    private int currentScore = 0;
    private bool isGameOver = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (restartButton) restartButton.onClick.AddListener(RestartGame);
    }

    void Start()
    {
        currentTime = totalTime;
        UpdateUI();
        if (resultPanel) resultPanel.SetActive(false);
    }

    void Update()
    {
        if (isGameOver) return;
        currentTime -= Time.deltaTime;
        if (currentTime <= 0)
        {
            currentTime = 0;
            EndGame(false, "Time is up!");
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
        targetScore += poisonTargetPenalty; // +5 to the target
        CheckWin();
        UpdateUI();
    }

    private void CheckWin()
    {
        if (currentScore >= targetScore)
        {
            EndGame(true, "Basket collected!");
        }
    }

    private void UpdateUI()
    {
        if (timeText) timeText.text = $"Time: {Mathf.CeilToInt(currentTime)}s";
        if (scoreText) scoreText.text = $"Berries: {currentScore} / {targetScore}";
    }

    private void EndGame(bool win, string message)
    {
        isGameOver = true;
        if (resultPanel)
        {
            resultPanel.SetActive(true);
            if (resultText) resultText.text = message;
        }
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
