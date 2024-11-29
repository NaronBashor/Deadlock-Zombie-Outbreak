using System.Collections.Generic;
using UnityEngine;

public class AmmoManager : MonoBehaviour
{
    // Dictionary to keep track of ammo by type
    private Dictionary<string, int> ammoInventory = new Dictionary<string, int>();

    private void Awake()
    {
        // Initialize different ammo types with default counts
        ammoInventory.Add("Pistol", 0);
        ammoInventory.Add("Rifle", 0);
        ammoInventory.Add("FlameThrower", 0);
    }

    public void AddAmmo(string ammoType, int amount)
    {
        if (ammoInventory.ContainsKey(ammoType)) {
            ammoInventory[ammoType] += amount;
        } else {
            ammoInventory[ammoType] = amount;
        }
    }

    public bool UseAmmo(string ammoType, int amount)
    {
        if (ammoInventory.ContainsKey(ammoType) && ammoInventory[ammoType] >= amount) {
            ammoInventory[ammoType] -= amount;
            return true;
        }
        return false;
    }

    public int GetAmmo(string ammoType)
    {
        if (string.IsNullOrEmpty(ammoType)) {
            //Debug.LogError("Ammo type is null or empty!");
            return 0;
        }

        return ammoInventory.ContainsKey(ammoType) ? ammoInventory[ammoType] : 0;
    }

}
