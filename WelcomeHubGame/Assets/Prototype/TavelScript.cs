using Unity.VectorGraphics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TavelScript : MonoBehaviour
{
    [SerializeField] private int cost;
    [SerializeField] private int reward;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void TravelToMinigameScene(string sceneName)
    {
        Debug.Log("minus cost: " + cost);
        Debug.Log("plus reward: " + reward);
        SceneManager.LoadScene(sceneName);
    }

    public void BackToMap()
    {
        SceneManager.LoadScene("MapProtype");
    }
}
