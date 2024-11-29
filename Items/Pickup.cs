using UnityEngine;
using System.Collections;

public class Pickup : MonoBehaviour
{
    public Collider2D coll;
    public Item item;
    public int amount;
    private float pulsateSpeed;
    private float pulsateAmount;
    private Vector3 originalScale;

    private void Start()
    {
        // Ensure this is only executed on the server, as clients shouldn't initialize this logic
        coll.enabled = false;

        // Store the original scale of the object to base the pulsation off of
        originalScale = transform.localScale;

        pulsateSpeed = 3.5f;
        pulsateAmount = 0.1f;

        // Start the pulsating animation
        StartCoroutine(Pulsate());
        StartCoroutine(ColliderDelay());
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) {
            // Notify the player to add the item to their inventory
            InventoryManager inventoryManager = other.GetComponentInChildren<InventoryManager>();
            if (inventoryManager != null) {
                inventoryManager.AddItem(item, amount);
                SoundManager.Instance.PlaySFX("ButtonClick", false);
                Destroy(gameObject);
            }
        }
    }

    private IEnumerator Pulsate()
    {
        while (true) {
            float scaleFactor = 1 + Mathf.Sin(Time.time * pulsateSpeed) * pulsateAmount;
            transform.localScale = originalScale * scaleFactor;
            yield return null;
        }
    }

    private IEnumerator ColliderDelay()
    {
        yield return new WaitForSeconds(1f);
        coll.enabled = true;  // Enable the collider only on the server
    }
}
