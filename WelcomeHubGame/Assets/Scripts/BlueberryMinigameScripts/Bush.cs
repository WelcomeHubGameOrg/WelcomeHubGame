using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour
{
    [Header("Capacity")]
    public int minCapacity = 6;
    public int maxCapacity = 12;
    [Range(0f, 1f)] public float badChance = 0.2f; // ~20% bad/inedible berries

    [Header("Regrowth")]
    public float regenInterval = 4f; // seconds between new berries appearing (while the bush's UI window is closed)

    private int capacity;
    private float regenTimer;
    private bool uiOpen;

    private List<bool> berries = new List<bool>(); // true = bad
    private bool playerNearby = false;

    void Start()
    {
        capacity = Random.Range(minCapacity, maxCapacity + 1);

        // Start roughly half-full so there's already something to collect
        int startCount = Mathf.CeilToInt(capacity * 0.5f);
        for (int i = 0; i < startCount; i++)
            berries.Add(Random.value < badChance);

        UpdateVisual();
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (berries.Count > 0 && BushUIWindow.Instance != null)
            {
                BushUIWindow.Instance.OpenBushUI(this, new List<bool>(berries));
            }
        }

        // Gradual regrowth — paused while this bush's UI window is open
        if (!uiOpen && berries.Count < capacity)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenInterval)
            {
                regenTimer = 0f;
                berries.Add(Random.value < badChance);
                UpdateVisual();
            }
        }
    }

    public void SetUIOpen(bool open)
    {
        uiOpen = open;
    }

    public void ConsumeBerryUI(bool isBad)
    {
        if (isBad)
        {
            GameManager.Instance.AddBadBerryPenalty(); // +5 to the target
        }
        else
        {
            GameManager.Instance.AddGoodBerry(); // +1 to the score
        }

        // Remove one berry of the matching type — which exact index doesn't matter,
        // this avoids index desync if the list changed while the window was open.
        berries.Remove(isBad);

        UpdateVisual();
    }

    public int BerryCount => berries.Count;

    private void UpdateVisual()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr) sr.color = berries.Count > 0 ? Color.white : new Color(1f, 1f, 1f, 0.35f);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (BushUIWindow.Instance != null) BushUIWindow.Instance.CloseBushUI();
        }
    }
}
