using UnityEngine;

public class ChoiceGame : MinigameController
{
    protected override void OnFinished(bool won)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMap(won);
        else
            Debug.Log($"Minigame ended standalone. Did player win? = {won}");
    }
}
