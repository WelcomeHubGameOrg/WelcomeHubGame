using UnityEngine;

// for saving the button locations whenever the scene switches
[System.Serializable]
public class MapButtonData
{
    public MinigameData minigameData;
    public Vector2 screenPosition;

    public MapButtonData(MinigameData data, Vector2 pos)
    {
        minigameData = data;
        screenPosition = pos;
    }
}
