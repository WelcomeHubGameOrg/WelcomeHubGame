using UnityEngine;
using TMPro;

public class UI : MonoBehaviour
{
    [Header("Buying tickets button")]
    public TMP_Text ButtonText;
    public GameObject HSLPanel;
    public GameObject BuyTicketPanel;
    public GameObject ExtendTicketPanel;

    [Header("Updating/Buying tickets")]
    public TMP_Text TicketZoneText;
    public TMP_Text PlayerMoneyDisplay;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Opening HSL Related Panels
    public void WindowPopUp(GameObject popUp)
    {
        if(popUp.activeSelf)
            popUp.SetActive(false);
        else
            popUp.SetActive(true);
    }    
    public void OpenHSL()
    {
        HSLPanel.SetActive(true);
        UpdateButtonText();
        BuyTicket(UpdateBoughtTicket());
    }
    public void BuyTicketButton()
    {
        if(GameManager.Instance.A || GameManager.Instance.B || GameManager.Instance.C || GameManager.Instance.D)
        {
            ExtendTicketPanel.SetActive(true);
        }
        else
        {
            BuyTicketPanel.SetActive(true);
        }
    }
    public void UpdateButtonText()
    {
        if(GameManager.Instance.A || GameManager.Instance.B || GameManager.Instance.C || GameManager.Instance.D)
        {
            ButtonText.text = "Extend Ticket";
        }
        else
        {
            ButtonText.text = "Buy Ticket";
        }
    }
    public string UpdateBoughtTicket()
    {
        string ticketZones = "";
        if(GameManager.Instance.A)
            ticketZones += "A";
        if(GameManager.Instance.B)
            ticketZones += "B";
        if(GameManager.Instance.C)
            ticketZones += "C";
        if(GameManager.Instance.D)
            ticketZones += "D";
        if(ticketZones == "")
            ticketZones = "none";
        return ticketZones;
    }
    //Buying tickets
    public void BuyTicket(string zone)
    {
        switch(zone)
        {
            case "A":
                GameManager.Instance.A = true;
                break;
            case "B":
                GameManager.Instance.B = true;
                break;
            case "C":
                GameManager.Instance.C = true;
                break;
            case "D":
                GameManager.Instance.D = true;
                break;
        }
        TicketZoneText.text = UpdateBoughtTicket();
        UpdateButtonText();
    }
    public void PayTicketPrice(float price)
    {
        GameManager.Instance.PlayerMoney -= price;
        PlayerMoneyDisplay.text = GameManager.Instance.PlayerMoney.ToString() + " €";
    }
}
