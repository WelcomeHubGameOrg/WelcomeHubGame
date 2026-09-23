using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TextController : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI fullEventText;
    [SerializeField] private float typeSpeed = 10;

    private Queue<string> paragraphs = new Queue<string>();

    public bool eventEnded;
    public bool isTyping;

    private string p;

    private Coroutine typeTextCoroutine;

    private const string HTML_ALPHA = "<color=#00000000>";
    private const float MAX_TYPE_TIME = 0.1f;

    public void DisplayNextParagraph(VNEvent vNEvent)
    {
        // if nothing in queue
        if (paragraphs.Count == 0)
        {
            if (!eventEnded)
            {
                // start reading text
                StartEvent(vNEvent);
            }
            else if (eventEnded && !isTyping)
            {
                Debug.Log("Event Over! Something else should happen.");
                //EndEvent();

                // change this later!
                return;
            }
        }

        // if something in queue
        if (!isTyping)
        {
            p = paragraphs.Dequeue();

            typeTextCoroutine = StartCoroutine(TypeVNEvent(p));
        }

        // if text is being typed
        else
        {
            FinishParagraphEarly();
        }
         

        // update text
        //fullEventText.text = p;

        if (paragraphs.Count == 0)
        {
            eventEnded = true;
        }
    }



    private void StartEvent(VNEvent vNEvent)
    {
        // activate gameObject
        //if (!gameObject.activeSelf)
        //{
        //    gameObject.SetActive(true);
        //}

        // add written text to queue
        for (int i = 0; i < vNEvent.paragraphs.Length; i++)
        {
            paragraphs.Enqueue(vNEvent.paragraphs[i]);
        }
    }

    private void EndEvent()
    {
        //clear the queue

        // return bool to false
        eventEnded = false;
    }

    // reveals text at the set text speed
    private IEnumerator TypeVNEvent(string p)
    {
        isTyping = true;

        fullEventText.text = "";

        string originalText = p;
        string displayedText = "";
        int alphaIndex = 0;

        foreach (char c in p.ToCharArray())
        {
            alphaIndex++;
            fullEventText.text = originalText;

            displayedText = fullEventText.text.Insert(alphaIndex, HTML_ALPHA);
            fullEventText.text = displayedText;

            yield return new WaitForSeconds(MAX_TYPE_TIME / typeSpeed);
        }

        isTyping = false;
    }

    private void FinishParagraphEarly()
    {
        //stop coroutine
        StopCoroutine(typeTextCoroutine);

        // finish displaying text
        fullEventText.text = p;

        // update isTyping bool
        isTyping = false;
    }
}
