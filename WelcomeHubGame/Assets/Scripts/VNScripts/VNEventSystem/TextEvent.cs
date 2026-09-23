using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TextEvent : MonoBehaviour
{
    public VNEvent vNEvent;
    [SerializeField] private TextController textController;
    [SerializeField] private GameObject answerPanel;
    [SerializeField] private GameObject answerButton;

    public void Interact()
    {
        if (!textController.eventEnded)
        {
            Display(vNEvent);
        }

        // Generate buttons for answer
        else
        {
            //int j = 0;
            /*foreach (var i in vNEvent.answers)
            {
                //Button newButton = UnityEngine.UI.Button;
                //var newButtonInstantiate(answerButton, answerPanel.transform, worldPositionStays: false);
                //newButton.GetComponentInChildren(Text).text;
                answerText = answerButton.GetComponent<Text>();
                //answerText = vNEvent.answers[j].Text;
            }*/
            //answerPanel.SetActive(true);

            // ^ get back to this later

            vNEvent = vNEvent.branches[0];
            Display(vNEvent);
        }
    }

    public void Display(VNEvent vNEvent)
    {
        // print dialogue
        //Debug.Log("Test");
        textController.DisplayNextParagraph(vNEvent);

    }

    // Start printing on enable
    void OnEnable()
    {
        Display(vNEvent);
    }
}
