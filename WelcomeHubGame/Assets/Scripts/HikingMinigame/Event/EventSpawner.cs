using UnityEngine;
using System.Collections;
using Event;

public class EventSpawner : MonoBehaviour
{
    public int eventMinIntervalSec;
    public int eventMaxIntervalSec;
    
    public EventCatalog catalog;
    public EventObjectMover eventObject;
    
    public Vector3 spawnPosition;
    public float roadSpeed;

    private void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    private int GetNextInterval()
    {
        return Random.Range(eventMinIntervalSec, eventMaxIntervalSec);
    }

    private IEnumerator SpawnLoop()
    {
        while (true)
        {
            var waitTime = GetNextInterval();
            yield return new WaitForSeconds(waitTime);

            if (!eventObject.gameObject.activeSelf)
                SpawnEvent();
        }
    }

    private void SpawnEvent()
    {
        var definition = catalog.GetRandomEvent();
        eventObject.ResetAndActivate(spawnPosition, roadSpeed, definition);
    }
}