using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpawnManager : MonoBehaviour
{
    [SerializeField] List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] List<GameObject> prefabs = new List<GameObject>();

    private void Start()
    {
        foreach (Transform spawnPoint in spawnPoints)
        {
            Instantiate(prefabs[Random.Range(0, prefabs.Count)], spawnPoint.transform.position, spawnPoint.transform.rotation);
        }
    }
}
