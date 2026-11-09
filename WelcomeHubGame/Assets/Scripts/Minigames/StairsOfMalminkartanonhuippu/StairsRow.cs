using UnityEngine;

public class StairsRow : MonoBehaviour
{
    public GameObject obstacle;

    private Vector3 target;
    private float speed;
    private bool sliding;
    private bool destroyWhenDone;

    public void SetObstacle(GameObject newObstacle)
    {
        obstacle = newObstacle;
    }

    // Slides the row to a new position in `duration` seconds (a short, smooth step instead of a teleport).
    public void SlideTo(Vector3 newTarget, float duration, bool destroyAtEnd)
    {
        target = newTarget;
        destroyWhenDone = destroyAtEnd;
        sliding = true;

        float distance = Vector3.Distance(transform.position, target);
        speed = duration > 0f ? distance / duration : float.MaxValue;
    }

    void Update()
    {
        if (!sliding)
        {
            return;
        }

        transform.position = Vector3.MoveTowards(transform.position, target, speed * Time.deltaTime);

        if ((transform.position - target).sqrMagnitude < 0.0001f)
        {
            transform.position = target;
            sliding = false;

            if (destroyWhenDone)
            {
                Destroy(gameObject);
            }
        }
    }

    public void DestroyRow()
    {
        Destroy(gameObject);
    }
}