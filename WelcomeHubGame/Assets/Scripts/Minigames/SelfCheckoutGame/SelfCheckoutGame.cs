using System;
using System.Collections.Generic;
using UnityEngine;

public class SelfCheckoutGame : MinigameController
{
    private float _timer = 60f;
    private float _score;
    private bool _timerRunning = true;

    public int ScannedCount = 0;
    public int TotalCount = 10;
    public event Action ScannedItem;

    private void Update()
    {
        if (_timerRunning)
        {
            _timer -= Time.deltaTime;
            if (_timer <= 0)
            {
                _timerRunning = false;
                Debug.Log("Game Over");
            }
        }
    }

    public void NotifyScannedItem()
    {
        ScannedCount++;
        ScannedItem?.Invoke();
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
        }
    }
}
