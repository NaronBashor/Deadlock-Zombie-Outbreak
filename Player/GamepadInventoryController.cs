using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadInventoryController : MonoBehaviour
{
    public InventoryManager inventoryManager;      // Reference to the InventoryManager
    private InventoryItem selectedItem = null;     // Item currently selected for picking up
    private bool isHoldingItem = false;            // True if the player is holding an item
    private Transform originalSlot = null;         // The original slot in case item can't be placed

    public GameObject itemDropPrefab;              // Reference to the prefab that will be dropped in the world
    public Transform dropPoint;                    // The point where the item should drop (e.g., player's position)

    private PlayerControls controls;

    private void Awake()
    {
        controls = new PlayerControls();
    }

    private void OnEnable()
    {
        controls.Player.Enable();
        controls.Player.PickupDrop.performed += OnPickupDrop;
        controls.Player.DropSingleItem.performed += OnDropSelected;  // Handle 'X' button for dropping a single item
        inventoryManager.OnInventoryClose += HandleInventoryClose; // Subscribe to inventory close event
        controls.Player.Consume.performed += OnUseItem;
    }

    private void OnDisable()
    {
        controls.Player.Disable();
        controls.Player.PickupDrop.performed -= OnPickupDrop;
        controls.Player.DropSingleItem.performed -= OnDropSelected;
        inventoryManager.OnInventoryClose -= HandleInventoryClose; // Unsubscribe from inventory close event
        controls.Player.Consume.performed -= OnUseItem;
    }

    private void OnPickupDrop(InputAction.CallbackContext context)
    {
        if (context.performed) {
            if (!inventoryManager.IsInventoryOpen) { return; }
            if (isHoldingItem) {
                // Drop the item if already holding one
                DropItem();
            } else {
                // Pick up the item if not holding one
                PickupItem();
            }
        }
    }

    private void OnDropSelected(InputAction.CallbackContext context)
    {
        if (context.performed) {
            if (!inventoryManager.IsInventoryOpen) { return; }
            DropSelectedItemIntoWorld();
        }
    }

    private void DropSelectedItemIntoWorld()
    {
        // Get the currently selected slot's item from the InventoryManager
        InventoryItem selectedItem = inventoryManager.GetSelectedItem();

        // Check if there's an item to drop
        if (selectedItem != null) {
            // Instantiate the pickup prefab at the drop point
            GameObject droppedItem = Instantiate(itemDropPrefab, dropPoint.position, Quaternion.identity);
            droppedItem.GetComponent<SpriteRenderer>().sprite = selectedItem.item.image;

            // Set the item data in the dropped prefab
            Pickup pickupScript = droppedItem.GetComponent<Pickup>();
            if (pickupScript != null) {
                pickupScript.item = selectedItem.item;
            }

            // Apply force to make it "bounce" away
            Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
            if (rb != null) {
                Vector2 throwDirection = GetRandomThrowDirection();
                float throwForce = Random.Range(5f, 10f);
                rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
                float randomTorque = Random.Range(-10f, 10f);
                rb.AddTorque(randomTorque, ForceMode2D.Impulse);
            }

            // Reduce the item count or destroy it if it's the last one
            selectedItem.count--;
            if (selectedItem.count <= 0) {
                Destroy(selectedItem.gameObject);
            } else {
                selectedItem.RefreshCount();
            }
        }
    }

    private void PickupItem()
    {
        // Get the currently selected slot from the InventoryManager
        InventorySlot selectedSlot = inventoryManager.GetSelectedSlot();

        // Check if there is an item in the selected slot
        if (selectedSlot != null && selectedSlot.transform.childCount > 0) {
            SoundManager.Instance.PlaySFX("ButtonClick", false);
            // Get the item and set it as the currently held item
            selectedItem = selectedSlot.GetComponentInChildren<InventoryItem>();

            // Store the original slot in case we can't place the item later
            originalSlot = selectedSlot.transform;

            // Update the item's parent to follow the cursor visually
            if (selectedItem != null) {
                isHoldingItem = true;
                selectedItem.parentAfterDrag = null;

                // Make the item follow the gamepad cursor by setting it to the top-level canvas (or any UI root)
                selectedItem.transform.SetParent(GameObject.Find("MainCanvas").GetComponent<Transform>());
                selectedItem.transform.SetAsLastSibling(); // Ensure it's on top of other UI elements
            }
        }
    }

    private void DropItemIntoWorld()
    {
        // Drop the selected item(s) into the game world at the drop point
        if (selectedItem != null) {
            // Get the total count of items to drop
            int dropCount = selectedItem.count;
            if (dropCount == 0) {
                dropCount = 1;
            }

            // Iterate over each item in the stack and drop them
            for (int i = 0; i < dropCount; i++) {
                // Instantiate the pickup prefab at the drop point
                GameObject droppedItem = Instantiate(itemDropPrefab, dropPoint.position, Quaternion.identity);
                droppedItem.GetComponent<SpriteRenderer>().sprite = selectedItem.item.image;

                // Set the item data in the dropped prefab
                Pickup pickupScript = droppedItem.GetComponent<Pickup>();
                if (pickupScript != null) {
                    pickupScript.item = selectedItem.item;
                }

                // Apply force to make it "bounce" away
                Rigidbody2D rb = droppedItem.GetComponent<Rigidbody2D>();
                if (rb != null) {
                    Vector2 throwDirection = GetRandomThrowDirection();  // Get a random throw direction
                    float throwForce = Random.Range(5f, 10f);  // Randomize force for variation
                    rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);

                    // Optionally add some random torque for rotation
                    float randomTorque = Random.Range(-10f, 10f);
                    rb.AddTorque(randomTorque, ForceMode2D.Impulse);
                }
            }

            // Clear or reduce the item count in the inventory
            selectedItem.count = 0;
            Destroy(selectedItem.gameObject);

            // Reset holding state
            isHoldingItem = false;
            selectedItem = null;
        }
    }


    // Called when the inventory is closed
    private void HandleInventoryClose()
    {
        if (isHoldingItem) {
            // Drop the item into the world if the inventory is closed mid-drag
            DropItemIntoWorld();
        }
    }

    // Helper method to get a random direction away from the player
    private Vector2 GetRandomThrowDirection()
    {
        float randomAngle = Random.Range(-45f, 45f);
        Vector2 direction = new Vector2(Mathf.Cos(randomAngle * Mathf.Deg2Rad), Mathf.Sin(randomAngle * Mathf.Deg2Rad));
        return direction.normalized;
    }

    private void OnUseItem(InputAction.CallbackContext context)
    {
        if (context.performed && inventoryManager.GetSelectedSlot().GetComponentInChildren<InventoryItem>() != null) {
            UseItem(inventoryManager.GetSelectedSlot().GetComponentInChildren<InventoryItem>());
        }
    }

    private void UseItem(InventoryItem item)
    {
        if (item == null || item.count <= 0) return;

        // Check the type of item to determine what action to take
        switch (item.item.itemType) {
            case Item.ItemType.Consumable:
                UseConsumable(item);
                // Reduce the item count and destroy if it's depleted
                item.count--;
                item.RefreshCount();
                if (item.count <= 0) {
                    Destroy(item.gameObject);
                }
                break;
        }
    }


    private void UseConsumable(InventoryItem item)
    {
        // Check if the consumable is a health pack
        if (item.item.itemName == "Health") {
            PlayerHealth playerHealth = GetComponentInParent<PlayerHealth>();
            if (playerHealth != null) {
                // Heal the player by the specified heal amount
                playerHealth.IncreaseHealth(item.item.healAmount);
                //Debug.Log($"Used a Health Pack! Healed {item.item.healAmount} HP.");
            }
        }
    }

    private void DropItem()
    {
        // Get the currently selected slot from the InventoryManager
        InventorySlot selectedSlot = inventoryManager.GetSelectedSlot();

        // Make sure we are holding an item
        if (selectedItem != null && selectedSlot != null) {
            InventoryItem itemInSlot = selectedSlot.GetComponentInChildren<InventoryItem>();
            SoundManager.Instance.PlaySFX("ButtonClick", false);

            if (itemInSlot != null && itemInSlot.item == selectedItem.item && itemInSlot.item.stackable) {
                // Attempt to stack items
                int maxStackSize = itemInSlot.item.maxStackSize;
                int combinedCount = itemInSlot.count + selectedItem.count;

                if (combinedCount <= maxStackSize) {
                    // Stack items together
                    itemInSlot.count = combinedCount;
                    itemInSlot.RefreshCount();
                    Destroy(selectedItem.gameObject);
                } else {
                    int remaining = combinedCount - maxStackSize;
                    itemInSlot.count = maxStackSize;
                    itemInSlot.RefreshCount();
                    selectedItem.count = remaining;
                    selectedItem.RefreshCount();
                    ReturnItemToOriginalSlot();
                    return;
                }
            } else if (itemInSlot == null) {
                // Drop the item into an empty slot
                selectedItem.parentAfterDrag = selectedSlot.transform;
                selectedItem.transform.SetParent(selectedSlot.transform);
                selectedItem.transform.localPosition = Vector3.zero;
            } else {
                // Slot is occupied by a different item, return to original slot
                ReturnItemToOriginalSlot();
                return;
            }

            // Reset holding state
            isHoldingItem = false;
            selectedItem = null;
        }
    }

    private void ReturnItemToOriginalSlot()
    {
        if (selectedItem != null && originalSlot != null) {
            selectedItem.parentAfterDrag = originalSlot;
            selectedItem.transform.SetParent(originalSlot);
            selectedItem.transform.localPosition = Vector3.zero;
            isHoldingItem = false;
            selectedItem = null;
        }
    }

    private void Update()
    {
        if (isHoldingItem && selectedItem != null) {
            // Follow the cursor visually while holding the item
            Vector3 cursorPosition = inventoryManager.GetCursorWorldPosition();
            selectedItem.transform.position = cursorPosition;
        }
    }
}
