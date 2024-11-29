using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public GameObject dialoguePanel;
    public TextMeshProUGUI dialogueTitle;   // Title element for mission name
    public TextMeshProUGUI dialogueText;    // Text element for mission description and completion text
    public float typingSpeed = 0.05f;

    private bool isDialogueActive = false;
    private Coroutine typingCoroutine;
    private List<string> currentDialogueSegments;
    private int currentSegmentIndex;
    public Mission currentMission;

    public void ShowMissionDialogue(Mission mission)
    {
        //Debug.Log($"Starting dialogue for mission: {mission.missionTitle}");

        dialoguePanel.SetActive(true); // Ensure the dialogue panel is active

        GameObject.Find("Player").GetComponent<PlayerController>().SetInputEnabled(false);

        // Stop any existing coroutine to reset dialogue
        if (typingCoroutine != null) {
            StopCoroutine(typingCoroutine);
        }

        currentMission = mission;
        dialogueTitle.text = mission.missionTitle;
        currentDialogueSegments = mission.GetDescriptionSegments();
        currentSegmentIndex = 0;
        isDialogueActive = true;

        ShowNextDialogueSegment();
    }



    public void ShowMissionCompletionDialogue(Mission mission)
    {
        //Debug.Log("Mission complete: " + mission.missionTitle);

        // Disable player input
        GameObject.Find("Player").GetComponent<PlayerController>().SetInputEnabled(false);

        dialogueTitle.text = mission.missionTitle;
        currentDialogueSegments = mission.GetCompletionTextSegments();
        currentSegmentIndex = 0;
        isDialogueActive = true;

        dialoguePanel.SetActive(true);
        ShowNextDialogueSegment();
    }

    private void ShowNextDialogueSegment()
    {
        // Skip empty segments if any
        while (currentSegmentIndex < currentDialogueSegments.Count && string.IsNullOrWhiteSpace(currentDialogueSegments[currentSegmentIndex])) {
            currentSegmentIndex++;
        }

        //Debug.Log("Showing next dialogue segment");

        // Display the next segment if available
        if (currentSegmentIndex < currentDialogueSegments.Count) {
            if (typingCoroutine != null) {
                StopCoroutine(typingCoroutine);
            }

            typingCoroutine = StartCoroutine(TypeText(currentDialogueSegments[currentSegmentIndex]));
            currentSegmentIndex++;
        } else {
            EndDialogue();
        }
    }

    private IEnumerator TypeText(string text)
    {
        dialogueText.text = ""; // Clear previous text
        foreach (char c in text) {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed);

            // Allow skipping text typing
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Gamepad.current?.buttonSouth.wasPressedThisFrame == true) {
                dialogueText.text = text; // Complete text instantly
                break;
            }
        }

        typingCoroutine = null;
    }

    private void Update()
    {
        // Continue to next dialogue segment if active and coroutine is done
        if (isDialogueActive && typingCoroutine == null && (Keyboard.current.spaceKey.wasPressedThisFrame || Gamepad.current?.buttonSouth.wasPressedThisFrame == true)) {
            ShowNextDialogueSegment();
        }
    }

    private void EndDialogue()
    {
        //Debug.Log("Ending dialogue for mission: " + currentMission?.missionTitle);

        if (currentMission != null && currentMission.missionID != null) {
            if (currentMission.missionID == "1") {
                GameManager.Instance.playerProgress.CompleteMission(currentMission);
            }
        }

        isDialogueActive = false;

        // Only deactivate the panel if no further dialogue is expected
        if (dialoguePanel.activeSelf) {
            dialoguePanel.SetActive(false);
        }

        // Re-enable player input after dialogue ends
        GameObject.Find("Player").GetComponent<PlayerController>().SetInputEnabled(true);
    }

}
