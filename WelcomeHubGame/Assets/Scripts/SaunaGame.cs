using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SaunaGame : MonoBehaviour
{
    [Header("Game Objects")]
    public Transform bucket;
    public Transform pot;

    [Header("Bucket Movement")]
    public float moveSpeed = 3f;
    public float leftLimit = -6f;
    public float rightLimit = 6f;

    [Header("Game Settings")]
    public int requiredPours = 10;

    [Header("Water")]
    public float waterFallSpeed = 5f;

    private int direction = 1;
    private int successfulPours = 0;
    private bool gameOver = false;
    private bool gameWon = false;

    private Text gameText;

    void Start()
    {
        CreateUI();
    }

    void Update()
    {
        if (gameOver || gameWon)
            return;

        MoveBucket();

        if (Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame)
        {
            PourWater();
        }
    }

    void MoveBucket()
    {
        bucket.position +=
            Vector3.right * direction * moveSpeed * Time.deltaTime;

        if (bucket.position.x >= rightLimit)
        {
            direction = -1;
        }

        if (bucket.position.x <= leftLimit)
        {
            direction = 1;
        }
    }

    void PourWater()
    {
        Vector3 mousePosition =
            Camera.main.ScreenToWorldPoint(
                Mouse.current.position.ReadValue()
            );

        mousePosition.z = 0f;

        SpriteRenderer bucketSprite =
            bucket.GetComponent<SpriteRenderer>();

        bool clickedBucket =
            mousePosition.x >= bucketSprite.bounds.min.x &&
            mousePosition.x <= bucketSprite.bounds.max.x &&
            mousePosition.y >= bucketSprite.bounds.min.y &&
            mousePosition.y <= bucketSprite.bounds.max.y;

        if (!clickedBucket)
        {
            return;
        }

        CreateWater();

        SpriteRenderer potSprite =
            pot.GetComponent<SpriteRenderer>();

        float bucketCenterX = bucket.position.x;

        bool waterOverPot =
            bucketCenterX >= potSprite.bounds.min.x &&
            bucketCenterX <= potSprite.bounds.max.x;

        if (waterOverPot)
        {
            SuccessfulPour();
        }
        else
        {
            LoseGame();
        }
    }

    void CreateWater()
    {
        GameObject water = new GameObject("Water");

        SpriteRenderer spriteRenderer =
            water.AddComponent<SpriteRenderer>();

        water.transform.position =
            bucket.position + Vector3.down * 0.5f;

        water.transform.localScale =
            new Vector3(0.6f, 0.6f, 1f);

        spriteRenderer.sprite =
            CreateCircleSprite();

        spriteRenderer.color = Color.blue;

        StartCoroutine(FallWater(water));
    }

    System.Collections.IEnumerator FallWater(GameObject water)
    {
        while (water != null)
        {
            water.transform.position +=
                Vector3.down *
                waterFallSpeed *
                Time.deltaTime;

            if (water.transform.position.y <= -2f)
            {
                Destroy(water);
                yield break;
            }

            yield return null;
        }
    }

    Sprite CreateCircleSprite()
    {
        Texture2D texture = new Texture2D(32, 32);

        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float distance = Vector2.Distance(
                    new Vector2(x, y),
                    new Vector2(15.5f, 15.5f)
                );

                if (distance < 15f)
                    texture.SetPixel(x, y, Color.white);
                else
                    texture.SetPixel(x, y, Color.clear);
            }
        }

        texture.Apply();

        return Sprite.Create(
            texture,
            new Rect(0, 0, 32, 32),
            new Vector2(0.5f, 0.5f)
        );
    }

    void SuccessfulPour()
    {
        successfulPours++;

        gameText.text =
            "Successful pours: " +
            successfulPours +
            " / " +
            requiredPours;

        Debug.Log(
            "SUCCESSFUL POUR: " +
            successfulPours +
            " / " +
            requiredPours
        );

        if (successfulPours >= requiredPours)
        {
            WinGame();
        }
    }

    void LoseGame()
    {
        gameOver = true;

        gameText.text =
            "GAME OVER!\n\n" +
            "OMG I feel like I'm in HELL!!";

        Debug.Log("GAME OVER!");

        CreateSmoke();
    }

    void WinGame()
    {
        gameWon = true;

        gameText.text =
            "YOU WIN!\nSauna temperature reached!";
    }

    void CreateSmoke()
    {
        for (int i = 0; i < 6; i++)
        {
            GameObject smoke = new GameObject("Smoke");

            SpriteRenderer spriteRenderer =
                smoke.AddComponent<SpriteRenderer>();

            float randomX =
                Random.Range(-1.5f, 1.5f);

            float randomY =
                Random.Range(-1.8f, -1.2f);

            smoke.transform.position =
                new Vector3(randomX, randomY, -0.5f);

            float randomSize =
                Random.Range(0.8f, 1.4f);

            smoke.transform.localScale =
                new Vector3(
                    randomSize,
                    randomSize,
                    1f
                );

            spriteRenderer.sprite =
                CreateCircleSprite();

            spriteRenderer.color =
                new Color(0.4f, 0.4f, 0.4f, 0.7f);

            StartCoroutine(RiseSmoke(smoke));
        }
    }

    System.Collections.IEnumerator RiseSmoke(GameObject smoke)
    {
        float lifeTime = 0f;

        while (smoke != null && lifeTime < 3f)
        {
            smoke.transform.position +=
                Vector3.up *
                0.8f *
                Time.deltaTime;

            smoke.transform.localScale +=
                Vector3.one *
                0.2f *
                Time.deltaTime;

            lifeTime += Time.deltaTime;

            yield return null;
        }

        if (smoke != null)
        {
            Destroy(smoke);
        }
    }

    void CreateUI()
    {
        GameObject canvasObject =
            new GameObject("GameCanvas");

        Canvas canvas =
            canvasObject.AddComponent<Canvas>();

        canvas.renderMode =
            RenderMode.ScreenSpaceOverlay;

        canvasObject.AddComponent<CanvasScaler>();
        canvasObject.AddComponent<GraphicRaycaster>();

        GameObject textObject =
            new GameObject("GameText");

        textObject.transform.SetParent(
            canvasObject.transform
        );

        gameText =
            textObject.AddComponent<Text>();

        gameText.text =
            "Successful pours: 0 / " +
            requiredPours;

        gameText.font =
            Resources.GetBuiltinResource<Font>(
                "LegacyRuntime.ttf"
            );

        //Bigger text
        gameText.fontSize = 36;

        gameText.alignment =
            TextAnchor.MiddleCenter;

        gameText.color = Color.white;

        RectTransform rect =
            gameText.GetComponent<RectTransform>();

        rect.anchorMin =
            new Vector2(0.5f, 1f);

        rect.anchorMax =
            new Vector2(0.5f, 1f);

        rect.pivot =
            new Vector2(0.5f, 1f);

        rect.anchoredPosition =
            new Vector2(0, -30);

        //Bigger text box
        rect.sizeDelta =
            new Vector2(800, 200);
    }
}