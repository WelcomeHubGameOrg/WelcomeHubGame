using System.Collections.Generic;
using UnityEngine;

public class Bush : MonoBehaviour
{
    [Header("Capacity")]
    public int minCapacity = 6;
    public int maxCapacity = 12;
    [Range(0f, 1f)] public float badChance = 0.2f; // ~20% bad/inedible berries

    [Header("Bad Berry Cap")]
    // Without a cap, bad berries can pile up forever: the player rationally avoids
    // clicking them (they're a penalty, not score), so they never get removed, while
    // regrowth stops once the bush hits capacity -- a bush could end up ALL bad and
    // stay stuck that way. This caps how many bad berries can exist in one bush at a
    // time (roughly a third of its capacity), guaranteeing there's always room left for
    // good ones to spawn.
    [Range(0.1f, 0.6f)] public float maxBadFraction = 0.34f;

    [Header("Regrowth")]
    public float regenInterval = 4f; // seconds between new berries appearing (while the bush's UI window is closed)
    public float unavailableAfterLeaving = 10f; // seconds the bush can't be opened again after you leave it

    private int capacity;
    private int maxBad; // cap on concurrent bad berries, recomputed per capacity (see Bad Berry Cap above)
    private float regenTimer;
    private bool uiOpen;
    private float unavailableUntil = 0f;

    private List<bool> berries = new List<bool>(); // true = bad
    private bool playerNearby = false;

    void Start()
    {
        // Start roughly half-full so there's already something to collect
        RegrowFromScratch();
    }

    void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (uiOpen)
            {
                // E toggles the window closed again (in addition to the Close button and walking away)
                if (BushUIWindow.Instance != null) BushUIWindow.Instance.CloseBushUI();
            }
            else if (Time.time >= unavailableUntil && berries.Count > 0 && BushUIWindow.Instance != null)
            {
                // Ignored entirely while the bush is still on cooldown after being left
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
                berries.Add(RollNewBerry());
                UpdateVisual();
            }
        }
    }

    public void SetUIOpen(bool open)
    {
        uiOpen = open;
        if (open) return;

        // Whenever the player leaves — whether some berries are left (usually the bad ones)
        // or the bush was fully cleared out — all of its berries get completely replaced,
        // and the bush can't be opened again for unavailableAfterLeaving seconds.
        RegrowFromScratch();
        unavailableUntil = Time.time + unavailableAfterLeaving;
    }

    // Public entry point for LevelManager/GameManager to force a completely fresh bush
    // (new berries, no cooldown) whenever a level (re)starts. SetActive(false)/(true) does
    // NOT re-run Start(), so without this a retried or revisited level would keep whatever
    // berries/cooldown the bush had from the previous attempt.
    public void ResetBush()
    {
        uiOpen = false;
        unavailableUntil = 0f;
        RegrowFromScratch();
    }

    private void RegrowFromScratch()
    {
        berries.Clear();

        capacity = Random.Range(minCapacity, maxCapacity + 1);
        maxBad = Mathf.Max(1, Mathf.CeilToInt(capacity * maxBadFraction));

        int startCount = Mathf.CeilToInt(capacity * 0.5f);
        for (int i = 0; i < startCount; i++)
            berries.Add(RollNewBerry());

        regenTimer = 0f;
        UpdateVisual();
    }

    // Rolls whether a newly-added berry is bad, respecting the maxBad cap so a bush can
    // never accumulate more bad berries than that regardless of luck or how long it sits.
    private bool RollNewBerry()
    {
        if (CountBad() >= maxBad) return false; // cap reached -- force this one good
        return Random.value < badChance;
    }

    private int CountBad()
    {
        int count = 0;
        for (int i = 0; i < berries.Count; i++)
            if (berries[i]) count++;
        return count;
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
