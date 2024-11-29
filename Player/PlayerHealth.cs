using UnityEngine;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class PlayerHealth : MonoBehaviour
{
    Animator anim;

    public GameObject bloodSplatter;
    public float maxHealth;
    private float currentHealth;

    private CharacterSkills characterSkills;

    public Image healthBarFill;    // Reference to the Image that fills the health bar
    public TextMeshProUGUI healthText;

    private bool isDead = false;
    private bool isInvincible = false;
    public float invincibilityDuration = 3f; // Duration of invincibility in seconds
    public float blinkInterval = 0.2f;       // Interval between blinks

    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        // Get the CharacterSkills component from the player
        characterSkills = GetComponent<CharacterSkills>();

        // Subscribe to the attribute update event
        if (characterSkills != null) {
            characterSkills.OnSkillsUpdated += InitHealth;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            originalColor = spriteRenderer.color;
        }
    }

    private void Start()
    {
        InitHealth();
    }

    public void InitHealth()
    {
        // Set initial max health based on Vitality when the game starts
        UpdateHealthBasedOnVitality();
        currentHealth = maxHealth; // Start with full health
        Initialize(maxHealth);
    }

    public void Initialize(float maxHealth)
    {
        this.maxHealth = maxHealth;
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    // Method to update the UI
    public void UpdateHealthBar()
    {
        Debug.Log("Updating health bar.");
        if (healthBarFill != null) {
            healthBarFill.fillAmount = currentHealth / maxHealth;
        }

        if (healthText != null) {
            healthText.text = $"{currentHealth} / {maxHealth}"; // Display current health
        }
    }

    public void IncreaseHealth(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, maxHealth);
        SoundManager.Instance.PlaySFX("Heal", false);
        UpdateHealthBar();
    }

    // Method to handle damage taken by the player
    public void TakeDamage(float damageAmount)
    {
        if (isDead || isInvincible) return; // Don't take damage if dead or invincible

        currentHealth -= damageAmount;
        SoundManager.Instance.PlaySFX("KnifeHit", false);
        UpdateHealthBar();

        PlayerDamageEffect damageEffect = GetComponent<PlayerDamageEffect>();
        if (damageEffect != null) {
            damageEffect.ShowDamageEffect();
        }

        // Check if the player's health has reached zero
        if (currentHealth <= 0f) {
            Die();
        }

        // Show blood splatter effect
        Instantiate(bloodSplatter, transform.position, Quaternion.identity);
    }

    public void Die()
    {
        isDead = true;

        // Disable player input and relevant components
        SoundManager.Instance.StopWalking();

        //GetComponent<CharacterController>().enabled = false;
        GetComponent<PlayerController>().SetInputEnabled(false);

        anim.SetBool("isAlive", false);

        // Optionally, trigger respawn after a delay
        StartCoroutine(RespawnWithDelay(2f)); // Wait 2 seconds before respawning
    }

    private IEnumerator RespawnWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartCoroutine(Respawn());
    }

    IEnumerator Respawn()
    {
        isDead = false;
        isInvincible = true;
        anim.SetBool("isAlive", true);
        InitHealth();

        // Make the player semi-transparent during invincibility
        if (spriteRenderer != null) {
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0.5f);
        }

        // Enable player movement or any other scripts
        //GetComponent<CharacterController>().enabled = true;
        GetComponent<PlayerController>().SetInputEnabled(true);

        // Blink for invincibility duration
        float elapsedTime = 0f;
        while (elapsedTime < invincibilityDuration) {
            // Toggle visibility to make the player blink
            if (spriteRenderer != null) {
                spriteRenderer.enabled = !spriteRenderer.enabled;
            }

            elapsedTime += blinkInterval;
            yield return new WaitForSeconds(blinkInterval);
        }

        // Ensure the player is visible after blinking ends
        if (spriteRenderer != null) {
            spriteRenderer.enabled = true;
            spriteRenderer.color = originalColor;
        }

        // End of invincibility
        isInvincible = false;
    }

    // Method to update max health based on the current Vitality level
    private void UpdateHealthBasedOnVitality()
    {
        UpdateHealthBar();
        float vitalityLevel = characterSkills.GetSkillLevel("Vitality");

        // Example: Each level of Vitality increases max health by 12
        maxHealth = 50 + (vitalityLevel * 24);

        // Optionally, restore health to the new max if you want to fully heal on update
        currentHealth = Mathf.Min(currentHealth, maxHealth);

        // Debug the new max health if needed
        //Debug.Log($"Max Health updated based on Vitality level: {maxHealth}");
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (characterSkills != null) {
            characterSkills.OnSkillsUpdated -= UpdateHealthBasedOnVitality;
        }
    }
}
