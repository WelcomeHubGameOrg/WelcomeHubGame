using UnityEngine;

[CreateAssetMenu(fileName = "NewMinigame", menuName = "Game/MinigameData")]
public class MinigameData : ScriptableObject
{
    public string displayName;
    public ScenePathSO scene;
    // public enum timeOfDay
    // morning, noon, evening, night

    [Tooltip("Which map zone(s) this minigame can spawn in")]
    public MapZoneType allowedZones = MapZoneType.Any;

    // public bool isUnlocked
    // for unlockable minigames

    //
}
