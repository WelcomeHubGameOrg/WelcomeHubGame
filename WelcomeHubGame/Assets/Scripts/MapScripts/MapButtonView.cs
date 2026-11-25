using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private MinigameData stationaryMinigame;

    [Header("Button Visuals")]
    public Image iconImage;

    [Header("Hover Panel")]
    public GameObject infoPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    private void Awake()
    {
        if (infoPanel != null) 
            infoPanel.SetActive(false);

        if (stationaryMinigame == null)
            return;

        Bind(stationaryMinigame);

        if (stationaryMinigame.scene != null && TryGetComponent<Button>(out var button))
            button.onClick.AddListener(TravelToStationaryMinigame);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (infoPanel == null) return;

        transform.SetAsLastSibling();
        infoPanel.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (infoPanel != null) infoPanel.SetActive(false);
    }
    public void Bind(MinigameData data)
    {
        if (data == null)
            return;

        if (iconImage != null && data.icon != null)
            iconImage.sprite = data.icon;

        if (titleText != null)
            titleText.text = data.displayName;

        if (descriptionText != null)
            descriptionText.text = data.description;
    }
    private void TravelToStationaryMinigame()
    {
        var travelManager = TravelManager.instance;
        if (travelManager == null)
            return;

        travelManager.MoveTo(GetComponent<RectTransform>(), stationaryMinigame);
    }
}