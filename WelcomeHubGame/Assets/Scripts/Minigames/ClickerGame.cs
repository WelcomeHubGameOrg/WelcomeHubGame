using TMPro;
using UnityEngine;

public class ClickerGame : MinigameController
{
    public int clicks = 0;
    public int targetClicks = 20;
    
    public float timer = 20f;
    
    [SerializeField] TextMeshProUGUI timerText;
    [SerializeField] TextMeshProUGUI clicksText;

    /*
    private void Start()
    {
        // AudioManager.Instance.PlayMusic(MusicID.MUSICHERE);
    }
    */

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
        // AudioManager.Instance.PlaySFX
        if (clicks >= targetClicks)
            FinishMinigame(true);
    }
}
