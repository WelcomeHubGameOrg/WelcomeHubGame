using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapButtonView : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Button Visuals")]
    public Image iconImage;

    [Header("Hover Panel")]
    public GameObject infoPanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;

    private void Awake()
    {
        if (infoPanel != null) infoPanel.SetActive(false);
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
}