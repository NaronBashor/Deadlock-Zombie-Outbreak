using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Level1Enemy : Enemy
{
    public GameObject bloodSplatter;
    protected Vector3 previousPosition;   // To store the enemy's previous position

    public float attackCooldown = 1.5f;  // Time between attacks
    private float lastAttackTime;        // When the last attack occurred
    public GameObject dropLootPrefab;

    public Collider2D enemyCollider;
    public Collider2D playerDetectorCollider;

    public GameObject damagePopupPrefab;

    public enum EnemyState
    {
        Idle,
        Chase,
        Attack,
        Retreat
    }

    public EnemyState currentState;
    public float attackRange = 1.5f;
    public float detectionRange = 10f;

    protected override void Start()
    {
        base.Start();
        // Ensure enemy components are only initialized on the server
            speed = 2f;
            attackDamage = 8;
            health = 80f;

            // Initialize the previous position to the current position
            previousPosition = transform.position;
            playerDetectorCollider.enabled = false;
    }

    public override void Attack()
    {
        base.Attack();
        // Add specific attack behavior for level 1 enemies
    }

    public override void TakeDamage(float damage, GameObject attacker)
    {
        if (health <= 0) {
            if (enemyCollider != null) {
                enemyCollider.enabled = false;
            }
            playerDetectorCollider.enabled = false;
            GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        }

        // Update health on the server
        health -= damage;

        // Notify all clients of the health update
        RpcShowDamage(damage);

        if (health <= 0) {
            Die();
            if (GameManager.Instance.playerProgress.activeMission != null && GameManager.Instance.playerProgress.activeMission.missionID == "3") {
                Debug.Log("Current active mission: " + (GameManager.Instance.playerProgress.activeMission.missionTitle).ToString());
                GameManager.Instance.OnZombieKilled();
            }
            
            // Find and generate loot from the loot system
            LootSystem lootSystem = FindAnyObjectByType<LootSystem>();
            List<LootItem> loot = lootSystem.GenerateLoot();

            // Drop each item in the loot list
            foreach (LootItem lootItem in loot) {
                int quantity = lootItem.GetRandomQuantity();
                DropLoot(lootItem, quantity);
            }
        }
    }

    private void DropLoot(LootItem lootItem, int quantity)
    {
        if (lootItem.lootPrefab == null) return;

        for (int i = 0; i < quantity; i++) {
            // Create a random offset for each dropped item to avoid stacking
            Vector3 randomOffset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-0.5f, 0.5f), 0);

            // Instantiate the loot item prefab with the random offset near the enemy's position
            GameObject lootDrop = Instantiate(dropLootPrefab, transform.position, Quaternion.identity);

            SpriteRenderer spriteRenderer = lootDrop.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) {
                spriteRenderer.sprite = lootItem.item.image; // Set the sprite to match the inventory item
            }

            // Assign item information to the pickup script on the prefab
            Pickup pickupScript = lootDrop.GetComponent<Pickup>();
            if (pickupScript != null) {
                pickupScript.item = lootItem.item;
                pickupScript.amount = quantity;
            }

            // Apply a "throw" effect to make the item bounce
            Rigidbody2D rb = lootDrop.GetComponent<Rigidbody2D>();
            if (rb != null) {
                Vector2 throwDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.5f, 1f)).normalized;
                float throwForce = UnityEngine.Random.Range(5f, 10f);
                rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);

                float randomTorque = UnityEngine.Random.Range(-10f, 10f);
                rb.AddTorque(randomTorque, ForceMode2D.Impulse);
            }
        }
    }

    private void RpcShowDamage(float damage)
    {
        Instantiate(bloodSplatter, transform.position, Quaternion.identity);

        // Spawn the damage text popup
        ShowDamagePopup(damage);
    }

    private void ShowDamagePopup(float damageAmount)
    {
        // Instantiate the popup prefab at the enemy's position
        GameObject popup = Instantiate(damagePopupPrefab, transform.position, Quaternion.identity);

        // Get the DamagePopup component and set the damage amount
        popup.GetComponent<DamagePopUp>().SetDamage(damageAmount);
    }

    protected override void Update()
    {
        base.Update();

        // Check if the enemy is moving by comparing current and previous position
        if (Vector3.Distance(transform.position, previousPosition) > 0.01f) {
            UpdateMoving(true);
        } else {
            UpdateMoving(false);
        }

        // Store the current position as the previous position for the next frame
        previousPosition = transform.position;

        // Call the appropriate state-handling function based on the current state
        switch (currentState) {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Chase:
                HandleChase();
                break;
            case EnemyState.Attack:
                HandleAttack();
                break;
            case EnemyState.Retreat:
                HandleRetreat();
                break;
        }
    }

    private void HandleIdle()
    {
        // If player is within detection range, start chasing
        if (target != null && Vector3.Distance(transform.position, target.position) < detectionRange) {
            currentState = EnemyState.Chase;
        }
    }

    private void HandleChase()
    {
        //Debug.Log("Chasing player...");

        // If player moves out of detection range, go back to Idle
        if (Vector3.Distance(transform.position, target.position) > detectionRange) {
            //Debug.Log("Lost player, returning to Idle.");
            currentState = EnemyState.Idle;
            return;
        }

        // Move towards the player
        MoveTowardsTarget();

        // If in attack range, switch to Attack state
        if (Vector3.Distance(transform.position, target.position) <= attackRange) {
            //Debug.Log("Player in attack range, switching to Attack state.");
            currentState = EnemyState.Attack;
        }
    }

    private void HandleAttack()
    {
        //Debug.Log("Attacking player...");

        // If enough time has passed since the last attack
        if (Time.time - lastAttackTime >= attackCooldown) {
            // Perform attack
            Attack();

            // Reset the attack timer
            lastAttackTime = Time.time;
        }

        // If player moves out of attack range, return to Chase
        if (Vector3.Distance(transform.position, target.position) > attackRange) {
            //Debug.Log("Player out of attack range, returning to Chase.");
            currentState = EnemyState.Chase;
        }
    }

    private void HandleRetreat()
    {
        // Example retreat logic (if enemy is low on health, etc.)
    }

    private void OnDrawGizmosSelected()
    {
        // Set the Gizmo color
        Gizmos.color = Color.red;

        // Draw a wireframe sphere (circle in 2D) representing the detection range
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw the attack range in a different color
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
