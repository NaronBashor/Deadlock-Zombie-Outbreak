using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerDamageEffect : MonoBehaviour
{
    public Image damageOverlay;           // Reference to the UI Image for the overlay
    public float fadeSpeed = 2f;          // Speed at which the overlay fades out
    public float maxAlpha = 0.8f;         // Maximum opacity of the overlay when hit
    private Coroutine fadeCoroutine;

    private void Start()
    {
        if (damageOverlay != null) {
            // Start with the overlay invisible
            Color overlayColor = damageOverlay.color;
            overlayColor.a = 0;
            damageOverlay.color = overlayColor;
        }
    }

    // Call this method when the player takes damage
    public void ShowDamageEffect()
    {
        if (damageOverlay != null) {
            // Stop any existing fade-out coroutine
            if (fadeCoroutine != null) {
                StopCoroutine(fadeCoroutine);
            }

            // Set the overlay to maximum opacity
            Color overlayColor = damageOverlay.color;
            overlayColor.a = maxAlpha;
            damageOverlay.color = overlayColor;

            // Start fading out the overlay
            fadeCoroutine = StartCoroutine(FadeOut());
        }
    }

    // Coroutine to fade out the overlay
    private IEnumerator FadeOut()
    {
        while (damageOverlay.color.a > 0) {
            Color overlayColor = damageOverlay.color;
            overlayColor.a -= Time.deltaTime * fadeSpeed;
            damageOverlay.color = overlayColor;
            yield return null;
        }
    }
}
