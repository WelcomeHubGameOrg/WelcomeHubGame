using UnityEngine;

namespace HikingMinigame
{
    public class CameraSetup : MonoBehaviour
    {
        private void Awake()
        {
            var cam = GetComponent<Camera>();

            cam.transparencySortMode = TransparencySortMode.CustomAxis;
            cam.transparencySortAxis = Vector3.up;
        }
    }
}
