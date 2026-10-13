using UnityEngine;
using TMPro;

public class StairsGameManager : MonoBehaviour
{
    private StairsRow[] rows = new StairsRow[5];
    private int score = 0;
    private const int MaxScore = 347;
    public bool GameEnded { get; private set; }
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject gameOverScreen;
    [SerializeField] private TMP_Text scoreText;

    public void StartGame()
    {
        CreateBoard();
    }

    void CreateBoard()
    {
        for (int row = 0; row < 5; row++)
        {
            CreateRow(row);
        }
    }

    void CreateRow(int rowNumber)
    {
        GameObject rowObject = new GameObject("Row " + rowNumber);
        StairsRow row = rowObject.AddComponent<StairsRow>();
        rowObject.transform.position = new Vector3(0, 2 - rowNumber, 0);

        for (int column = 0; column < 3; column++)
        {
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.transform.SetParent(rowObject.transform);
            cell.transform.localPosition = new Vector3(column - 1, 0, 0);
        }

        if (rowNumber != 4)
        {
            float obstacleChange = GetObstacleChange();

            if (Random.value < obstacleChange)
            {
                int obstacleColumn = Random.Range(0, 3);
                CreateObstacle(row, obstacleColumn);
            }
        }
        
        rows[rowNumber] = row;
    }

    float GetObstacleChange()
    {
        switch (score)
        {
            case < 50:
                return 0.30f;
            case < 100:
                return 0.40f;
            case < 150:
                return 0.50f;
            case < 200:
                return 0.60f;
            case < 250:
                return 0.70f;
            case < 300:
                return 0.80f;
            default:
                return 0.90f;
        }
    }

    void CreateObstacle(StairsRow row, int column)
    {
        GameObject obstacle = Instantiate(obstaclePrefab);
        obstacle.transform.SetParent(row.transform);
        obstacle.transform.localPosition = new Vector3(column - 1, 0, -1);
        row.SetObstacle(obstacle);
    }

    public void MoveBoard()
    {
        rows[4].DestroyRow();

        for (int row = 4; row > 0; row--)
        {
            rows[row] = rows[row - 1];

            rows[row].transform.position = new Vector3(0, 2 - row, 0);
        }
        CreateRow(0);
    }

    public bool BottomRowHasObstacle(int column)
    {
        return rows[4].obstacle != null && Mathf.RoundToInt(rows[4].obstacle.transform.localPosition.x + 1) == column;
    }

    public void AddScore()
    {
        score++;

        if (score >= MaxScore) {
            GameOver();
        }
    }

    public int GetScore()
    {
        return score;
    }

    public void GameOver()
    {
        GameEnded = true;
        scoreText.text = "Score: " + score;
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }
}
