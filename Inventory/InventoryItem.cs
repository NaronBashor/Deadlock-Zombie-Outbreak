using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public Image image;
    public TextMeshProUGUI countText;

    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public Item item;
    [HideInInspector] public int count = 1;

    private InventoryManager inventoryManager;

    private void Start()
    {
        inventoryManager = GameObject.Find("InventoryManager").GetComponent<InventoryManager>();
        RefreshCount();
    }

    public void InitializeItem(Item newItem)
    {
        item = newItem;
        image.sprite = newItem.image;
        RefreshCount();
    }

    public void RefreshCount()
    {
        countText.text = count.ToString();
        bool textActive = count > 1;
        countText.gameObject.SetActive(textActive);
    }

    // Hover Detection
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (inventoryManager != null) {
            inventoryManager.SetHoveredItem(this); // Set this item as the hovered item
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (inventoryManager != null) {
            inventoryManager.ClearHoveredItem(); // Clear the hovered item when pointer exits
        }
    }

    // Begin dragging an item
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (inventoryManager.IsInventoryOpen) {
            image.raycastTarget = false;
            parentAfterDrag = transform.parent;  // Save the original parent
            transform.SetParent(GameObject.Find("Inventory Panel").GetComponent<Transform>());  // Move to root for dragging
        }
    }

    // While dragging an item
    public void OnDrag(PointerEventData eventData)
    {
        if (inventoryManager.IsInventoryOpen) {
            // Follow mouse position during drag
            transform.position = Input.mousePosition;
        }
    }

    // End dragging and drop the item
    public void OnEndDrag(PointerEventData eventData)
    {
        image.raycastTarget = true;

        if (inventoryManager.IsInventoryOpen) {
            // Check if the item is over a valid drop slot
            if (eventData.pointerEnter != null) {
                InventorySlot dropSlot = eventData.pointerEnter.GetComponent<InventorySlot>();
                if (dropSlot != null && dropSlot.transform.childCount == 0) {
                    // Successfully dropped into a new slot
                    transform.SetParent(dropSlot.transform);
                    transform.localPosition = Vector3.zero;
                    parentAfterDrag = dropSlot.transform;  // Update parent after successful drop
                } else {
                    // Revert to the original slot if not dropped on a valid empty slot
                    transform.SetParent(parentAfterDrag);
                    transform.localPosition = Vector3.zero;
                }
            } else {
                // Revert to the original slot if not dropped on a valid target
                transform.SetParent(parentAfterDrag);
                transform.localPosition = Vector3.zero;
            }
        } else {
            // If inventory is closed during drag, drop item into the world
            inventoryManager.DropDraggedItemIntoWorld(this);
        }
    }
}
