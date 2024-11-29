using UnityEngine;

public class MissionTrigger : MonoBehaviour
{
    public Mission mission;            // Reference to the Mission ScriptableObject
    public DialogueManager dialogueManager; // Reference to DialogueManager
    //private bool playerInPosition = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player") && !mission.isCompleted) {
            //playerInPosition = true;
            CompleteMission();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) {
            //playerInPosition = false;
        }
    }

    private void CompleteMission()
    {
        mission.isCompleted = true;
        //playerInPosition = false;

        if (dialogueManager != null) {
            dialogueManager.ShowMissionCompletionDialogue(mission);
        }

        GameManager.Instance.playerProgress.CompleteMission(mission);
    }
}
