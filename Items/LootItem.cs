using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Loot Item")]
public class LootItem : ScriptableObject
{
    public Item item;              // Reference to the item data
    public int minQuantity = 1;     // Minimum quantity that can be looted
    public int maxQuantity = 5;     // Maximum quantity that can be looted
    public float dropChance = 1.0f; // Drop chance as a probability (e.g., 0.5 = 50%)
    public GameObject lootPrefab;

    // Generates a random quantity for this item
    public int GetRandomQuantity()
    {
        return Random.Range(minQuantity, maxQuantity + 1);
    }
}
