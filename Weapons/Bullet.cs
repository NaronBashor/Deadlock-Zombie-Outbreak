using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 20f;          // Speed of the bullet
    public Vector2 damage = Vector2.zero;
    public Rigidbody2D rb;             // Rigidbody component for bullet physics
    public GameObject impactEffect;    // Optional: an effect that plays upon impact
    public GameObject player;

    private void Start()
    {
        // Rotate the bullet 90 degrees to the left (Z-axis)
        transform.Rotate(0, 0, -90);

        // Make the bullet move in the right direction (adjust based on your bullet direction)
        rb.velocity = transform.right * speed;
    }

    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        //Debug.Log("Bullet hit: " + hitInfo.name);

        if (hitInfo.CompareTag("Enemy")) {
            //Debug.Log("Hit an enemy!");

            Level1Enemy enemy = hitInfo.GetComponent<Level1Enemy>();
            if (enemy != null) {
                //Debug.Log("Dealing damage: " + damage);
                SoundManager.Instance.PlaySFX("BulletHit", false);
                enemy.TakeDamage(Random.Range(damage.x, (damage.y + 1)), player);
            }
        }

        // Create impact effect and destroy the bullet
        if (impactEffect != null) {
            GameObject fire = Instantiate(impactEffect, transform.position, transform.rotation);
            Destroy(fire, 0.0625f);
        }

        Destroy(gameObject);
    }

}
