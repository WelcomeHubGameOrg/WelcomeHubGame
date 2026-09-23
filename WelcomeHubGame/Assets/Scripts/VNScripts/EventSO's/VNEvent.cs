using UnityEngine;

[CreateAssetMenu(menuName = "VNEvent/New Visual Novel Event")]
public class VNEvent : ScriptableObject
{
    public string eventName;

    [TextArea(2, 4)]
    public string[] paragraphs;

    [TextArea(1, 2)]
    public string[] answers;

    public Sprite eventSprite;

    public VNEvent[] branches;
}
