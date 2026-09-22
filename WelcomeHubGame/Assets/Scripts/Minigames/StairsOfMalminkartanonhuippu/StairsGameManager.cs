using UnityEngine;
using TMPro;

public class StairsGameManager : MonoBehaviour
{
    private StairsRow[] rows = new StairsRow[5];
    private int score = 0;
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

        for (int column = 0; column < 2; column++)
        {
            GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Cube);
            cell.transform.SetParent(rowObject.transform);
            cell.transform.localPosition = new Vector3(column - 0.5f, 0, 0);
        }

        if (rowNumber != 4)
        {
            int obstaclePosition = Random.Range(0, 3);

            if (obstaclePosition == 1)
            {
                CreateObstacle(row, 0);
            }
            else if (obstaclePosition == 2)
            {
                CreateObstacle(row, 1);
            }
        }
        
        rows[rowNumber] = row;
    }

    void CreateObstacle(StairsRow row, int column)
    {
        GameObject obstacle = Instantiate(obstaclePrefab);
        obstacle.transform.SetParent(row.transform);
        obstacle.transform.localPosition = new Vector3(column - 0.5f, 0, -1);
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
        return rows[4].obstacle != null && Mathf.RoundToInt(rows[4].obstacle.transform.localPosition.x + 0.5f) == column;
    }

    public void AddScore()
    {
        score++;
    }

    public int GetScore()
    {
        return score;
    }

    public void GameOver()
    {
        scoreText.text = "Score: " + score;
        gameOverScreen.SetActive(true);
        Time.timeScale = 0;
    }
}
