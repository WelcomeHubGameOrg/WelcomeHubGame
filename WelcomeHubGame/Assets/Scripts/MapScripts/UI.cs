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

    public void WindowPopUp(GameObject popUp)
    {
        if(popUp.activeSelf)
            popUp.SetActive(false);
        else
            popUp.SetActive(true);
    }    
}
