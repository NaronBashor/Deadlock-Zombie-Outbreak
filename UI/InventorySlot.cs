using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventorySlot : MonoBehaviour, IDropHandler
{
    public Image image;
    public Color selectedColor, notSelectedColor;

    public Image itemIcon;
    public Text itemCountText;

    public void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        image.color = selectedColor;
    }

    public void Deselect()
    {
        image.color = notSelectedColor;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (transform.childCount == 0) {
            InventoryItem inventoryItem = eventData.pointerDrag.GetComponent<InventoryItem>();
            inventoryItem.parentAfterDrag = transform;
        }
    }

    // Update the slot to display an item
    public void SetItem(InventoryItem item)
    {
        itemIcon.sprite = item.item.image;
        itemIcon.enabled = true;
        itemCountText.text = item.count > 1 ? item.count.ToString() : ""; // Display count if greater than 1
        itemCountText.enabled = item.count > 1;
    }

    // Clear the slot if no item is present
    public void ClearSlot()
    {
        itemIcon.sprite = null;
        itemIcon.enabled = false;
        itemCountText.text = "";
        itemCountText.enabled = false;
    }
}

[System.Serializable]
public class SavedInventorySlot
{
    public string itemName;  // Name of the item in the slot
    public int count;        // Quantity of the item
    public int slotIndex;    // Slot index for restoring to the correct position
}

