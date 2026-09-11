using UnityEngine;

public class CameraSetup : MonoBehaviour
{
    void Awake()
    {
        var cam = GetComponent<Camera>();

        cam.transparencySortMode = TransparencySortMode.CustomAxis;
        cam.transparencySortAxis = Vector3.up;
    }
}
