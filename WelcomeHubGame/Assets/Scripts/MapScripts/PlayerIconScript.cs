using UnityEngine;
using UnityEngine.UI;

public class PlayerIconScript : MonoBehaviour
{
    [SerializeField] private GameObject player; 
    [SerializeField] private Image playerImage;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        TravelManager.instance.player = playerImage;
        TravelManager.instance.playerRect = player.GetComponent<RectTransform>();
        player.GetComponent<RectTransform>().anchoredPosition = TravelManager.instance.playerPosition;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
