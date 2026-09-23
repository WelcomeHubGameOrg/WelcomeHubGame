using UnityEngine;
using UnityEngine.UI;

public class EventStarter : MonoBehaviour, IEventCall
{
    [SerializeField] private GameObject eventCanvas;
    [SerializeField] private VNEvent nextEvent;

    // testing purposes
    public TextEvent textEvent;
    public Image eventImage;

    public void StartEvent()
    {
        textEvent.vNEvent = nextEvent;
        eventImage.sprite = nextEvent.eventSprite;

        eventCanvas.SetActive(true);
    }
}
