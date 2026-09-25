using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// Defines one map region (A, B, C, D) as a polygon in the SAME coordinate space that you use for minBounds/maxBounds
/// and anchoredPosition (canvas-local units)
/// </summary>
[System.Serializable]
public class MapZone
{
    // A, B, C, or D only, don't use Any
    public MapZoneType zoneId;
    public List<Vector2> points;

    private Vector2 _min, _max;
    private bool _boundsCached;
    
    public Vector2 Min { get { CacheBounds(); return _min; } }
    public Vector2 Max { get { CacheBounds(); return _max; } }

    private void CacheBounds()
    {
        if (_boundsCached || points == null || points.Count == 0) return;
        
        _min = points[0];
        _max = points[0];
        foreach (var p in points)
        {
            _min = Vector2.Min(_min, p);
            _max = Vector2.Max(_max, p);
        }
        _boundsCached = true;
    }

    public bool Contains(Vector2 point)
    {
        if (points == null || points.Count < 3) return false;

        bool inside = false;
        int j = points.Count - 1;
        for (int i = 0; i < points.Count; i++)
        {
            Vector2 pi = points[i];
            Vector2 pj = points[j];

            bool intersects = (pi.y > point.y) != (pj.y > point.y) &&
                              (point.x < (pj.x - pi.x) * (point.y - pi.y) / (pj.y - pi.y) + pi.x);

            if (intersects) inside = !inside;
            j = i;
        }
        return inside;
    }
}
