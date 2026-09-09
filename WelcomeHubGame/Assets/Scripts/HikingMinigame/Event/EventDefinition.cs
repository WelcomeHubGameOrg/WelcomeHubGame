using UnityEngine;

[CreateAssetMenu(fileName = "NewEvent", menuName = "Events/EventDefinition")]
public class EventDefinition : ScriptableObject
{
    public string EventName;
    public Sprite Icon;
    [TextArea] public string Description;

    public float Weight = 1f;
    public int MaxOccurrences = -1; // -1 = infinite

    public EventChoice[] Choices;
    public string[] RequiredTags;
}

[System.Serializable]
public class EventChoice
{
    public string choiceText;
    [TextArea] public string consequenceDescription;
}