using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Animator anim;

    public List<AnimatorOverrideController> overrideControllers = new List<AnimatorOverrideController>();

    public float speed;
    public float attackDamage;
    public Transform target;  // Typically the player

    protected bool isAlive = true;

    private int expAmountOnKill;

    protected float health;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        target = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>();
    }

    protected virtual void Start()
    {
        expAmountOnKill = 20;

        int random = Random.Range(0, overrideControllers.Count);
        if (random == 0) {
            return;
        }
        anim.runtimeAnimatorController = overrideControllers[random];
    }

    protected virtual void Update()
    {
        if (!isAlive)
            return;
    }

    public virtual void TakeDamage(float amount, GameObject attacker)
    {
        health -= amount;

        if (health <= 0 && isAlive) {
            Die();
        }
        //Debug.Log($"Enemy took {amount} damage.");
    }

    protected virtual void UpdateMoving(bool isMoving)
    {
        anim.SetBool("isMoving", isMoving);
    }

    protected virtual void Die()
    {
        FindObjectOfType<CharacterSkills>().AddCharacterExperience(expAmountOnKill);
        isAlive = false;
        anim.SetBool("isAlive", false);
        StartCoroutine(WaitAndDestroy(2.0f));
        // Handle death animation, effects, etc.
    }

    // Coroutine to handle delayed destruction
    private IEnumerator WaitAndDestroy(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        Destroy(gameObject); // Destroy the object after the delay
    }

    protected virtual void MoveTowardsTarget()
    {
        if (target == null || !isAlive) return;

        // Calculate direction from enemy to player (target)
        Vector3 direction = (target.position - transform.position).normalized;

        // Move the enemy towards the player
        transform.position += direction * speed * Time.deltaTime;

        // Rotate the enemy to face the direction of movement
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg + 90f;
        transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    // Override in child classes for more specific attacks or behaviors
    public virtual void Attack()
    {
        // Attack logic, possibly triggering animations and dealing damage
        anim.SetTrigger("attack");
    }
}
