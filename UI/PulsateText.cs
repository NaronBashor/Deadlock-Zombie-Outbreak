using UnityEngine;
using System.Collections;

public class PulsateText : MonoBehaviour
{
    public float pulsateSpeed = 3.5f;
    public float pulsateAmount = 0.1f;

    private RectTransform rectTransform;
    private Vector3 originalScale;

    // Start is called before the first frame update
    public void Start()
    {
        rectTransform = GetComponent<RectTransform>();

        if (rectTransform != null) {
            originalScale = rectTransform.localScale;
            StartCoroutine(Pulsate());
        } else {
            Debug.LogError("RectTransform component not found on this GameObject.");
        }
    }

    private void Update()
    {
        if (this.gameObject.activeSelf) {
            StartCoroutine(Pulsate());
        }
    }

    private IEnumerator Pulsate()
    {
        while (true) {
            // Debug Time.time and pulsateSpeed
            //Debug.Log($"Time.time: {Time.time}, pulsateSpeed: {pulsateSpeed}");

            // Calculate the new scale factor
            float sinValue = Mathf.Sin(Time.time * pulsateSpeed);
            float scaleFactor = 1 + sinValue * pulsateAmount;

            // Apply the scale factor to the RectTransform
            Vector3 newScale = originalScale * scaleFactor;
            rectTransform.localScale = newScale;

            // Debug to check the sinValue and the scale factor
            //Debug.Log($"Pulsating... Original Scale: {originalScale}, Sin Value: {sinValue}, Scale Factor: {scaleFactor}, New Scale: {newScale}");

            yield return null;
        }
    }
}
