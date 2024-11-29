using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    public GameObject enemyPrefab; // The enemy prefab to spawn
    public Transform[] spawnPoints; // Array of predefined spawn points
    private Transform player;
    private Camera mainCamera;

    public float spawnInterval = 5f; // Default time interval between spawns
    public float nightSpawnInterval = 3f; // Faster spawn interval at night
    private float currentSpawnInterval; // Variable to track the current spawn interval

    public int maxEnemies = 30; // Maximum number of enemies allowed on the map
    private List<GameObject> activeEnemies = new List<GameObject>(); // List to track active enemies

    private DayNightCycle dayNightCycle; // Reference to the DayNightCycle script

    private void Start()
    {
        spawnInterval = 20;
        nightSpawnInterval = 10;
        // Find the DayNightCycle instance
        dayNightCycle = FindObjectOfType<DayNightCycle>();

        // Set the initial spawn interval and start the repeating spawn function
        currentSpawnInterval = spawnInterval;
        player = GameObject.FindGameObjectWithTag("Player").transform;
        mainCamera = Camera.main;
        InvokeRepeating(nameof(SpawnEnemies), 2f, currentSpawnInterval);
    }

    private void Update()
    {
        // Check if the time of day has changed and adjust the spawn interval if needed
        if (dayNightCycle.CurrentTimeOfDay == DayNightCycle.TimeOfDay.Night) {
            // Set to faster spawn interval during nighttime
            currentSpawnInterval = nightSpawnInterval;
        } else {
            // Reset to default spawn interval during daytime
            currentSpawnInterval = spawnInterval;
        }

        // Adjust the repeating spawn interval based on the time of day
        CancelInvoke(nameof(SpawnEnemies));
        InvokeRepeating(nameof(SpawnEnemies), 0f, currentSpawnInterval);

        // Remove destroyed enemies from the list to keep it up-to-date
        activeEnemies.RemoveAll(enemy => enemy == null);
    }

    private void SpawnEnemies()
    {
        // Check if the maximum enemy limit has been reached
        if (activeEnemies.Count >= maxEnemies)
            return;

        foreach (Transform spawnPoint in spawnPoints) {
            // Check if the spawn point is outside the camera's view
            Vector3 viewportPoint = mainCamera.WorldToViewportPoint(spawnPoint.position);
            if (viewportPoint.x < 0 || viewportPoint.x > 1 || viewportPoint.y < 0 || viewportPoint.y > 1 || viewportPoint.z < 0) {
                // Spawn the enemy at the specified spawn point if under max enemy count and outside of camera view
                GameObject zombie = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);

                // Set the player's transform as the target for the newly spawned zombie
                zombie.GetComponent<Level1Enemy>().target = player;

                // Add the spawned enemy to the list
                activeEnemies.Add(zombie);

                // Stop spawning if we’ve reached the max limit
                if (activeEnemies.Count >= maxEnemies)
                    break;
            }
        }
    }
}
