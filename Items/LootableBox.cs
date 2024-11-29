using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class LootableBox : MonoBehaviour
{
    public GameObject dropLootPrefab;
    private bool playerNearby = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null) {
            if (collision.CompareTag("Player")) {
                playerNearby = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null) {
            if (collision.CompareTag("Player")) {
                playerNearby = false;
            }
        }
    }

    private void Update()
    {
        if (playerNearby) {
            if (Keyboard.current.spaceKey.wasPressedThisFrame || Gamepad.current?.buttonSouth.wasPressedThisFrame == true) {
                // Find and generate loot from the loot system
                LootSystem lootSystem = FindAnyObjectByType<LootSystem>();
                List<LootItem> loot = lootSystem.GenerateLoot();

                // Drop each item in the loot list
                foreach (LootItem lootItem in loot) {
                    int quantity = lootItem.GetRandomQuantity();
                    DropLoot(lootItem, quantity);
                }
                Destroy(this.gameObject);
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
}
