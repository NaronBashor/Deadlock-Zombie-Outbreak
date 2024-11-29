using UnityEngine;
using TMPro;

public class DamagePopUp : MonoBehaviour
{
    public float moveSpeed = 2f;         // Speed at which the text moves upwards
    public float disappearTime = 1f;     // Time before the text disappears
    private float disappearTimer;
    public TextMeshProUGUI textMeshPro;     // Reference to the TextMeshPro component
    private Color textColor;             // Color of the text

    private void Start()
    {
        disappearTimer = disappearTime;
        textColor = Color.red;  // Store initial color
        textMeshPro.color = textColor;
    }

    private void Update()
    {
        // Move the text upwards over time
        transform.position += new Vector3(0, moveSpeed * Time.deltaTime, 0);

        // Fade out the text over time
        disappearTimer -= Time.deltaTime;
        if (disappearTimer <= 0) {
            // Fade out the alpha
            textColor.a -= Time.deltaTime / disappearTime;
            textMeshPro.color = textColor;

            // If fully transparent, destroy the object
            if (textColor.a <= 0) {
                Destroy(gameObject);
            }
        }
    }

    // Set the text value for the popup
    public void SetDamage(float damageAmount)
    {
        textMeshPro.SetText(Mathf.RoundToInt(damageAmount).ToString());
    }
}
