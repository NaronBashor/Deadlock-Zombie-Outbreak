using UnityEngine;
using System.Collections;

public class WeaponManager : MonoBehaviour
{
    private Animator anim;
    private AmmoManager ammoManager;

    public Transform pistolFirePoint;
    public Transform rifleFirePoint;
    public GameObject bulletPrefab;

    private CharacterSkills characterSkills;

    Vector2 damage;

    // New Fields for Reloading
    public int pistolMagazineSize = 10;
    public int rifleMagazineSize = 30;
    private int currentMagazineAmmo;
    private int currentPistolMagazineAmmo;
    private int currentRifleMagazineAmmo;
    private int maxMagazineAmmo;      // Max capacity of the magazine
    private float reloadTime = 2.0f;
    private bool isReloading = false;

    // Track the currently equipped ammo type
    private string currentAmmoType;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        ammoManager = GetComponentInParent<AmmoManager>();

        // Get the CharacterSkills component from the player
        characterSkills = GetComponent<CharacterSkills>();

        // Subscribe to the attribute update event
        if (characterSkills != null) {
            characterSkills.OnSkillsUpdated += UpdateDamageBasedOnDexterity;
        }
    }

    private void Start()
    {
        UpdateDamageBasedOnDexterity();
        currentPistolMagazineAmmo = pistolMagazineSize; // Initialize Pistol magazine
        currentRifleMagazineAmmo = rifleMagazineSize;   // Initialize Rifle magazine
    }


    private void UpdateDamageBasedOnDexterity()
    {
        float dexterity = characterSkills.GetSkillLevel("Dexterity");

        float minDamage = 4 + (dexterity * 3);
        float maxDamage = 6 + (dexterity * 4);

        damage = new Vector2(minDamage, maxDamage);
    }

    public void UpdateCurrentWeapon(Item selectedItem)
    {
        if (selectedItem == null) {
            // No item selected, revert to default animation
            anim.SetInteger("currentWeapon", 0); // Default to melee or unarmed
            currentAmmoType = null;
            return;
        }

        // Determine weapon type based on the selected item
        anim.SetInteger("currentWeapon", selectedItem.weaponAnimationId);

        // Set the current ammo type and the current magazine ammo based on the weapon
        switch (selectedItem.weaponAnimationId) {
            case 2: // Pistol
                currentAmmoType = "Pistol";
                maxMagazineAmmo = pistolMagazineSize;
                currentMagazineAmmo = currentPistolMagazineAmmo; // Use the pistol's magazine
                break;
            case 3: // Rifle
                currentAmmoType = "Rifle";
                maxMagazineAmmo = rifleMagazineSize;
                currentMagazineAmmo = currentRifleMagazineAmmo; // Use the rifle's magazine
                break;
            default:
                currentAmmoType = null;
                break;
        }
    }


    public void Attack(bool isPressed)
    {
        // If the weapon is reloading, skip the attack
        if (isReloading) return;

        // Allow melee weapons (bat and knife) even if currentAmmoType is null
        int currentWeapon = anim.GetInteger("currentWeapon");

        // If it's a ranged weapon and there's no ammo type, skip the attack
        if (currentAmmoType == null && (currentWeapon == 2 || currentWeapon == 3 || currentWeapon == 4)) return;

        // Check if no item is selected (default to bat)
        //if (currentWeapon == -1) {
        //    currentWeapon = 0; // Default to bat if no weapon is equipped
        //}

        // Proceed with attack logic
        if (isPressed) {
            switch (currentWeapon) {
                case 0: // Bat (default attack)
                    PerformMeleeAttack("batAttack");
                    break;
                case 1: // Knife
                    PerformMeleeAttack("knifeAttack");
                    break;
                case 2: // Pistol
                    Shoot("Pistol", pistolFirePoint, 1);
                    break;
                case 3: // Rifle
                    Shoot("Rifle", rifleFirePoint, 1);
                    //StartRifle();
                    break;
                case 4: // Flamethrower
                    StartFlamethrower();
                    break;
            }
        } else {
            // Stop flamethrower when attack button is released
            //if (currentWeapon == 3) {
            //    StopRifle();
            //}
            if (currentWeapon == 4) {
                StopFlamethrower();
            }
        }
    }


    private void PerformMeleeAttack(string triggerName)
    {
        SoundManager.Instance.PlaySFX("BatSwing", false);
        anim.SetTrigger(triggerName);
    }

    private void Shoot(string ammoType, Transform firePoint, int ammoPerShot)
    {
        if (currentMagazineAmmo <= 0) {
            StartCoroutine(ReloadWeapon(ammoType)); // If no ammo in magazine, start reloading
            return;
        }

        currentMagazineAmmo--; // Reduce ammo in the magazine

        // Save the updated ammo count based on the current weapon
        if (currentAmmoType == "Pistol") {
            currentPistolMagazineAmmo = currentMagazineAmmo;
        } else if (currentAmmoType == "Rifle") {
            currentRifleMagazineAmmo = currentMagazineAmmo;
        }

        int weapon = anim.GetInteger("currentWeapon");
        if (weapon == 2) {
            SoundManager.Instance.PlaySFX("GunShot", false);
            anim.SetTrigger("shootGun");
        } else if (weapon == 3) {
            SoundManager.Instance.PlaySFX("GunShot", false);
            anim.SetTrigger("rifleAttack");
        }
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Bullet>().damage = damage;
        bullet.GetComponent<Bullet>().player = this.gameObject;
        Physics2D.IgnoreCollision(bullet.GetComponent<Collider2D>(), GetComponent<Collider2D>());
    }


    private IEnumerator ReloadWeapon(string ammoType)
    {
        if (isReloading) yield break;
        isReloading = true;
        SoundManager.Instance.PlaySFX("Reload", false);

        int availableAmmo = ammoManager.GetAmmo(ammoType);

        // Check if there is any ammo to reload
        if (availableAmmo <= 0) {
            Debug.Log("Out of ammo! Cannot reload.");
            isReloading = false;
            yield break; // Exit if there's no ammo left to reload
        }

        Debug.Log($"Reloading {ammoType}...");

        yield return new WaitForSeconds(reloadTime); // Wait for reload time

        // Calculate the ammo needed to fill the magazine
        int ammoNeeded = maxMagazineAmmo - currentMagazineAmmo;

        // Use the ammo from the inventory to refill the magazine
        int ammoToReload = Mathf.Min(availableAmmo, ammoNeeded);
        currentMagazineAmmo += ammoToReload;
        ammoManager.UseAmmo(ammoType, ammoToReload); // Consume ammo from inventory

        // Save the updated ammo count for the correct weapon
        if (ammoType == "Pistol") {
            currentPistolMagazineAmmo = currentMagazineAmmo;
        } else if (ammoType == "Rifle") {
            currentRifleMagazineAmmo = currentMagazineAmmo;
        }

        isReloading = false;
    }


    private void StartRifle()
    {
        anim.SetTrigger("rifleAttack");
        anim.SetBool("flaming", true);
    }

    private void StopRifle()
    {
        anim.SetBool("flaming", false);
    }

    private void StartFlamethrower()
    {
        anim.SetTrigger("flameThrowAttack");
        anim.SetBool("flaming", true);
    }

    private void StopFlamethrower()
    {
        anim.SetBool("flaming", false);
    }

    private void OnDestroy()
    {
        // Unsubscribe from the event to avoid memory leaks
        if (characterSkills != null) {
            characterSkills.OnSkillsUpdated -= UpdateDamageBasedOnDexterity;
        }
    }

    public int GetCurrentMagazineAmmo()
    {
        return currentMagazineAmmo;
    }

    public int GetCurrentWeaponID()
    {
        return anim.GetInteger("currentWeapon");
    }
}