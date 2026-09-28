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
    /// The minigame's own result handling (win/lose UI, reporting score, etc).
    /// </summary>
    protected abstract void OnFinished(bool won);
}
