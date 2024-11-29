using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static Item;

public class InventoryManager : MonoBehaviour
{
    public Item[] allItems;
    public InventorySlot[] inventorySlots;     // Array of inventory slots (16 slots for the 4x4 grid)
    public InventorySlot[] itemBarSlots;       // Array of item bar slots (4 slots for the 1x4 item bar)
    public GameObject inventoryItemPrefab;     // Prefab for inventory items
    public GameObject inventoryPanel;          // Reference to the inventory panel UI
    public GameObject levelUpPanel;

    // UI Elements for Level-Up Screen
    public Button strengthButton;              // Button reference for Strength
    public Button vitalityButton;              // Button reference for Vitality
    public Button dexterityButton;             // Button reference for Dexterity

    public CharacterSkills characterSkills;    // Reference to the CharacterSkills script

    private int selectedItemBarSlot = 0;

    public PlayerController playerController;  // Reference to the player associated with this inventory
    private WeaponManager weaponManager;

    private int selectedSlot = -1;             // Currently selected slot index
    private int inventoryWidth = 4;            // Number of slots per row in the inventory grid (4 columns)
    private int inventoryHeight = 4;           // Number of rows in the inventory grid (4 rows)

    private bool inItemBar = true;             // Flag to track if we're in the item bar
    private bool isInventoryOpen = false;      // Track if the inventory panel is open or closed
    private bool canUseItem = true;

    private InventoryItem selectedItem;
    public InventoryItem hoveredItem;

    PlayerControls controls;
    public GameObject itemDropPrefab;

    public event Action OnInventoryClose;

    public bool IsInventoryOpen
    {
        get {
            return isInventoryOpen;
        }
        set {
            isInventoryOpen = value;
        }
    }

    private void Awake()
    {
        controls = new PlayerControls();

        // Inventory Controls
        controls.Player.NavMenu.performed += OnNavigate;
        controls.Player.ToggleInventory.performed += OnToggleInventory;
        controls.Player.ToggleLevelUpPanel.performed += OnOpenLevelUpPanel;
        controls.Player.Consume.performed += OnUseItem;

        inventoryPanel = GameObject.Find("Inventory Panel");
    }

    private void OnEnable() => controls.Player.Enable();
    private void OnDisable() => controls.Player.Disable();

    private void ToggleInventory()
    {
        if (!isInventoryOpen) {
            OnInventoryClose?.Invoke();
        }
    }

    private void Start()
    {
        weaponManager = playerController.GetComponentInParent<WeaponManager>();
        ChangeSelectedSlot(0);
        inventoryPanel.SetActive(false);
        levelUpPanel.SetActive(false);
        characterSkills = GetComponentInParent<CharacterSkills>();

        // Load saved inventory if available
        if (GameSettings.HasPreviousSave()) {
            GameSettings.LoadSettings();
            LoadInventory();
        }
    }

    private void Update()
    {
        HandleMouseScroll();
    }

    // Handle mouse scroll input to cycle through the item bar
    private void HandleMouseScroll()
    {
        float scroll = Mouse.current.scroll.y.ReadValue();

        if (scroll > 0f) // Scroll up
        {
            // Use the unified selection method
            SelectItemBarSlot((selectedItemBarSlot - 1 + itemBarSlots.Length) % itemBarSlots.Length);
        } else if (scroll < 0f) // Scroll down
          {
            // Use the unified selection method
            SelectItemBarSlot((selectedItemBarSlot + 1) % itemBarSlots.Length);
        }
    }

    // Unified selection method for the item bar
    private void SelectItemBarSlot(int newSlotIndex)
    {
        // Deselect the current slot if there is a valid selection
        if (selectedItemBarSlot >= 0 && selectedItemBarSlot < itemBarSlots.Length) {
            itemBarSlots[selectedItemBarSlot].Deselect();
        }

        // Update the selected slot index
        selectedItemBarSlot = newSlotIndex;

        // Ensure the new slot index is within bounds of the item bar
        if (selectedItemBarSlot < 0) selectedItemBarSlot = itemBarSlots.Length - 1;
        else if (selectedItemBarSlot >= itemBarSlots.Length) selectedItemBarSlot = 0;

        // Select the new slot
        itemBarSlots[selectedItemBarSlot].Select();

        // Check if the selected item is a weapon and update accordingly
        UpdateSelectedWeapon();

        // Update hovered item and synchronize with D-pad state
        HandleItemSelectionChange();
    }

    // Method to update the weapon if the selected item is a weapon
    private void UpdateSelectedWeapon()
    {
        // Retrieve the selected item in the item bar
        InventoryItem selectedItem = itemBarSlots[selectedItemBarSlot].GetComponentInChildren<InventoryItem>();

        if (selectedItem != null && selectedItem.item != null && selectedItem.item.itemType == Item.ItemType.Weapon) {
            // Update the WeaponManager with the new weapon
            weaponManager.UpdateCurrentWeapon(selectedItem.item);

            // Optionally, if there's a player animator, update the animation
            if (playerController != null) {
                playerController.GetComponent<Animator>().SetInteger("currentWeapon", selectedItem.item.weaponAnimationId);
            }
        } else {
            // If no weapon is selected, reset the WeaponManager and animation
            weaponManager.UpdateCurrentWeapon(null);
            if (playerController != null) {
                playerController.GetComponent<Animator>().SetInteger("currentWeapon", 0); // Default to unarmed or melee
            }
        }
    }


    // Called when the selected item bar slot changes
    private void HandleItemSelectionChange()
    {
        InventoryItem selectedItem = itemBarSlots[selectedItemBarSlot].GetComponentInChildren<InventoryItem>();

        if (selectedItem != null) {
            // Perform any logic when the item changes, like updating weapon animations
            // Example: Notify the WeaponManager about the change
            WeaponManager weaponManager = GetComponentInParent<WeaponManager>();
            if (weaponManager != null) {
                weaponManager.UpdateCurrentWeapon(selectedItem.item);
            }
        }
    }

    public void Initialize(PlayerController controller)
    {
        playerController = controller;
        ChangeSelectedSlot(0);
        inventoryPanel.SetActive(false);
    }

    private void OnOpenLevelUpPanel(InputAction.CallbackContext context)
    {
        if (context.performed && isInventoryOpen && GetComponentInParent<CharacterSkills>().GetAvailableSkillPoints() > 0) {
            OpenLevelUpPanel();
        }
    }

    private void OpenLevelUpPanel()
    {
        levelUpPanel.SetActive(true);
    }

    private void OnToggleInventory(InputAction.CallbackContext context)
    {
        if (context.performed) // Disable if Level-Up panel is open
        {
            isInventoryOpen = !isInventoryOpen;
            inventoryPanel.SetActive(isInventoryOpen);
            ToggleInventory();

            if (isInventoryOpen) {
                playerController.SetInputEnabled(false);  // Disable player input
                inItemBar = false;
                SelectFirstInventorySlot();
            } else {
                playerController.SetInputEnabled(true);  // Enable player input
                inItemBar = true;
                SelectFirstItemBarSlot();
            }
        }
    }

    public void DropDraggedItemIntoWorld(InventoryItem draggedItem)
    {
        // Instantiate a world item prefab at the player's position
        GameObject droppedItemGO = Instantiate(itemDropPrefab, playerController.transform.position, Quaternion.identity);

        // Set the item data on the dropped prefab
        SpriteRenderer spriteRenderer = droppedItemGO.GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) {
            spriteRenderer.sprite = draggedItem.item.image; // Set the sprite to match the inventory item
        }

        // Assign item information to the pickup script on the prefab
        Pickup pickupScript = droppedItemGO.GetComponent<Pickup>();
        if (pickupScript != null) {
            pickupScript.item = draggedItem.item;
            pickupScript.amount = draggedItem.count;
        }

        // Apply a "throw" effect to make the item bounce
        Rigidbody2D rb = droppedItemGO.GetComponent<Rigidbody2D>();
        if (rb != null) {
            Vector2 throwDirection = new Vector2(UnityEngine.Random.Range(-1f, 1f), UnityEngine.Random.Range(0.5f, 1f)).normalized;
            float throwForce = UnityEngine.Random.Range(5f, 10f);
            rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);

            float randomTorque = UnityEngine.Random.Range(-10f, 10f);
            rb.AddTorque(randomTorque, ForceMode2D.Impulse);
        }

        // Destroy the dragged item object in the inventory
        Destroy(draggedItem.gameObject);
    }





    // Method to select the first inventory slot
    private void SelectFirstInventorySlot()
    {
        if (inventorySlots.Length > 0) {
            ChangeSelectedSlot(0); // Select the first slot in the inventory grid
        }
    }

    // Method to select the first item bar slot
    private void SelectFirstItemBarSlot()
    {
        if (itemBarSlots.Length > 0) {
            ChangeSelectedSlot(0); // Select the first slot in the item bar
        }
    }

    public void OnNavigate(InputAction.CallbackContext context)
    {
        if (context.performed) {
            Vector2 navigateInput = context.ReadValue<Vector2>();
            NavigateInventory(navigateInput);
        }
    }

    // Method to set the hovered item
    public void SetHoveredItem(InventoryItem item)
    {
        hoveredItem = item;
        //Debug.Log("Hovered item set: " + (hoveredItem != null ? hoveredItem.item.itemName : "None"));
    }

    // Method to clear the hovered item
    public void ClearHoveredItem()
    {
        hoveredItem = null;
        //Debug.Log("Hovered item cleared.");
    }

    // Method to get the currently hovered item
    public InventoryItem GetHoveredItem()
    {
        return hoveredItem;
    }

    private void OnUseItem(InputAction.CallbackContext context)
    {
        if (context.performed && hoveredItem != null) {
            UseItem(hoveredItem); // Use the hovered item if it exists
        }
    }


    private void UseItem(InventoryItem item)
    {
        if (item == null || item.count <= 0 || !canUseItem) return;

        // Check the type of item to determine what action to take
        switch (item.item.itemType) {
            case Item.ItemType.Consumable:
                StartCoroutine(UseItemDelay(item));
                // Reduce the item count and destroy if it's depleted
                item.count--;
                item.RefreshCount();
                if (item.count <= 0) {
                    Destroy(item.gameObject);
                }
                break;
        }
    }

    IEnumerator UseItemDelay(InventoryItem item)
    {
        canUseItem = false;
        yield return new WaitForSeconds(1f);
        UseConsumable(item);
    }

    private void UseConsumable(InventoryItem item)
    {
        canUseItem = true;
        //Debug.Log("Used: " + item.item.itemName);
        // Check if the consumable is a health pack
        if (item.item.itemName == "Health") {
            PlayerHealth playerHealth = playerController.GetComponent<PlayerHealth>();
            if (playerHealth != null) {
                // Heal the player by the specified heal amount
                playerHealth.IncreaseHealth(item.item.healAmount);
                //Debug.Log($"Used a Health Pack! Healed {item.item.healAmount} HP.");
            }
        }
    }

    // Returns the currently selected slot based on the current selection
    public InventorySlot GetSelectedSlot()
    {
        if (inItemBar) {
            // If navigating in the item bar
            if (selectedSlot >= 0 && selectedSlot < itemBarSlots.Length) {
                return itemBarSlots[selectedSlot];
            }
        } else {
            // If navigating in the inventory grid
            if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length) {
                return inventorySlots[selectedSlot];
            }
        }
        return null;
    }

    // Helper method to get the world position of the cursor (for positioning held item)
    public Vector3 GetCursorWorldPosition()
    {
        InventorySlot currentSlot = GetSelectedSlot();
        if (currentSlot != null) {
            return currentSlot.transform.position;
        }
        return Vector3.zero;
    }

    void NavigateInventory(Vector2 direction)
    {
        // Determine if we're navigating only the item bar or the full inventory
        if (!isInventoryOpen) {
            // Inventory is closed, only navigate the item bar
            if (direction.x > 0.1f) {
                MoveSelectionInItemBar(1);  // Move right
            } else if (direction.x < -0.1f) {
                MoveSelectionInItemBar(-1); // Move left
            }
        } else {
            // Inventory is open, navigate both item bar and inventory grid
            if (inItemBar) {
                // Navigation logic for the item bar
                if (direction.x > 0.1f) {
                    MoveSelectionInItemBar(1);  // Move right
                } else if (direction.x < -0.1f) {
                    MoveSelectionInItemBar(-1); // Move left
                } else if (direction.y > 0.1f) {
                    // Move up to the grid from the item bar
                    inItemBar = false;
                    ChangeSelectedSlot((inventoryHeight - 1) * inventoryWidth); // Move to the bottom-left corner of the grid
                }
            } else {
                // Navigation logic for the 4x4 grid
                if (direction.x > 0.1f) {
                    MoveSelectionInGrid(1);  // Move right
                } else if (direction.x < -0.1f) {
                    MoveSelectionInGrid(-1); // Move left
                } else if (direction.y > 0.1f) {
                    MoveSelectionInGrid(-inventoryWidth); // Move up
                } else if (direction.y < -0.1f) {
                    // Move down; if we're in the last row of the grid, transition to item bar
                    if (selectedSlot >= (inventoryHeight - 1) * inventoryWidth) {
                        inItemBar = true;
                        ChangeSelectedSlot(0); // Reset to the first slot in the item bar
                    } else {
                        MoveSelectionInGrid(inventoryWidth);  // Move down
                    }
                }
            }
        }
    }

    public InventoryItem GetSelectedItem()
    {
        InventorySlot selectedSlot = GetSelectedSlot();
        return selectedSlot != null ? selectedSlot.GetComponentInChildren<InventoryItem>() : null;
    }

    void MoveSelectionInGrid(int direction)
    {
        // Calculate the new slot index based on the direction
        int newSlot = selectedSlot + direction;

        // Handle wrapping for horizontal navigation
        if (direction == 1 && (selectedSlot + 1) % inventoryWidth == 0) {
            // Wrap from the end of a row to the beginning of the same row
            newSlot = selectedSlot - (inventoryWidth - 1);
        } else if (direction == -1 && selectedSlot % inventoryWidth == 0) {
            // Wrap from the start of a row to the end of the same row
            newSlot = selectedSlot + (inventoryWidth - 1);
        }

        // Ensure the new slot index is within bounds of the 4x4 grid
        if (newSlot >= 0 && newSlot < inventorySlots.Length) {
            ChangeSelectedSlot(newSlot);
        }
    }

    void MoveSelectionInItemBar(int direction)
    {
        // Calculate the new slot index based on the direction
        int newSlot = selectedSlot + direction;

        // Ensure the new slot index wraps around within the item bar bounds
        if (newSlot < 0) {
            newSlot = itemBarSlots.Length - 1; // Wrap to the last slot if moving left from the first slot
        } else if (newSlot >= itemBarSlots.Length) {
            newSlot = 0; // Wrap to the first slot if moving right from the last slot
        }

        // Update the selected slot
        ChangeSelectedSlot(newSlot);
    }



    void ChangeSelectedSlot(int newValue)
    {
        if (inItemBar) {
            // Deselect the previous inventory slot (if any was selected)
            if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length) {
                inventorySlots[selectedSlot].Deselect();
            }

            // Deselect all inventory slots to ensure none are highlighted when in the item bar
            DeselectAllInventorySlots();

            // Deselect the previous item bar slot
            if (selectedSlot >= 0 && selectedSlot < itemBarSlots.Length) {
                itemBarSlots[selectedSlot].Deselect();
            }

            // Select the new item bar slot and update selectedSlot
            selectedSlot = Mathf.Clamp(newValue, 0, itemBarSlots.Length - 1);
            itemBarSlots[selectedSlot].Select();

            // Update the selected item reference
            selectedItem = itemBarSlots[selectedSlot].GetComponentInChildren<InventoryItem>();
        } else {
            // Deselect the previous item bar slot (if any was selected)
            if (selectedSlot >= 0 && selectedSlot < itemBarSlots.Length) {
                itemBarSlots[selectedSlot].Deselect();
            }

            // Deselect all item bar slots to ensure none are highlighted when in the inventory
            DeselectAllItemBarSlots();

            // Deselect the previous inventory slot
            if (selectedSlot >= 0 && selectedSlot < inventorySlots.Length) {
                inventorySlots[selectedSlot].Deselect();
            }

            // Select the new inventory slot and update selectedSlot
            selectedSlot = Mathf.Clamp(newValue, 0, inventorySlots.Length - 1);
            inventorySlots[selectedSlot].Select();

            // Update the selected item reference
            selectedItem = inventorySlots[selectedSlot].GetComponentInChildren<InventoryItem>();
        }

        // Notify the WeaponManager if the item is a weapon
        if (selectedItem != null && selectedItem.item.itemType == Item.ItemType.Weapon) {
            weaponManager.UpdateCurrentWeapon(selectedItem.item);
        } else {
            // If no item or not a weapon, reset to unarmed state
            //weaponManager.UpdateCurrentWeapon(null);
        }
    }

    // Helper method to deselect all inventory slots
    void DeselectAllInventorySlots()
    {
        foreach (InventorySlot slot in inventorySlots) {
            slot.Deselect();
        }
    }

    // Helper method to deselect all item bar slots
    void DeselectAllItemBarSlots()
    {
        foreach (InventorySlot slot in itemBarSlots) {
            slot.Deselect();
        }
    }


    public bool AddItem(Item item, int quantity)
    {
        if (item.itemType == ItemType.Ammo) {
            // If the item is ammo, add directly to the AmmoManager
            AmmoManager ammoManager = playerController.GetComponent<AmmoManager>();
            ammoManager.AddAmmo(item.ammoType, item.ammoAmount * quantity);
            return true;
        } else {
            // First, try to stack the item if it already exists in the inventory
            for (int i = 0; i < inventorySlots.Length; i++) {
                InventorySlot slot = inventorySlots[i];
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                // Check if the item matches and it's stackable
                if (itemInSlot != null && itemInSlot.item == item && itemInSlot.count < itemInSlot.item.maxStackSize && item.stackable) {
                    int spaceLeft = itemInSlot.item.maxStackSize - itemInSlot.count;
                    int quantityToAdd = Mathf.Min(spaceLeft, quantity);

                    // Add as much as possible to the existing stack
                    itemInSlot.count += quantityToAdd;
                    itemInSlot.RefreshCount();

                    // If there's any leftover quantity, subtract it and continue adding
                    quantity -= quantityToAdd;
                    if (quantity <= 0) {
                        return true;
                    }
                }
            }

            // If there's still quantity left, try to place it in an empty slot
            for (int i = 0; i < inventorySlots.Length; i++) {
                InventorySlot slot = inventorySlots[i];
                InventoryItem itemInSlot = slot.GetComponentInChildren<InventoryItem>();

                // If the slot is empty, place the remaining quantity here
                if (itemInSlot == null) {
                    SpawnNewItem(item, slot, quantity);
                    return true;
                }
            }
        }

        // If there was no space, return false (inventory full)
        return false;
    }

    // Modified method to spawn a new item with a specific quantity
    void SpawnNewItem(Item item, InventorySlot slot, int quantity)
    {
        GameObject newItemGO = Instantiate(inventoryItemPrefab, slot.transform);
        InventoryItem inventoryItem = newItemGO.GetComponentInChildren<InventoryItem>();
        inventoryItem.InitializeItem(item);
        inventoryItem.count = Mathf.Min(quantity, item.maxStackSize); // Set the initial count based on the quantity
        inventoryItem.RefreshCount();
    }

    public InventoryItem GetItemInSlot(int index, bool isItemBar)
    {
        if (isItemBar) {
            // Return the item in the specified item bar slot if it exists
            if (index >= 0 && index < itemBarSlots.Length) {
                return itemBarSlots[index].GetComponentInChildren<InventoryItem>();
            }
        } else {
            // Return the item in the specified inventory slot if it exists
            if (index >= 0 && index < inventorySlots.Length) {
                return inventorySlots[index].GetComponentInChildren<InventoryItem>();
            }
        }
        return null; // Return null if the slot is out of bounds or empty
    }

    private void OnApplicationQuit()
    {
        SaveInventoryToGameSettings();
    }


    public void SaveInventoryToGameSettings()
    {
        GameSettings.inventoryData.Clear(); // Clear existing saved inventory data

        // Loop through each slot in the inventory to save the current state
        for (int i = 0; i < inventorySlots.Length; i++) {
            InventorySlot slot = inventorySlots[i];
            InventoryItem item = slot.GetComponentInChildren<InventoryItem>();

            // Check if the slot has an item to save
            if (item != null && item.item != null) {
                // Create a new SavedInventorySlot with the necessary data
                SavedInventorySlot slotData = new SavedInventorySlot {
                    itemName = item.item.itemName,  // Save the item's name
                    count = item.count,             // Save the item's quantity
                    slotIndex = i                   // Save the slot index to restore the item to the same position
                };

                // Add the saved slot data to the GameSettings inventory data list
                GameSettings.inventoryData.Add(slotData);
            }
        }

        // Save the entire game settings including inventory
        GameSettings.SaveSettings();
    }

    public void LoadInventory()
    {
        foreach (SavedInventorySlot savedSlot in GameSettings.inventoryData) {
            // Ensure the slot index is within bounds
            if (savedSlot.slotIndex >= 0 && savedSlot.slotIndex < inventorySlots.Length) {
                InventorySlot slot = inventorySlots[savedSlot.slotIndex];  // Find the InventorySlot by index

                // Instantiate a new InventoryItem in this slot
                GameObject itemGO = Instantiate(inventoryItemPrefab, slot.transform);
                InventoryItem inventoryItem = itemGO.GetComponent<InventoryItem>();

                // Set up the InventoryItem based on the saved data
                Item itemData = FindItemByName(savedSlot.itemName);  // Retrieve the Item ScriptableObject by name
                if (itemData != null) {
                    inventoryItem.InitializeItem(itemData);
                    inventoryItem.count = savedSlot.count;  // Set the item count based on saved data
                    inventoryItem.RefreshCount();
                }
            }
        }
    }


    private Item FindItemByName(string itemName)
    {
        // Assuming you have a list or array of all possible items somewhere in your project
        foreach (Item item in allItems) {
            if (item.itemName == itemName) {
                return item;
            }
        }
        return null;  // Return null if no item with the name is found
    }

}