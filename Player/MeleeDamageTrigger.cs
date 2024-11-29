using UnityEngine;

public class MeleeDamageTrigger : MonoBehaviour
{
    public int baseDamage;

    private CharacterSkills characterSkills;
    private float damage;
    private bool hasDamaged = false; // A flag to ensure damage is only applied once

    public GameObject player;

    public bool isBat = false;
    public bool isKnife = false;

    void Awake()
    {
        characterSkills = GetComponentInParent<CharacterSkills>();

        if (characterSkills != null) {
            characterSkills.OnSkillsUpdated += UpdateDamageBasedOnStrength;
        }
    }

    private void Start()
    {
        UpdateDamageBasedOnStrength();
    }

    private void UpdateDamageBasedOnStrength()
    {
        float strength = characterSkills.GetSkillLevel("Strength");
        damage = baseDamage + (strength * 3);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hasDamaged && collision != null && collision.CompareTag("Enemy")) {
            UpdateDamageBasedOnStrength();
            if (isBat) {
                SoundManager.Instance.PlaySFX("BatHit", false);
            } else {
                SoundManager.Instance.PlaySFX("KnifeHit", false);
            }
                CmdDealDamage(collision.gameObject, damage);
            hasDamaged = true; // Mark as damaged
        }
    }

    // Command that runs on the server
    private void CmdDealDamage(GameObject enemyObject, float damage)
    {
        // Ensure that the target has a Level1Enemy component
        Level1Enemy enemy = enemyObject.GetComponent<Level1Enemy>();
        if (enemy != null) {
            enemy.TakeDamage(damage, player);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy")) {
            hasDamaged = false; // Reset the flag when exiting the collider
        }
    }
}
