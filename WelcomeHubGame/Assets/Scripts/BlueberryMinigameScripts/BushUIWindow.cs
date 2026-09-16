using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BushUIWindow : MonoBehaviour
{
    public static BushUIWindow Instance;

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform berryContainer;
    [SerializeField] private GameObject berryButtonPrefab;
    [SerializeField] private Button closeButton;

    [Header("Colors")]
    [SerializeField] private Color goodColor = new Color(0.2f, 0.4f, 1f); // ripe (safe to collect)
    [SerializeField] private Color badColor = new Color(0.8f, 0.2f, 0.2f);   // unripe/bad (penalty)

    [Header("Labels")]
    [SerializeField] private string goodLabel = "Good berry";
    [SerializeField] private string badLabel = "Bad berry";

    private Bush currentBushRef;

    private void Awake()
    {
        Instance = this;
        if (panelRoot) panelRoot.SetActive(false);
        if (closeButton) closeButton.onClick.AddListener(CloseBushUI);
    }

    public void OpenBushUI(Bush bush, List<bool> berriesData)
    {
        currentBushRef = bush;
        currentBushRef.SetUIOpen(true);
        if (panelRoot) panelRoot.SetActive(true);

        // Lock player movement while the berry panel is open
        PlayerMovement.InputEnabled = false;

        foreach (Transform child in berryContainer)
        {
            Destroy(child.gameObject);
        }

        foreach (bool isBad in berriesData)
        {
            GameObject btnObj = Instantiate(berryButtonPrefab, berryContainer);
            Button btn = btnObj.GetComponent<Button>();
            Image img = btnObj.GetComponent<Image>();
            TMP_Text label = btnObj.GetComponentInChildren<TMP_Text>();

            if (img) img.color = isBad ? badColor : goodColor;
            if (label) label.text = isBad ? badLabel : goodLabel;

            btn.onClick.AddListener(() => OnBerryClicked(isBad, btnObj));
        }
    }

    private void OnBerryClicked(bool isBad, GameObject btnObj)
    {
        if (currentBushRef == null) return;
        currentBushRef.ConsumeBerryUI(isBad);
        Destroy(btnObj);

        if (currentBushRef.BerryCount <= 0)
        {
            CloseBushUI();
        }
    }

    public void CloseBushUI()
    {
        if (panelRoot) panelRoot.SetActive(false);
        if (currentBushRef != null) currentBushRef.SetUIOpen(false);
        currentBushRef = null;

        // Re-enable player movement once the berry panel closes
        PlayerMovement.InputEnabled = true;
    }
}
