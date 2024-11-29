using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static Skill;

public class CharacterSkills : MonoBehaviour
{
    public List<Skill> skills;
    private string saveFilePath;

    public string classChosen;

    // UI References
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI vitalityText;
    public TextMeshProUGUI dexterityText;
    public Slider xpSlider;                   // Reference to the XP Slider
    public TextMeshProUGUI xpText;            // Reference to the XP Text
    public TextMeshProUGUI skillPointsText;   // Reference to available skill points Text
    public TextMeshProUGUI skillPointsAvailableText;
    public TextMeshProUGUI characterLevelText; // Reference to character level Text

    private float characterLevel = 1;  // New character level field
    private float characterExperience = 0;
    private float experienceToNextLevel = 100;
    private float availableSkillPoints = 0;

    public Action OnSkillsUpdated;

    // Skill names as constants for easy reference
    private const string STRENGTH = "Strength";
    private const string DEXTERITY = "Dexterity";
    private const string VITALITY = "Vitality";

    private void Awake()
    {
        saveFilePath = Application.persistentDataPath + "/skillsData.json";
        classChosen = GameSettings.classChosen;
        // Check if "New Game" flag is set
        if (PlayerPrefs.GetInt("IsNewGame", 0) == 1) {
            ResetSkills();  // Reset to default for new game
            InitializeDefaultSkills(classChosen);
            //Debug.Log("New game");
            PlayerPrefs.SetInt("IsNewGame", 0); // Clear the flag to avoid repeating
            PlayerPrefs.Save();
        } else {
            LoadSkills();
        }
    }

    private void Update()
    {
        // Example of adding experience (for debugging)
        //if (Input.GetKeyDown(KeyCode.Space)) {
        //    AddCharacterExperience(50);
        //}
    }

    // Initialize default skills based on character class
    private void InitializeDefaultSkills(string classChosen)
    {
        skills = new List<Skill>
        {
            new Skill(STRENGTH),
            new Skill(DEXTERITY),
            new Skill(VITALITY)
        };

        // Set default levels based on the selected character class
        switch (classChosen) {
            case "Ranger":
                SetSkillLevel(STRENGTH, 2);
                SetSkillLevel(DEXTERITY, 5);
                SetSkillLevel(VITALITY, 2);
                break;
            case "Fighter":
                SetSkillLevel(STRENGTH, 5);
                SetSkillLevel(DEXTERITY, 2);
                SetSkillLevel(VITALITY, 5);
                break;
            default:
                Debug.LogWarning("Character class not recognized. Defaulting to basic skills.");
                break;
        }
    }

    private void SetSkillLevel(string skillName, int level)
    {
        Skill skill = skills.Find(s => s.skillName == skillName);
        if (skill != null) {
            skill.level = level;
            skill.experienceToNextLevel = skill.CalculateExperienceToNextLevel(); // Adjust XP requirement if needed
        }
    }

    // Add experience to the character and handle level up
    public void AddCharacterExperience(float experience)
    {
        characterExperience += experience;

        // Check if the character levels up
        while (characterExperience >= experienceToNextLevel) {
            LevelUpCharacter();
        }

        UpdateUI();
        SaveSkills();
    }

    public float GetAvailableSkillPoints()
    {
        return availableSkillPoints;
    }

    // Level up the character, increase available skill points, and adjust XP requirements
    private void LevelUpCharacter()
    {
        characterExperience -= experienceToNextLevel;
        characterLevel++;
        availableSkillPoints++;
        experienceToNextLevel += 50;  // Increase XP requirement for each level
        //Debug.Log($"Character leveled up to level {characterLevel}! Skill points available: {availableSkillPoints}");
    }

    // Update all UI elements
    public void UpdateUI()
    {
        strengthText.text = GetSkillLevel(STRENGTH).ToString();
        vitalityText.text = GetSkillLevel(VITALITY).ToString();
        dexterityText.text = GetSkillLevel(DEXTERITY).ToString();
        skillPointsAvailableText.text = "PRESS SELECT TO LEVEL UP";
        skillPointsAvailableText.gameObject.SetActive(availableSkillPoints > 0);
        characterLevelText.text = characterLevel.ToString();
        UpdateXPUI();
    }

    // Update the XP bar and text
    private void UpdateXPUI()
    {
        xpSlider.maxValue = experienceToNextLevel;
        xpSlider.value = characterExperience;
        xpText.text = $"XP: {characterExperience} / {experienceToNextLevel}";
        //Debug.Log($"Slider was set to a value of: {xpSlider.value}");
    }

    // Allocate a skill point to a specific skill
    public void AllocateSkillPoint(string skillName)
    {
        Skill skill = skills.Find(s => s.skillName == skillName);
        if (skill != null && availableSkillPoints > 0) {
            OnSkillsUpdated?.Invoke();
            skill.level++;
            availableSkillPoints--;
            UpdateUI();
            SaveSkills();
            Debug.Log($"{skill.skillName} level increased to {skill.level}. Remaining skill points: {availableSkillPoints}");
        } else {
            Debug.LogWarning("No available skill points or invalid skill.");
        }
    }

    // Get the current level of a skill
    public float GetSkillLevel(string skillName)
    {
        Skill skill = skills.Find(s => s.skillName == skillName);
        return skill != null ? skill.level : 0;
    }

    public void SaveSkills()
    {
        if (string.IsNullOrEmpty(saveFilePath)) {
            //Debug.LogError("Save file path is null or empty!");
            return;
        }

        SkillsData data = new SkillsData {
            skills = skills,
            characterLevel = characterLevel,
            characterExperience = characterExperience,
            experienceToNextLevel = experienceToNextLevel,
            availableSkillPoints = availableSkillPoints
        };
        string json = JsonUtility.ToJson(data, true);

        // Encrypt the JSON data
        string encryptedJson = EncryptionHelper.Encrypt(json);

        File.WriteAllText(saveFilePath, encryptedJson);
    }

    public void LoadSkills()
    {
        if (File.Exists(saveFilePath)) {
            string encryptedJson = File.ReadAllText(saveFilePath);

            try {
                // Decrypt the JSON data
                string json = EncryptionHelper.Decrypt(encryptedJson);

                SkillsData data = JsonUtility.FromJson<SkillsData>(json);
                skills = data.skills;
                characterLevel = data.characterLevel;
                characterExperience = data.characterExperience;
                experienceToNextLevel = data.experienceToNextLevel;
                availableSkillPoints = data.availableSkillPoints;

                UpdateUI();
                Debug.Log("Skills loaded successfully.");
            } catch (Exception e) {
                Debug.LogError("Failed to decrypt save file: " + e.Message);
            }
        } else {
            Debug.LogWarning("No save file found. Initializing default skills.");
            InitializeDefaultSkills(classChosen);
        }
    }

    // Reset skills to default state (like starting a new game)
    public void ResetSkills()
    {
        if (File.Exists(saveFilePath)) {
            File.Delete(saveFilePath);
        }

        InitializeDefaultSkills(classChosen);
        characterLevel = 1;
        characterExperience = 0;
        experienceToNextLevel = 100;
        availableSkillPoints = 0;
        SaveSkills();
        UpdateUI();
        //Debug.Log($"{characterLevel}, {characterExperience}, {experienceToNextLevel}, {availableSkillPoints}");
        //Debug.Log("Skills have been reset to defaults.");
    }

    private void OnApplicationQuit()
    {
        SaveSkills();
    }
}

// Serializable class to wrap the list of skills for JSON serialization
[System.Serializable]
public class SkillsData
{
    public List<Skill> skills;
    public float characterLevel;
    public float characterExperience;
    public float experienceToNextLevel;
    public float availableSkillPoints;
}
