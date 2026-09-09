using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TavelScript : MonoBehaviour
{
    [SerializeField] private int cost;
    [SerializeField] private int reward;
    [SerializeField] private int zone;

    public void ShowPopUp(GameObject popUp)
    {
        if (popUp.activeSelf)
            popUp.SetActive(false);
        else
            popUp.SetActive(true);
    }

    public void TravelToMinigameScene(string sceneName)
    {
        Debug.Log("minus cost: " + cost);
        Debug.Log("plus reward: " + reward);
        SceneManager.LoadScene(sceneName);
    }


    // == should i put on a separete script? ==
    public void BackToMap()
    {
        SceneManager.LoadScene("MapProtype");
    }
}
