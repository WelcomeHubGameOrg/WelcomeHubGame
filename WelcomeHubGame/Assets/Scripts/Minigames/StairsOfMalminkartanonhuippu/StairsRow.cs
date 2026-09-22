using UnityEngine;

public class StairsRow : MonoBehaviour
{
    public GameObject obstacle;

    public void SetObstacle(GameObject newObstacle)
    {
        obstacle = newObstacle;
    }

    public void DestroyRow()
    {
        Destroy(gameObject);
    }
}
