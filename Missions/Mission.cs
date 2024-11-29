using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Mission")]
public class Mission : ScriptableObject
{
    public string missionTitle;
    [TextArea(5, 10)] public string missionDescription;
    [TextArea(3, 5)] public string missionCompletionText;
    public bool isCompleted;
    public string missionID;
    public string objectiveTextForTracker;

    // New fields for specific objective tracking
    public int zombiesToKill; // Total zombies needed to complete mission
    public int zombiesKilled; // Counter for killed zombies

    // Max characters per dialogue box segment
    private const int maxCharactersPerSegment = 250;

    // Method to get segmented description lines for dialogue
    public List<string> GetDescriptionSegments()
    {
        return SplitTextIntoSegments(missionDescription, maxCharactersPerSegment);
    }

    // Method to get segmented description lines for dialogue
    public List<string> GetCompletionTextSegments()
    {
        return SplitTextIntoSegments(missionCompletionText, maxCharactersPerSegment);
    }

    private List<string> SplitTextIntoSegments(string text, int maxCharacters)
    {
        List<string> segments = new List<string>();

        // Split the text into paragraphs by double newlines
        string[] paragraphs = text.Split(new string[] { "\n\n" }, System.StringSplitOptions.None);

        foreach (string paragraph in paragraphs) {
            if (string.IsNullOrWhiteSpace(paragraph)) continue; // Skip empty paragraphs

            int currentIndex = 0;

            // Further split each paragraph if it exceeds maxCharacters
            while (currentIndex < paragraph.Length) {
                int length = Mathf.Min(maxCharacters, paragraph.Length - currentIndex);
                string segment = paragraph.Substring(currentIndex, length);

                // Ensure that we end the segment at a word boundary
                int lastSpaceIndex = segment.LastIndexOf(' ');
                if (lastSpaceIndex != -1 && currentIndex + length < paragraph.Length) {
                    segment = segment.Substring(0, lastSpaceIndex);
                    length = lastSpaceIndex + 1;
                }

                segments.Add(segment.Trim());
                currentIndex += length;
            }
        }

        return segments;
    }

    // Method to check if mission is complete
    public bool CheckMissionCompletion()
    {
        if (zombiesKilled >= zombiesToKill) {
            isCompleted = true;
            return true;
        }
        return false;
    }

    // Method to increment kill count
    public void AddKill()
    {
        zombiesKilled++;
        CheckMissionCompletion();
    }
}
