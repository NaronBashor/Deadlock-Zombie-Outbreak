using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static PlayerProgress;

public static class GameSettings
{
    public static string classChosen = " ";
    public static bool canStartNewGame = false;
    public static string playerName;
    public static string timeOfDay = "Day";

    public static float volume = 1.0f;
    public static int difficulty = 1;
    public static bool isFullScreen = true;
    public static int resolutionWidth = 1920;
    public static int resolutionHeight = 1080;
    public static int characterChosen = -1;

    public static float playerMoveSpeed = 5f;
    public static float rotationSpeed = 8f;
    public static float playerHealAmount = 25f;
    public static bool usingController = true;
    public static bool mute = false;

    public static List<SavedInventorySlot> inventoryData = new List<SavedInventorySlot>();
    public static List<string> completedMissions = new List<string>();   // List of completed mission IDs

    private static string settingsFilePath = Application.persistentDataPath + "/gamesettings.json";
    private static string progressFilePath = Application.persistentDataPath + "/playerprogress.json";

    // Save both game settings and player progress
    public static void SavePlayerProgress(PlayerProgress playerProgress)
    {
        // Save Player Progress Data
        if (playerProgress != null) {
            string progressJson = JsonUtility.ToJson(playerProgress.progressData, true);
            File.WriteAllText(progressFilePath, progressJson);
        }

        //Debug.Log("Settings and progress saved.");
    }

    // Save both game settings and player progress
    public static void SaveSettings()
    {
        // Save Game Settings Data
        GameSettingsData settingsData = new GameSettingsData {
            classChosen = classChosen,
            canStartNewGame = canStartNewGame,
            playerName = playerName,
            volume = volume,
            difficulty = difficulty,
            isFullScreen = isFullScreen,
            resolutionWidth = resolutionWidth,
            resolutionHeight = resolutionHeight,
            playerMoveSpeed = playerMoveSpeed,
            rotationSpeed = rotationSpeed,
            playerHealAmount = playerHealAmount,
            usingController = usingController,
            mute = mute,
            characterChosen = characterChosen,
            inventoryData = inventoryData,
            timeOfDay = timeOfDay
        };

        // Convert the settings data to JSON format
        string settingsJson = JsonUtility.ToJson(settingsData, true);

        // Encrypt the JSON string
        string encryptedJson = EncryptionHelper.Encrypt(settingsJson);

        // Write the encrypted JSON string to the file
        File.WriteAllText(settingsFilePath, encryptedJson);
    }

    public static void LoadPlayerProgress(PlayerProgress playerProgress)
    {
        if (File.Exists(progressFilePath)) {
            string progressJson = File.ReadAllText(progressFilePath);
            PlayerProgressData loadedProgressData = JsonUtility.FromJson<PlayerProgressData>(progressJson);

            playerProgress.progressData = loadedProgressData ?? new PlayerProgressData();

            // Restore mission and dialogue states
            foreach (var missionID in loadedProgressData.completedMissions) {
                Mission mission = playerProgress.FindMissionByID(missionID);
                if (mission != null) mission.isCompleted = true;
            }

            //Debug.Log("Player progress loaded.");
        } else {
            //Debug.Log("No previous player progress found.");
        }
    }

    public static void LoadSettings()
    {
        if (File.Exists(settingsFilePath)) {
            // Read the encrypted content from the file
            string encryptedJson = File.ReadAllText(settingsFilePath);

            // Decrypt the JSON string
            string settingsJson = EncryptionHelper.Decrypt(encryptedJson);

            // Deserialize JSON into settings data
            GameSettingsData settingsData = JsonUtility.FromJson<GameSettingsData>(settingsJson);

            classChosen = settingsData.classChosen;
            playerName = settingsData.playerName;
            canStartNewGame = settingsData.canStartNewGame;

            volume = settingsData.volume;
            difficulty = settingsData.difficulty;
            isFullScreen = settingsData.isFullScreen;
            resolutionWidth = settingsData.resolutionWidth;
            resolutionHeight = settingsData.resolutionHeight;
            playerMoveSpeed = settingsData.playerMoveSpeed;
            rotationSpeed = settingsData.rotationSpeed;
            playerHealAmount = settingsData.playerHealAmount;
            usingController = settingsData.usingController;
            mute = settingsData.mute;
            characterChosen = settingsData.characterChosen;
            inventoryData = settingsData.inventoryData ?? new List<SavedInventorySlot>();
            timeOfDay = settingsData.timeOfDay;

            //Debug.Log("Encrypted game settings loaded.");
        } else {
            //Debug.LogWarning("No settings file found.");
        }
    }


    public static bool HasPreviousSave()
    {
        return File.Exists(settingsFilePath) && File.Exists(progressFilePath);
    }

    public static void ResetToDefaults()
    {
        classChosen = " ";
        canStartNewGame = false;
        volume = 1.0f;
        difficulty = 1;
        isFullScreen = true;
        resolutionWidth = 1920;
        resolutionHeight = 1080;
        playerMoveSpeed = 5f;
        rotationSpeed = 8f;
        playerHealAmount = 25f;
        usingController = true;
        mute = false;
        characterChosen = -1;

        if (File.Exists(settingsFilePath)) {
            File.Delete(settingsFilePath);
        }

        if (File.Exists(progressFilePath)) {
            File.Delete(progressFilePath);
        }

        //Debug.Log("Settings reset to defaults and save files deleted.");
    }

    public static void ApplySettings()
    {
        Screen.SetResolution(resolutionWidth, resolutionHeight, isFullScreen);
        AudioListener.volume = volume;
    }
}

[System.Serializable]
public class GameSettingsData
{
    public string classChosen;
    public bool canStartNewGame;
    public string playerName;
    public float volume;
    public int difficulty;
    public bool isFullScreen;
    public int resolutionWidth;
    public int resolutionHeight;
    public int characterChosen;
    public float playerMoveSpeed;
    public float rotationSpeed;
    public float playerHealAmount;
    public bool usingController;
    public bool mute;
    public string timeOfDay;
    public List<SavedInventorySlot> inventoryData;
    public List<string> completedMissions;
}
