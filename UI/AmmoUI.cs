using TMPro;
using UnityEngine;

public class AmmoUI : MonoBehaviour
{
    public TextMeshProUGUI ammoText;                 // Reference to the UI Text element for current weapon ammo
    public TextMeshProUGUI pistolAmmoText;           // Reference to the UI Text element for Pistol ammo count
    public TextMeshProUGUI rifleAmmoText;            // Reference to the UI Text element for Rifle ammo count
    public TextMeshProUGUI flamethrowerAmmoText;     // Reference to the UI Text element for Flamethrower ammo count

    public WeaponManager weaponManager;              // Reference to the WeaponManager
    public AmmoManager ammoManager;                  // Reference to the AmmoManager

    private string currentAmmoType = null;           // The type of ammo the current weapon uses

    private void Update()
    {
        if (weaponManager == null || ammoManager == null) {
            Debug.LogError("WeaponManager or AmmoManager reference is missing in AmmoUI.");
            return;
        }

        // Update the current ammo type based on the equipped weapon
        UpdateCurrentAmmoType();

        // If no applicable weapon is being held, clear the ammo display
        if (string.IsNullOrEmpty(currentAmmoType)) {
            ammoText.text = "";  // Clear the current weapon's ammo display
        } else {
            // Get the current ammo in the magazine
            int currentAmmoInMagazine = weaponManager.GetCurrentMagazineAmmo();

            // Get the total ammo in the inventory for the current weapon type
            int totalAmmoInInventory = ammoManager.GetAmmo(currentAmmoType);

            // Update the UI Text to show "current / total"
            ammoText.text = $"{currentAmmoInMagazine} / {totalAmmoInInventory}";
        }

        // Update individual ammo counts
        UpdateAmmoCounts();
    }

    // Method to update the current ammo type based on the equipped weapon
    private void UpdateCurrentAmmoType()
    {
        if (weaponManager == null) {
            Debug.LogError("WeaponManager reference is missing in AmmoUI.");
            return;
        }

        int currentWeapon = weaponManager.GetCurrentWeaponID();
        switch (currentWeapon) {
            case 2:
                currentAmmoType = "Pistol";
                break;
            case 3:
                currentAmmoType = "Rifle";
                break;
            case 4:
                currentAmmoType = "FlameThrower";
                break;
            default:
                currentAmmoType = null;  // No valid weapon equipped
                break;
        }
    }

    // Method to update the total ammo counts for each type in the UI
    private void UpdateAmmoCounts()
    {
        // Get the total ammo counts from the AmmoManager
        int pistolTotalAmmo = ammoManager.GetAmmo("Pistol");
        int rifleTotalAmmo = ammoManager.GetAmmo("Rifle");
        int flamethrowerTotalAmmo = ammoManager.GetAmmo("FlameThrower");

        // Update each text field with the corresponding ammo count
        if (pistolAmmoText != null) {
            pistolAmmoText.text = pistolTotalAmmo.ToString();
        }

        if (rifleAmmoText != null) {
            rifleAmmoText.text = rifleTotalAmmo.ToString();
        }

        if (flamethrowerAmmoText != null) {
            flamethrowerAmmoText.text = flamethrowerTotalAmmo.ToString();
        }
    }
}
