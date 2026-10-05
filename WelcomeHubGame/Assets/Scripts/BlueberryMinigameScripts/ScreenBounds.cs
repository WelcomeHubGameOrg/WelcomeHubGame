using UnityEngine;

// Invisible walls exactly on the edges of what the camera shows, so the player can never leave the
// visible area. Works with perspective and orthographic cameras and follows resolution / aspect
// changes. Created automatically next to the HUD (see GameHud.Ensure) -- nothing to set up in the scene.
public class ScreenBounds : MonoBehaviour
{
    private const float Thickness = 4f; // thick so fast bodies can't tunnel through

    private BoxCollider2D left, right, top, bottom;
    private Camera cam;
    private Vector2 lastMin = new Vector2(float.NaN, float.NaN), lastMax;

    private void Awake()
    {
        PhysicsMaterial2D slick = new PhysicsMaterial2D("ScreenWall");
        slick.friction = 0f;     // no sticking to the wall while falling / pushing against it
        slick.bounciness = 0f;

        left = MakeWall("ScreenWall_Left", slick);
        right = MakeWall("ScreenWall_Right", slick);
        top = MakeWall("ScreenWall_Top", slick);
        bottom = MakeWall("ScreenWall_Bottom", slick);
        Refresh();
    }

    private BoxCollider2D MakeWall(string wallName, PhysicsMaterial2D material)
    {
        GameObject go = new GameObject(wallName); // default layer, so it never counts as "ground" for jumping
        go.transform.SetParent(transform, false);
        BoxCollider2D box = go.AddComponent<BoxCollider2D>();
        box.sharedMaterial = material;
        return box;
    }

    private void LateUpdate()
    {
        Refresh();
    }

    private void Refresh()
    {
        if (cam == null) cam = Camera.main;
        if (cam == null) return;

        // Where the screen corners land on the z = 0 plane (the plane the game is played on)
        Plane plane = new Plane(Vector3.forward, Vector3.zero);
        if (!Hit(plane, new Vector2(0f, 0f), out Vector3 bl) || !Hit(plane, new Vector2(1f, 1f), out Vector3 tr)) return;

        Vector2 min = new Vector2(Mathf.Min(bl.x, tr.x), Mathf.Min(bl.y, tr.y));
        Vector2 max = new Vector2(Mathf.Max(bl.x, tr.x), Mathf.Max(bl.y, tr.y));
        if ((min - lastMin).sqrMagnitude < 0.0001f && (max - lastMax).sqrMagnitude < 0.0001f) return;
        lastMin = min;
        lastMax = max;

        float w = max.x - min.x, h = max.y - min.y;
        float cx = (min.x + max.x) * 0.5f, cy = (min.y + max.y) * 0.5f;

        SetWall(left,   new Vector2(min.x - Thickness * 0.5f, cy), new Vector2(Thickness, h + Thickness * 2f));
        SetWall(right,  new Vector2(max.x + Thickness * 0.5f, cy), new Vector2(Thickness, h + Thickness * 2f));
        SetWall(top,    new Vector2(cx, max.y + Thickness * 0.5f), new Vector2(w + Thickness * 2f, Thickness));
        SetWall(bottom, new Vector2(cx, min.y - Thickness * 0.5f), new Vector2(w + Thickness * 2f, Thickness));
    }

    private bool Hit(Plane plane, Vector2 viewport, out Vector3 point)
    {
        Ray ray = cam.ViewportPointToRay(new Vector3(viewport.x, viewport.y, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }
        point = Vector3.zero;
        return false;
    }

    private static void SetWall(BoxCollider2D box, Vector2 center, Vector2 size)
    {
        box.transform.position = new Vector3(center.x, center.y, 0f);
        box.size = size;
        box.offset = Vector2.zero;
    }
}
