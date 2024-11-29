using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using static PlayerProgress;

public class GameManager : MonoBehaviour
{
    public PlayerProgress playerProgress;
    public PlayerProgressData progressData;
    public Mission initialMission; // Set the first mission in the Inspector
    public Mission killZombiesMission;

    public static GameManager Instance { get; private set; } // Singleton instance

    private void Awake()
    {
        // Set up the singleton instance
        if (Instance != null && Instance != this) {
            Destroy(gameObject); // Destroy duplicate GameManager instances
        } else {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persist GameManager across scenes
            SceneManager.sceneLoaded += OnSceneLoaded; // Register event
        }

        if (progressData == null) {
            progressData = new PlayerProgressData();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded; // Unregister event to avoid memory leaks
    }

    public void ResetAllMissions()
    {
        // Load all Mission ScriptableObjects from the Resources folder
        Mission[] allMissions = Resources.LoadAll<Mission>("Missions");

        // Set isCompleted to false for each mission
        foreach (Mission mission in allMissions) {
            mission.isCompleted = false;
            if (mission.missionID == "3") {
                mission.zombiesKilled = 0;
            }
        }

        Debug.Log("All mission statuses have been reset.");
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void SaveGame()
    {
        GameSettings.SavePlayerProgress(playerProgress);
    }

    public void LoadGame()
    {
        GameSettings.LoadPlayerProgress(playerProgress);
    }

    public void StartNewGame()
    {
        ResetAllMissions();
        playerProgress.ResetProgress(); // Reset progress for a new game
    }

    // Function to be called every time a scene is loaded
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Debug.Log($"Scene loaded: {scene.name}");

        // Call any other functions you want to trigger on a new scene load
        if (scene.name == "Game") {
            YourFunction();
        }
    }

    public void YourFunction()
    {
        // This function will be called each time a new scene is loaded
        //Debug.Log("YourFunction has been called in the new scene!");
        playerProgress.StartMission(initialMission); // Start the first mission
    }

    // Call this method whenever a zombie is killed
    public void OnZombieKilled()
    {
        if (!killZombiesMission.isCompleted) {
            killZombiesMission.AddKill();
            GameObject.Find("Quest Tracker Description").GetComponent<TextMeshProUGUI>().text = $"Zombies killed: {killZombiesMission.zombiesKilled}/{killZombiesMission.zombiesToKill}";
            if (killZombiesMission.zombiesKilled >= 10) {
                GameManager.Instance.playerProgress.CompleteMission(killZombiesMission);
            }
        }
    }
}
