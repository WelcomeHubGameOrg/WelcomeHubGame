using UnityEngine;

[CreateAssetMenu(fileName = "NewMinigame", menuName = "Game/MinigameData")]
public class MinigameData : ScriptableObject
{
    public string minigameID; // this one might not be necessary at all
    public string sceneName;
    public string displayName;
    // public enum timeOfDay
    // morning, noon, evening, night
    
    // public enum zone
    // A, B, C, D, ANY
    
    // public bool isUnlocked
    // for unlockable minigames
    
    //
}
