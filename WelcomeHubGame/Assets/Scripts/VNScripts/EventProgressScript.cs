using System;
using Unity.VisualScripting;
using UnityEngine;

public class EventProgressScript : MonoBehaviour
{
    // this is a class that hosts all the button functions and
    // also keeps a tally of score for victory condition!

    public GameObject[] eventList;
    public int eventCount = 0;
    public int points = 0;

    public void BadEnd()
    {
        eventList[eventCount].SetActive(false);
        eventList[6].SetActive(true);
    }

    public void CorrectAnswer()
    {
        eventList[eventCount].SetActive(false);
        eventCount++;
        points++;
        eventList[eventCount].SetActive(true);
    }

    public void WrongAnswer()
    {
        eventList[eventCount].SetActive(false);
        eventCount++;
        eventList[eventCount].SetActive(true);
    }


}
