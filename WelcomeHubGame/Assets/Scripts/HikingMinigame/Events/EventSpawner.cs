using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace HikingMinigame.Events
{
    public class EventSpawner : MonoBehaviour
    {
        [SerializeField] private float _minGapSeconds = 1.5f;
        [SerializeField] private float _maxGapSeconds = 4f;
        [SerializeField, FormerlySerializedAs("catalog")] private EventCatalog _catalog;
        [SerializeField, FormerlySerializedAs("eventObject")] private EventObjectMover _eventObject;
        [SerializeField] private DialogueManager _dialogue;
        [SerializeField, FormerlySerializedAs("spawnPosition")] private Vector3 _spawnPosition;
        [SerializeField, FormerlySerializedAs("roadSpeed")] private float _roadSpeed;

        private EventPicker _picker;

        private bool IsRoadFree => !_eventObject.gameObject.activeSelf;

        private void Start()
        {
            _picker = new EventPicker(_catalog.RandomEvents);

            var startEvent = _catalog.StartEvent;
            if (startEvent != null)
                _dialogue.ShowEvent(startEvent);

            StartCoroutine(SpawnLoop());
        }

        private IEnumerator SpawnLoop()
        {
            while (true)
            {
                yield return new WaitUntil(() => IsRoadFree);
                yield return new WaitForSeconds(Random.Range(_minGapSeconds, _maxGapSeconds));

                if (!_picker.TryPick(out var next))
                    yield break;

                _eventObject.ResetAndActivate(_spawnPosition, _roadSpeed, next);
            }
        }
    }
}
