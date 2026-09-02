using UnityEngine;

public class UI : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowPopUp(GameObject popUp)
    {
        popUp.SetActive(true);
    }

    public void Travel(GameObject popUp)
    {
        //temporary
        popUp.SetActive(false);
    }
}
