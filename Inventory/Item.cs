using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Object/Item")]
public class Item : ScriptableObject
{
    [Header("General Properties")]
    public string itemName;              // Name of the item
    public Sprite image;                 // The image representing the item
    public ItemType itemType;            // Type of the item (Weapon, Consumable, Ammo, etc.)
    public bool stackable = false;       // Can the item be stacked?
    public int maxStackSize = 1;         // Maximum stack size (only relevant if stackable)

    [Header("Weapon Properties (if applicable)")]
    public bool isWeapon = false;        // Is this item a weapon?
    public int weaponAnimationId; // Unique ID for animations (e.g., 0 = unarmed, 1 = bat, 2 = pistol)
    public int maxAmmo = 0;              // Maximum ammo capacity if it's a weapon
    public float damage = 0f;            // Damage dealt by the weapon
    public float fireRate = 0f;          // Fire rate of the weapon
    public float range = 0f;             // Effective range of the weapon
    public WeaponType weaponType;        // Type of weapon (e.g., Pistol, Rifle, Melee)

    [Header("Consumable Properties")]
    public int healAmount = 0;           // Health gained if it's a consumable item

    [Header("Ammo Properties")]
    public string ammoType;              // Name/type of ammo (if applicable)
    public int ammoAmount = 0;           // Quantity of ammo for ammo pickups

    public enum ItemType
    {
        Weapon,
        Consumable,
        Ammo,
        KeyItem,      // Special items that can't be used or stacked (like quest items)
        Crafting,     // Items used for crafting purposes
        Other         // Any miscellaneous items
    }

    public enum WeaponType
    {
        None,
        Pistol,
        Rifle,
        Shotgun,
        Melee,
        FlameThrower,
        Other
    }
}
