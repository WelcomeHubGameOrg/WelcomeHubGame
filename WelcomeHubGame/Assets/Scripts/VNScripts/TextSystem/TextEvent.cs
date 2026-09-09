using System;
using UnityEngine;

public class TextEvent : Events, IDisplay
{
    [SerializeField] private WrittenText writtenText;
    [SerializeField] private TextController textController;
    [SerializeField] private GameObject answerPanel;

    public override void Interact()
    {
        if (!textController.eventEnded)
        {
            Display(writtenText);
        }

        else {
            answerPanel.SetActive(true);
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
        Display(writtenText);
    }
}
