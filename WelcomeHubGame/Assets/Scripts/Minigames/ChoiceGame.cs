using UnityEngine;

public class ChoiceGame : MonoBehaviour, IMinigameController
{
    public void FinishMinigame(bool won)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMap(won);
        else
            Debug.Log($"Minigame ended standalone. Did player win? = {won}");
    }
}
