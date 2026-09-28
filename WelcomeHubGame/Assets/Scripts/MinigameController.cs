using UnityEngine;

public abstract class MinigameController : MonoBehaviour
{
    private bool _finished;
    
    public void FinishMinigame(bool won)
    {
        if (_finished) return;
        _finished = true;
        
        // AudioManager.Instance.StopMusic();
        OnFinished(won);
    }

    /// <summary>
    /// Default: Returns to the map. You can override this to do your own thing, and then call base.OnFinished(won)
    /// when you're actually ready to leave the scene
    /// </summary>
    protected virtual void OnFinished(bool won)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.ReturnToMap(won);
        else
            Debug.Log($"Minigame ended standalone. Did player win? = {won}");
    }
}
