using System;
using TMPro;
using UnityEngine;

public class ClickerGame : MonoBehaviour, IMinigameController
{
    public int clicks = 0;
    public int targetClicks = 20;
    
    public float timer = 20f;
    
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI clicksText;

    private void Update()
    {
        timer -= Time.deltaTime;
        timerText.SetText("Timer: {0:0}", timer);
        clicksText.SetText("Clicks: {0}", clicks);

        if (timer <= 0)
        {
            Debug.Log("Minigame finished, player loses");
            FinishMinigame(false);
        }
    }

    public void RegisterClick()
    {
        clicks++;
        if (clicks >= targetClicks)
            FinishMinigame(true);
    }

    public void FinishMinigame(bool won)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMap(won);
        else
            Debug.Log($"Minigame ended standalone. Did player win? = {won}");
    }
}
