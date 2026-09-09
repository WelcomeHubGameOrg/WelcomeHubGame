using UnityEngine;

public class EventObjectMover : MonoBehaviour
{
    public float scrollSpeed;
    public DialogueManager dialogueManager;
    
    private Camera _cam;
    private float _camLeftEdge;

    private EventDefinition _eventDefinition;
    
    void Start()
    {
        CacheCameraLeftEdge();
    }
    
    void Update()
    {
        transform.Translate(Vector2.left * (scrollSpeed * Time.deltaTime));

        if (transform.position.x < _camLeftEdge)
        {
            gameObject.SetActive(false);
        }
    }
    
    public void ResetAndActivate(Vector3 spawnPosition, float speed, EventDefinition definition)
    {
        transform.position = spawnPosition;
        scrollSpeed = speed;
        GetComponent<SpriteRenderer>().sprite = definition.Icon;
        
        _eventDefinition = definition;
        gameObject.SetActive(true);
    }
    
    private void CacheCameraLeftEdge()
    {
        _cam = Camera.main;
        _camLeftEdge = _cam!.transform.position.x - _cam.orthographicSize * _cam.aspect;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        dialogueManager.ShowEvent(_eventDefinition);
        gameObject.SetActive(false);
    }
}
