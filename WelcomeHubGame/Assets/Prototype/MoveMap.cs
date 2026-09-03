using UnityEngine;
using UnityEngine.EventSystems;

public class MoveMap : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 offset;

    public float zoomSpeed = 0.1f;
    public float minZoom = 0.5f;
    public float maxZoom = 3f;

    private RectTransform map;

    private void Awake()
    {
        map = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        offset = transform.position - (Vector3)eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = (Vector3)eventData.position + offset;
        ClampPosition();
    }

    public void OnEndDrag(PointerEventData eventData)
    {

    }

    private void Update()
    {
        float scroll = Input.mouseScrollDelta.y;

        if (scroll != 0)
        {
            float zoom = transform.localScale.x + scroll * zoomSpeed;
            zoom = Mathf.Clamp(zoom, minZoom, maxZoom);

            transform.localScale = new Vector3(zoom, zoom, 1);

            ClampPosition();
        }
    }

    private void ClampPosition()
    {
        Vector3[] corners = new Vector3[4];
        map.GetWorldCorners(corners);

        float left = corners[0].x;
        float right = corners[2].x;
        float bottom = corners[0].y;
        float top = corners[2].y;

        float cameraWidth = Screen.width;
        float cameraHeight = Screen.height;

        Vector3 position = transform.position;

        // Prevent left edge from going inside the screen
        if (left > 0)
            position.x -= left;

        // Prevent right edge from going inside the screen
        if (right < cameraWidth)
            position.x += cameraWidth - right;

        // Prevent bottom edge from going inside the screen
        if (bottom > 0)
            position.y -= bottom;

        // Prevent top edge from going inside the screen
        if (top < cameraHeight)
            position.y += cameraHeight - top;

        transform.position = position;
    }
}