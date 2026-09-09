using UnityEngine;


[CreateAssetMenu(menuName = "EventText/NewEventTextContainer")]
public class WrittenText : ScriptableObject
{
    public string eventName;

    [TextArea(2, 3)]
    public string[] paragraphs;
}
