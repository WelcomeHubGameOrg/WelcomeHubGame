using UnityEngine;

namespace HikingMinigame
{
    public class ParallaxObject : MonoBehaviour
    {
        public float scrollSpeed;

        public SpriteRenderer SpriteRenderer { get; private set; }

        private void Awake()
        {
            SpriteRenderer = GetComponent<SpriteRenderer>();
        }

        private void Update()
        {
            var moveAmount = -scrollSpeed * Time.deltaTime;
            transform.position += new Vector3(moveAmount, 0f, 0f);
        }
    }
}
