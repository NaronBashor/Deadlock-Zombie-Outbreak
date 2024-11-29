using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;
using TMPro;

public class PlayerProgress : MonoBehaviour
{
    public PlayerProgressData progressData = new PlayerProgressData();
    private DialogueManager dialogueManager;

    // Track the currently active mission
    public Mission activeMission;

    private void Awake()
    {
        dialogueManager = FindAnyObjectByType<DialogueManager>();
    }

    // Reset progress
    public void ResetProgress()
    {
        progressData = new PlayerProgressData();
        activeMission = null; // Reset active mission
    }

    // Start a mission if it’s not already completed
    public void StartMission(Mission mission)
    {
        if (!progressData.completedMissions.Contains(mission.missionID)) {
            activeMission = mission; // Set as active mission

            // Show mission start dialogue or instructions if applicable
            GameObject.Find("Quest Tracker Title").GetComponent<TextMeshProUGUI>().text = mission.missionTitle;
            GameObject.Find("Quest Tracker Description").GetComponent<TextMeshProUGUI>().text = mission.objectiveTextForTracker;
            
            if (dialogueManager != null) {
                dialogueManager.ShowMissionDialogue(mission);
            } else {
                dialogueManager = FindAnyObjectByType<DialogueManager>();
                dialogueManager.ShowMissionDialogue(mission);
            }

            //Debug.Log($"Mission '{mission.missionTitle}' started.");
        }
    }

    // Complete a mission
    public void CompleteMission(Mission mission)
    {
        if (activeMission == mission && !progressData.completedMissions.Contains(mission.missionID)) {
            GameObject.Find("Quest Tracker Title").GetComponent<TextMeshProUGUI>().text = "No Active Mission";
            GameObject.Find("Quest Tracker Description").GetComponent<TextMeshProUGUI>().text = "- Find a mission.";

            // Mark the mission as completed, clear active mission, and save progress
            progressData.completedMissions.Add(mission.missionID);
            mission.isCompleted = true;
            activeMission = null; // Clear the active mission since it's completed
            GameManager.Instance.SaveGame();

            // Find and set the next mission as active
            Mission nextMission = FindNextMission(mission.missionID);
            if (nextMission != null) {
                // Start the next mission after the current dialogue ends
                StartCoroutine(StartNextMissionAfterDialogue(nextMission));
            }
        }
    }

    // Coroutine to wait until dialogue ends before starting the next mission
    private IEnumerator StartNextMissionAfterDialogue(Mission nextMission)
    {
        yield return new WaitForSeconds(2f);
        StartMission(nextMission);
    }

    // Method to find the next mission based on the current mission's ID
    private Mission FindNextMission(string currentMissionID)
    {
        Mission[] missions = Resources.LoadAll<Mission>("Missions").OrderBy(m => m.missionID).ToArray();
        int currentIndex = Array.FindIndex(missions, m => m.missionID == currentMissionID);

        if (currentIndex >= 0 && currentIndex < missions.Length - 1) {
            return missions[currentIndex + 1];
        }
        return null;
    }

    // Find mission by ID
    public Mission FindMissionByID(string id)
    {
        return Resources.LoadAll<Mission>("Missions").FirstOrDefault(mission => mission.missionID == id);
    }

    [Serializable]
    public class PlayerProgressData
    {
        public List<string> completedMissions = new List<string>();
        public List<string> availableDialogues = new List<string>();
        public List<SavedInventorySlot> inventoryData = new List<SavedInventorySlot>();
    }
}
