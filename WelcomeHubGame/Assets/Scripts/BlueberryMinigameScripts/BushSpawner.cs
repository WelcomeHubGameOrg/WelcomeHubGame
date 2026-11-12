using System.Collections.Generic;
using UnityEngine;

public class BushSpawner : MonoBehaviour
{
    [Header("Prefab")]
    public GameObject bushPrefab;

    [Header("Count")]
    public int bushCount = 6;

    [Header("Spawn Zone (X axis, world coordinates)")]
    public float minX = -8f;
    public float maxX = 8f;

    [Header("Height")]
    public float groundY = 0f;      // Y of the ground's top surface
    public float bushYOffset = 0.5f; // how far above that surface the bush sits (usually half the bush sprite's height)

    [Header("Minimum Spacing Between Bushes")]
    public float minSpacing = 2.5f;

    void Start()
    {
        SpawnBushes();
    }

    void SpawnBushes()
    {
        if (bushPrefab == null)
        {
            Debug.LogError($"[{nameof(BushSpawner)}] Bush Prefab is not assigned in the Inspector.", this);
            return;
        }

        List<float> usedX = new List<float>();
        int attempts = 0;
        int spawned = 0;

        while (spawned < bushCount && attempts < bushCount * 30)
        {
            attempts++;
            float x = Random.Range(minX, maxX);

            bool tooClose = false;
            foreach (float used in usedX)
            {
                if (Mathf.Abs(used - x) < minSpacing)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;

            usedX.Add(x);
            Vector3 pos = new Vector3(x, groundY + bushYOffset, 0f);
            Instantiate(bushPrefab, pos, Quaternion.identity);
            spawned++;
        }

        if (spawned < bushCount)
        {
            Debug.LogWarning($"[{nameof(BushSpawner)}] Only managed to place {spawned} of {bushCount} bushes " +
                              "— try increasing the minX/maxX range or reducing minSpacing.", this);
        }
    }
}
