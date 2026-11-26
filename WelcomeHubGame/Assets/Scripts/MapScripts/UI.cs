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

    [Header("Story-Related")]
    public GameObject StoryPanelStart;

    [SerializeField] private MoveMap moveMap;
    [Header("Story setup")]
    [SerializeField] private GameObject modeChoiceStep;
    [SerializeField] private GameObject nameStep;
    [SerializeField] private GameObject genderStep;
    [SerializeField] private GameObject studentTypeStep;
    [SerializeField] private TMP_InputField storyNameInput;
    [SerializeField] private TMP_Text storyNameError;
    [SerializeField] private RectTransform airportStart;
    [SerializeField] private RectTransform homeStart;

    private string pendingStoryName;
    private StoryGender pendingStoryGender;

    public void ChooseStory()
    {
        ShowStoryStep(nameStep);
        storyNameInput.ActivateInputField();
    }

    public void ConfirmStoryName()
    {
        string enteredName = storyNameInput.text.Trim();
        if (string.IsNullOrEmpty(enteredName))
        {
            storyNameError.text = "Enter a name to continue.";
            return;
        }

        pendingStoryName = enteredName;
        storyNameError.text = string.Empty;
        ShowStoryStep(genderStep);
    }

    public void ChooseMale() => ChooseGender(StoryGender.Male);
    public void ChooseFemale() => ChooseGender(StoryGender.Female);
    public void ChooseNonBinary() => ChooseGender(StoryGender.NonBinary);

    private void ChooseGender(StoryGender gender)
    {
        pendingStoryGender = gender;
        ShowStoryStep(studentTypeStep);
    }

    public void ChooseExchangeStudent() => FinishStory(StudentType.Exchange, airportStart);
    public void ChooseFinnishStudent() => FinishStory(StudentType.Finnish, homeStart);

    private void FinishStory(StudentType studentType, RectTransform startPoint)
    {
        Vector2 startPosition = TravelManager.instance.PlacePlayerAt(startPoint);
        GameManager.Instance.BeginStory(
            pendingStoryName, pendingStoryGender, studentType, startPosition);

        SetWelcomeVisible(false);
    }

    private void ShowStoryStep(GameObject step)
    {
        modeChoiceStep.SetActive(step == modeChoiceStep);
        nameStep.SetActive(step == nameStep);
        genderStep.SetActive(step == genderStep);
        studentTypeStep.SetActive(step == studentTypeStep);
    }
    private void Start()
    {
        SetWelcomeVisible(GameManager.Instance.SelectedMode == PlayMode.Unselected);
    }

    public void ChooseSandbox()
    {
        GameManager.Instance.SetPlayMode(PlayMode.Sandbox);
        SetWelcomeVisible(false);
    }

    private void SetWelcomeVisible(bool visible)
    {
        StoryPanelStart.SetActive(visible);

        if (moveMap != null)
            moveMap.enabled = !visible;
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
