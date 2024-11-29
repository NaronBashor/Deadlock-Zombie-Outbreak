using UnityEngine;

public class PlayerDetection : MonoBehaviour
{
    private Enemy parentEnemy;  // Reference to the parent Enemy script

    private void Start()
    {
        // Get the parent enemy script
        parentEnemy = GetComponentInParent<Enemy>();
    }

    // Detect player collisions using OnTriggerEnter2D (for 2D games)
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            // Get the player's health component
            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null) {
                // Deal damage to the player when the enemy collides with them
                playerHealth.TakeDamage(parentEnemy.attackDamage);
                //Debug.Log("Enemy dealt " + parentEnemy.attackDamage + " damage to the player.");
            }
        }
    }
}
