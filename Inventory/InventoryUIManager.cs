using UnityEngine;

public class InventoryUIManager : MonoBehaviour
{
    public InventorySlot[] inventorySlots;    // UI slots in the inventory grid
    public InventorySlot[] itemBarSlots;      // UI slots in the item bar

    private InventoryManager inventoryManager;

    private void Start()
    {
        // Reference the InventoryManager component in the same parent
        inventoryManager = GetComponentInParent<InventoryManager>();

        // Initialize the UI slots with the inventory data at the start
        UpdateUI();
    }

    // Call this function to update the UI slots based on InventoryManager's data
    public void UpdateUI()
    {
        // Update the inventory grid
        for (int i = 0; i < inventorySlots.Length; i++) {
            InventorySlot slot = inventorySlots[i];
            InventoryItem itemInSlot = inventoryManager.GetItemInSlot(i, false); // Retrieve item from the inventory

            // Update the slot's UI based on the retrieved item data
            if (itemInSlot != null) {
                slot.SetItem(itemInSlot);
            } else {
                slot.ClearSlot(); // Clear the slot if it's empty
            }
        }

        // Update the item bar
        for (int i = 0; i < itemBarSlots.Length; i++) {
            InventorySlot slot = itemBarSlots[i];
            InventoryItem itemInSlot = inventoryManager.GetItemInSlot(i, true); // Retrieve item from the item bar

            // Update the slot's UI based on the retrieved item data
            if (itemInSlot != null) {
                slot.SetItem(itemInSlot);
            } else {
                slot.ClearSlot(); // Clear the slot if it's empty
            }
        }
    }
}
