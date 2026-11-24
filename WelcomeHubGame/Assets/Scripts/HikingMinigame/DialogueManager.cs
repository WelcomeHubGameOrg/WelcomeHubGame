using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HikingMinigame.Effects;
using HikingMinigame.Events;
using HikingMinigame.Items;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace HikingMinigame
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("dialoguePanel")] private GameObject _panel;
        [SerializeField, FormerlySerializedAs("titleText")] private TMP_Text _titleText;
        [SerializeField, FormerlySerializedAs("descriptionText")] private TMP_Text _descriptionText;
        [SerializeField, FormerlySerializedAs("choiceContainer")] private Transform _buttonContainer;
        [SerializeField, FormerlySerializedAs("choiceButtonPrefab")] private Button _buttonPrefab;
        [SerializeField, FormerlySerializedAs("slowdownDuration")] private float _slowdownDuration = 0.5f;
        [SerializeField] private string _continueLabel = "Continue";
        [SerializeField] private string _gameOverMessage = "You are too exhausted to go on.";
        [SerializeField] private string _summaryTitle = "The hike is over";
        [SerializeField] private string _revivedTitle = "Exhausted!";

        private Coroutine _slowdown;

        public event Action SummaryClosed;

        private static HikingGameManager Game => HikingGameManager.Instance;

        private void Start()
        {
            Game.GameOver += OnGameOver;
            Game.Revived += OnRevived;
        }

        private void OnDestroy()
        {
            if (Game == null)
                return;

            Game.GameOver -= OnGameOver;
            Game.Revived -= OnRevived;
        }

        public void ShowEvent(EventDefinition definition)
        {
            OpenPanel(definition.Title, definition.Description);

            foreach (var choice in definition.Choices)
                AddChoiceButton(choice);
        }

        private void OnGameOver()
        {
            if (!_panel.activeSelf)
                ShowSummary();
        }

        private void OnRevived(ItemDefinition item)
        {
            if (_panel.activeSelf)
                return;

            OpenPanel(_revivedTitle, $"You collapse on the trail. Your {item.DisplayName} gets you back on your feet.");
            AddButton(_continueLabel, Close);
        }

        private void ShowSummary()
        {
            var score = Game.Score;
            var summary = string.Join("\n",
                $"You hiked for {score.Duration:m\\:ss}.",
                $"Time: {score.TimePoints} points",
                $"Nature: {score.NaturePoints} points",
                $"Total: {score.Total} points");

            OpenPanel(_summaryTitle, summary);
            AddButton(_continueLabel, CloseSummary);
        }

        private void CloseSummary()
        {
            HidePanel();
            SummaryClosed?.Invoke();
        }

        private void OpenPanel(string title, string text)
        {
            if (_slowdown != null)
                StopCoroutine(_slowdown);

            Game.Pause();
            _slowdown = StartCoroutine(SlowDownTime());
            _panel.SetActive(true);

            _titleText.text = title;
            _descriptionText.text = text;
            ClearButtons();
        }

        private void AddChoiceButton(EventChoice choice)
        {
            var isAvailable = choice.IsAvailable(Game.Inventory);
            var label = isAvailable ? choice.Text : $"{choice.Text} (needs {choice.RequiredItem.DisplayName})";

            var button = AddButton(label, () => SelectChoice(choice));
            button.interactable = isAvailable;
        }

        private void SelectChoice(EventChoice choice)
        {
            var context = new EffectContext(Game);
            var outcome = choice.Resolve(Game.Inventory);
            outcome.Apply(context);

            ShowResult(outcome.Text, context.Messages);
        }

        private void ShowResult(string outcomeText, IEnumerable<string> messages)
        {
            var lines = messages.Prepend(outcomeText);
            if (Game.IsGameOver)
                lines = lines.Append(_gameOverMessage);

            _descriptionText.text = string.Join("\n", lines);

            ClearButtons();
            AddButton(_continueLabel, Close);
        }

        private void Close()
        {
            if (Game.IsGameOver)
            {
                ShowSummary();
                return;
            }

            HidePanel();
            Time.timeScale = 1f;
            Game.Resume();
        }

        private void HidePanel()
        {
            if (_slowdown != null)
                StopCoroutine(_slowdown);

            _panel.SetActive(false);
        }

        private Button AddButton(string label, UnityAction onClick)
        {
            var button = Instantiate(_buttonPrefab, _buttonContainer);
            button.GetComponentInChildren<TMP_Text>().text = label;
            button.onClick.AddListener(onClick);
            return button;
        }

        private void ClearButtons()
        {
            foreach (Transform child in _buttonContainer)
                Destroy(child.gameObject);
        }

        private IEnumerator SlowDownTime()
        {
            var startScale = Time.timeScale;
            var elapsed = 0f;

            while (elapsed < _slowdownDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                Time.timeScale = Mathf.SmoothStep(startScale, 0f, elapsed / _slowdownDuration);
                yield return null;
            }

            Time.timeScale = 0f;
        }
    }
}
