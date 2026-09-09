using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class FinalEvent : Events, IDisplay
{
    [SerializeField] private WrittenText writtenText;
    [SerializeField] private WrittenText goodEndingText;
    [SerializeField] private TextController textController;

    [SerializeField] private EventProgressScript eventProgressScript;

    public override void Interact()
    {
        if (!textController.eventEnded && eventProgressScript.points != 3)
        {
            Display(writtenText);
        }

        else if (!textController.eventEnded && eventProgressScript.points == 3)
        {
            Display(goodEndingText);
        }

        else
        {
            SceneManager.LoadScene("VNPrototype");
        }
    }

    public void Display(WrittenText writtenText)
    {
        //start printing dialogue
        Debug.Log("Test");
        textController.DisplayNextParagraph(writtenText);

    }

    // Already print when reaching next VN branch
    void OnEnable()
    {
        textController.eventEnded = false;
        if (eventProgressScript.points != 3)
        {
            Display(writtenText);
        }

        else if (eventProgressScript.points == 3)
            Display(goodEndingText);
    }
}
