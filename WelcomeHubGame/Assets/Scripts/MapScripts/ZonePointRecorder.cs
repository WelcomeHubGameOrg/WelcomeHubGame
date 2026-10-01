using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Temp dev tool. Attach this to the same gameObject that holds the background Image
/// (+ needs "Raycast Target" checked so the clicks actually register)
///
/// In Play mode:
/// - Click each vertex around a zone's outline, in order. Doesn't matter if clockwise or counter, just pick one
/// - Press P to print a ready-to-paste List<Vector2> to console
/// - Press Backspace to clear and start the next zone
///
/// Remove or disable this component once the zones are set up
/// </summary>
public class ZonePointRecorder : MonoBehaviour, IPointerClickHandler
{
    [Tooltip("The same RectTransform the buttons use for anchoredPosition (mapCanvasTransform)")]
    public RectTransform mapRect;

    [Tooltip("Leave null if Canvas Render Mode is Screen Space - Overlay")]
    public Camera uiCamera;

    public KeyCode printKey = KeyCode.P;
    public KeyCode clearKey = KeyCode.Backspace;
    
    private readonly List<Vector2> _recordedPoints = new List<Vector2>();

    public void OnPointerClick(PointerEventData eventData)
    {
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(mapRect, eventData.position, uiCamera,
                out Vector2 localPoint))
        {
            _recordedPoints.Add(localPoint);
            Debug.Log($"Point {_recordedPoints.Count}: ({localPoint.x}, {localPoint.y})");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(printKey)) PrintPoints();
        else if (Input.GetKeyDown(clearKey))
        {
            _recordedPoints.Clear();
            Debug.Log("Cleared recorded points");
        }
    }

    private void PrintPoints()
    {
        var sb = new StringBuilder();
        sb.AppendLine("Paste this into the zone's Points list:");
        foreach (var p in _recordedPoints)
            sb.AppendLine($"({p.x:F1}, {p.y:F1})");
        
        Debug.Log(sb.ToString());
    }
}
