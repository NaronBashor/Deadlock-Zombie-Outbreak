using UnityEngine;
using UnityEngine.UI;

public class DayNightCycle : MonoBehaviour
{
    public enum TimeOfDay { Day, Night }
    public TimeOfDay CurrentTimeOfDay { get; private set; }

    public Image colorOverlay; // Reference to the overlay image
    public float dayDuration = 60f; // Full day-night cycle duration in seconds
    public Color dayColor = new Color(1f, 1f, 1f, 0f); // Daytime color with transparent overlay
    public Color nightColor = new Color(0.1f, 0.1f, 0.3f, 0.5f); // Night color with higher alpha (dark blue, semi-transparent)

    private float timeOfDay = 0f;

    void Update()
    {
        // In DayNightCycle script, inside the Update method
        GameSettings.timeOfDay = (CurrentTimeOfDay == TimeOfDay.Day) ? "Day" : "Night";

        // Update time of day
        timeOfDay += Time.deltaTime / dayDuration;
        if (timeOfDay >= 1f) timeOfDay = 0f; // Reset after a full cycle

        // Calculate cycle progress between 0 (night) and 1 (day)
        float cycleProgress = (Mathf.Sin(timeOfDay * Mathf.PI * 2) + 1f) / 2f; // Smooth cycle between 0 (night) and 1 (day)

        // Adjust the alpha curve to peak more at night and remain low otherwise
        float alphaProgress = Mathf.Clamp01(-Mathf.Sin(timeOfDay * Mathf.PI * 2)); // Peaks in the middle of the night

        // Use adjusted alpha that peaks only during the middle of the night
        float adjustedAlpha = Mathf.Lerp(0f, nightColor.a, alphaProgress); // Use nightColor.a for peak alpha at night
        Color currentColor = Color.Lerp(dayColor, nightColor, 1 - cycleProgress); // Interpolates from dayColor to nightColor
        colorOverlay.color = new Color(currentColor.r, currentColor.g, currentColor.b, adjustedAlpha);

        // Update the CurrentTimeOfDay based on cycle progress
        CurrentTimeOfDay = cycleProgress > 0.5f ? TimeOfDay.Day : TimeOfDay.Night;
    }


}
