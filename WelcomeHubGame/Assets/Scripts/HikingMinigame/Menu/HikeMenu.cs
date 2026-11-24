using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HikingMinigame.Menu
{
    public class HikeMenu : MonoBehaviour
    {
        [SerializeField] private GameObject _menuPanel;
        [SerializeField] private Button _startButton;
        [SerializeField] private Button _highscoreButton;
        [SerializeField] private Button _quitButton;
        [SerializeField] private GameObject _highscorePanel;
        [SerializeField] private TMP_Text _highscoreText;
        [SerializeField] private Button _backButton;
        [SerializeField] private DialogueManager _dialogue;
        [SerializeField] private int _minigameId = 1;
        [SerializeField] private int _highscoreCount = 10;
        [SerializeField] private string _loadingMessage = "Loading highscores...";
        [SerializeField] private string _unavailableMessage = "Highscores are not available right now.";
        [SerializeField] private string _noScoresMessage = "No highscores yet. Be the first!";

        private HikeHighscores _highscores;
        private Task _pendingSubmit = Task.CompletedTask;

        private static HikingGameManager Game => HikingGameManager.Instance;
        private bool IsDestroyed => this == null;

        private void Awake()
        {
            _highscores = new HikeHighscores(_minigameId);

            _startButton.onClick.AddListener(StartHike);
            _highscoreButton.onClick.AddListener(ShowHighscores);
            _quitButton.onClick.AddListener(Quit);
            _backButton.onClick.AddListener(ShowMenu);
        }

        private void Start()
        {
            Game.GameOver += OnGameOver;
            _dialogue.SummaryClosed += ShowMenu;
            _ = _highscores.EnsurePlayerAsync();

            if (HikeSession.StartImmediately)
            {
                HikeSession.StartImmediately = false;
                BeginRun();
            }
            else
            {
                ShowMenu();
            }
        }

        private void OnDestroy()
        {
            if (Game != null)
                Game.GameOver -= OnGameOver;

            if (_dialogue != null)
                _dialogue.SummaryClosed -= ShowMenu;
        }

        private void OnGameOver()
        {
            HikeSession.RecordRun(Game.Score);
            _pendingSubmit = _highscores.SubmitAsync(Game.Score, Game.RunStartedAt, Game.RunEndedAt);
        }

        private void StartHike()
        {
            if (Game.IsGameOver)
                RestartScene();
            else
                BeginRun();
        }

        private void BeginRun()
        {
            _menuPanel.SetActive(false);
            _highscorePanel.SetActive(false);
            Game.StartRun();
        }

        private static void RestartScene()
        {
            HikeSession.StartImmediately = true;
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().path);
        }

        private async void ShowHighscores()
        {
            _menuPanel.SetActive(false);
            _highscorePanel.SetActive(true);
            _highscoreText.text = _loadingMessage;

            await _pendingSubmit;
            var entries = await _highscores.GetTopAsync(_highscoreCount);
            if (IsDestroyed)
                return;

            _highscoreText.text = FormatHighscores(entries);
        }

        private string FormatHighscores(IReadOnlyList<HighscoreEntry> entries)
        {
            if (entries == null)
                return _unavailableMessage;

            if (entries.Count == 0)
                return _noScoresMessage;

            return string.Join("\n", entries.Select((entry, index) =>
                $"{index + 1}. {entry.PlayerName}{(entry.IsCurrentPlayer ? " (you)" : string.Empty)}   {entry.Score}"));
        }

        private void ShowMenu()
        {
            _menuPanel.SetActive(true);
            _highscorePanel.SetActive(false);
        }

        private void Quit()
        {
            var hasWon = HikeSession.HasWonAnyRun;
            HikeSession.End();
            Game.Finish(hasWon);
        }
    }
}
