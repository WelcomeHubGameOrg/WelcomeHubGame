using UnityEngine;
using UnityEngine.Serialization;

namespace HikingMinigame.Events
{
    public class EventObjectMover : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("scrollSpeed")] private float _scrollSpeed;
        [SerializeField, FormerlySerializedAs("dialogueManager")] private DialogueManager _dialogue;

        private float _camLeftEdge;
        private EventDefinition _eventDefinition;

        private void Start()
        {
            CacheCameraLeftEdge();
        }

        private void Update()
        {
            transform.Translate(Vector2.left * (_scrollSpeed * Time.deltaTime));

            if (transform.position.x < _camLeftEdge)
                gameObject.SetActive(false);
        }

        public void ResetAndActivate(Vector3 spawnPosition, float speed, EventDefinition definition)
        {
            transform.position = spawnPosition;
            _scrollSpeed = speed;
            GetComponent<SpriteRenderer>().sprite = definition.Icon;

            _eventDefinition = definition;
            gameObject.SetActive(true);
        }

        private void CacheCameraLeftEdge()
        {
            var cam = Camera.main;
            _camLeftEdge = cam.transform.position.x - cam.orthographicSize * cam.aspect;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (!other.CompareTag("Player"))
                return;

            _dialogue.ShowEvent(_eventDefinition);
            gameObject.SetActive(false);
        }
    }
}
