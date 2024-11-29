using System;
using UnityEngine;

[System.Serializable]
public class Skill
{
    public string skillName;
    public float level;
    public float experience;
    public float experienceToNextLevel;

    private float baseXP = 100;

    // Event triggered when the skill levels up
    public event Action<string, float> OnLevelUp;

    // Constructor to initialize the skill
    public Skill(string name)
    {
        skillName = name;
        level = 1;
        experience = 0;
        experienceToNextLevel = CalculateExperienceToNextLevel(); // Starting requirement for the next level
    }

    // Calculate experience required for the next level
    public float CalculateExperienceToNextLevel()
    {
        return baseXP * Mathf.Pow(level, 1.5f);
    }

    // Add experience and check if it levels up
    public void AddExperience(int xp)
    {
        experience += xp;
        while (experience >= experienceToNextLevel) {
            LevelUp();
        }
    }

    // Level up the skill
    private void LevelUp()
    {
        experience -= experienceToNextLevel;
        level++;
        experienceToNextLevel += CalculateExperienceIncrease(level);

        // Trigger the level-up event
        OnLevelUp?.Invoke(skillName, level);
    }

    // Method to determine how much to increase the experience needed for the next level
    public float CalculateExperienceIncrease(float currentLevel)
    {
        // Adjust this formula as needed for scaling
        return 50 + (currentLevel * 10); // Example scaling
    }

    // Method to reset the experience and levels (if needed)
    public void Reset()
    {
        level = 1;
        experience = 0;
        experienceToNextLevel = 100;
    }
}
