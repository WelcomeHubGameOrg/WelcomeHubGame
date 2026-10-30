using UnityEngine;

// A simple platform that moves back and forth between two world points.
// Put it on the "Ground" layer so the player's ground check detects it,
// give it a Kinematic Rigidbody2D + BoxCollider2D (not a trigger), and set
// pointA / pointB to where it should travel between.
[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    [Header("Travel")]
    public Vector2 pointA;
    public Vector2 pointB;
    public float speed = 1.5f;

    private Rigidbody2D rb;
    private Vector2 target;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        target = pointB;
    }

    void FixedUpdate()
    {
        Vector2 newPos = Vector2.MoveTowards(rb.position, target, speed * Time.fixedDeltaTime);
        rb.MovePosition(newPos);

        if (Vector2.Distance(newPos, target) < 0.05f)
            target = (target == pointB) ? pointA : pointB;
    }
}
