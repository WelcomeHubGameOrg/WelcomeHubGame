using System;
using TMPro;
using UnityEngine;

public class SelfCheckoutGame : MinigameController
{
    private float _timer = 60f;
    private int _score;
    private bool _timerRunning = true;

    [SerializeField] TextMeshProUGUI timerText;
    
    public int ScannedCount = 0;
    public int TotalCount = 10;
    public event Action<int, string> ScannedItem;

    private void Update()
    {
        if (_timerRunning)
        {
            _timer -= Time.deltaTime;
            timerText.SetText("Timer: {0:0}", _timer);
            if (_timer <= 0f)
            {
                _timerRunning = false;
                _timer = 0f;
                timerText.SetText("Timer: {0:0}", _timer);
                Debug.Log("Game Over");
            }
        }
    }

    public void NotifyScannedItem(int price, string productName)
    {
        ScannedCount++;
        ScannedItem?.Invoke(price, productName);
        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (!_timerRunning) return;
        
        Debug.Log("Checking win condition");
        if (ScannedCount >= TotalCount)
        {
            _timerRunning = false;
            Debug.Log("All items scanned, win game");
            Debug.Log($"Timer : {_timer}");
            CalculateScore();
        }
    }

    private void CalculateScore()
    {
        _score = Mathf.RoundToInt((_timer * 100f));
        Debug.Log($"Score : {_score}");
    }
}
