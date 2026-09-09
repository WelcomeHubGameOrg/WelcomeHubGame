using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TMP_Text titleText;
    public TMP_Text descriptionText;
    public Transform choiceContainer;
    public Button choiceButtonPrefab;

    public float slowdownDuration = 0.5f;
    
    public void ShowEvent(EventDefinition eventDefinition)
    {
        SlowDownTimeScale();
        dialoguePanel.SetActive(true);

        titleText.text = eventDefinition.EventName;
        descriptionText.text = eventDefinition.Description;

        foreach (Transform child in choiceContainer)
            Destroy(child.gameObject);

        foreach (var choice in eventDefinition.Choices)
        {
            var button = Instantiate(choiceButtonPrefab, choiceContainer);
            button.GetComponentInChildren<TMP_Text>().text = choice.choiceText;
            button.onClick.AddListener(() => SelectChoice(choice));
        }
    }

    private void SelectChoice(EventChoice choice)
    {
        Debug.Log(choice.consequenceDescription);

        dialoguePanel.SetActive(false);
        Time.timeScale = 1f;
    }
    
    private void SlowDownTimeScale()
    {
        StartCoroutine(SlowDownCoroutine());
    }

    private IEnumerator SlowDownCoroutine()
    {
        var startScale = Time.timeScale;
        var elapsed = 0f;

        while (elapsed < slowdownDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            var t = elapsed / slowdownDuration;
            var eased = t * t * (3f - 2f * t); // smoothstep: eases in AND out, no abrupt start/end
            Time.timeScale = Mathf.Lerp(startScale, 0f, eased);
            yield return null;
        }

        Time.timeScale = 0f;
    }
}