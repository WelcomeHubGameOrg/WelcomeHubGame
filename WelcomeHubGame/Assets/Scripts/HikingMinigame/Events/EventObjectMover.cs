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
        private SpriteRenderer _spriteRenderer;

        private bool IsFullyOffScreen => _spriteRenderer.bounds.max.x < _camLeftEdge;

        private void Awake()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            CacheCameraLeftEdge();
        }

        private void Update()
        {
            transform.Translate(Vector2.left * (_scrollSpeed * Time.deltaTime));

            if (IsFullyOffScreen)
                gameObject.SetActive(false);
        }

        public void ResetAndActivate(Vector3 trailPosition, float heightAboveTrail, float speed, EventDefinition definition)
        {
            transform.position = trailPosition + Vector3.up * heightAboveTrail;
            KeepTriggerOnTrail(heightAboveTrail);
            _scrollSpeed = speed;
            GetComponent<SpriteRenderer>().sprite = definition.Icon;

            _eventDefinition = definition;
            gameObject.SetActive(true);
        }

        private void KeepTriggerOnTrail(float heightAboveTrail)
        {
            GetComponent<Collider2D>().offset = Vector2.down * (heightAboveTrail / transform.lossyScale.y);
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
