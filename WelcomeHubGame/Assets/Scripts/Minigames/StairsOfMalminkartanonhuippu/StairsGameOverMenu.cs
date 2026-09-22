using UnityEngine;
using UnityEngine.SceneManagement;

public class StairsGameOverMenu : MonoBehaviour
{
    public void TryAgain()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
