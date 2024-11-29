using System.Collections.Generic;
using UnityEngine;

public class LootSystem : MonoBehaviour
{
    public List<LootItem> lootTable;   // List of loot items with their probabilities and quantities
    public int maxLootItems = 3;       // Max number of items that can be looted at once

    // Method to generate loot
    public List<LootItem> GenerateLoot()
    {
        List<LootItem> lootItems = new List<LootItem>();

        // Shuffle the lootTable to make selection random
        List<LootItem> shuffledLootTable = new List<LootItem>(lootTable);
        Shuffle(shuffledLootTable);

        // Iterate through the shuffled table and randomly add items to loot list
        foreach (var lootItem in shuffledLootTable) {
            if (lootItems.Count >= maxLootItems) break;

            // Use probability to decide if this item should be added
            if (Random.value <= lootItem.dropChance) {
                lootItems.Add(lootItem);
            }
        }

        return lootItems;
    }

    // Helper function to shuffle the list
    private void Shuffle(List<LootItem> list)
    {
        for (int i = list.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            LootItem temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
