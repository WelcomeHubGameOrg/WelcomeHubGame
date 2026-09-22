using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TavelScript : MonoBehaviour
{
    [SerializeField] private int cost;
    [SerializeField] private int reward;
    [SerializeField] private int zone;
    [SerializeField] private string sceneName;
    [SerializeField] private RectTransform minigameRectTransform;

    public void ShowPopUp(GameObject popUp)
    {
        if (popUp.activeSelf)
            popUp.SetActive(false);
        else
            popUp.SetActive(true);
    }

    public void TravelToMinigameScene()
    {
        Debug.Log("minus cost: " + cost);
        Debug.Log("plus reward: " + reward);
        TravelManager.instance.MoveTo(minigameRectTransform.anchoredPosition, sceneName);
        //SceneManager.LoadScene(sceneName);
    }


    // == should i put on a separete script? ==
    public void BackToMap()
    {
        SceneManager.LoadScene("MapProtype");
    }
}
