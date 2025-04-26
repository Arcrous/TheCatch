using System.Collections.Generic;
using UnityEngine;

public class FishSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FishSpawnInfo
    {
        public GameObject fishPrefab;
        public FishData fishData;
        public float spawnWeight = 1f;
    }

    [Header("Spawn Settings")]
    [SerializeField] private List<FishSpawnInfo> regularFish = new List<FishSpawnInfo>();
    [SerializeField] private List<FishSpawnInfo> specialFish = new List<FishSpawnInfo>();
    [SerializeField] private List<FishSpawnInfo> aggressiveFish = new List<FishSpawnInfo>();

    [Header("Spawn Parameters")]
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int maxFishCount = 15;
    [SerializeField] private float specialFishChance = 0.1f;
    [SerializeField] private float aggressiveFishChance = 0.2f;
    [SerializeField] private Vector2 spawnAreaMin = new Vector2(-8f, -15f);
    [SerializeField] private Vector2 spawnAreaMax = new Vector2(8f, -5f);
    [SerializeField] private float minDistanceBetweenFish = 3f;

    private float spawnTimer;
    private List<GameObject> activeFish = new List<GameObject>();

    private void Start()
    {
        spawnTimer = spawnInterval;

        // Initial fish population
        for (int i = 0; i < maxFishCount / 2; i++)
        {
            SpawnRandomFish();
        }
    }

    // Add this method to handle fish despawn notification
    public void OnFishDespawned()
    {
        spawnTimer = 0; // Trigger immediate respawn attempt
    }

    private void FixedUpdate()
    {
        // Clean up list of destroyed fish
        activeFish.RemoveAll(fish => fish == null);

        // Spawn fish on timer if below max count
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && activeFish.Count < maxFishCount)
        {
            if (SpawnRandomFish())
            {
                spawnTimer = spawnInterval;
            }
            else
            {
                spawnTimer = 0.5f; // Try again soon if spawn failed
            }
        }
    }

    private bool SpawnRandomFish()
    {
        // Determine fish type
        float fishTypeRoll = Random.value;
        List<FishSpawnInfo> selectedList;

        if (fishTypeRoll < specialFishChance)
        {
            selectedList = specialFish;
        }
        else if (fishTypeRoll < specialFishChance + aggressiveFishChance)
        {
            selectedList = aggressiveFish;
        }
        else
        {
            selectedList = regularFish;
        }

        // If the selected list is empty, default to regular fish
        if (selectedList.Count == 0)
        {
            selectedList = regularFish;
        }

        // Choose a specific fish based on weights
        float totalWeight = 0;
        foreach (var fishInfo in selectedList)
        {
            totalWeight += fishInfo.spawnWeight;
        }

        float roll = Random.Range(0, totalWeight);
        float cumulativeWeight = 0;

        FishSpawnInfo selectedFish = selectedList[0]; // Default
        foreach (var fishInfo in selectedList)
        {
            cumulativeWeight += fishInfo.spawnWeight;
            if (roll <= cumulativeWeight)
            {
                selectedFish = fishInfo;
                break;
            }
        }

        // Try multiple spawn positions if needed
        const int maxAttempts = 10;
        bool validPosition = false;
        Vector2 spawnPos = Vector2.zero;

        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            spawnPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            // Check distance to other fish
            validPosition = true;
            foreach (GameObject existingFish in activeFish)
            {
                if (existingFish == null) continue;

                float distance = Vector2.Distance(spawnPos, existingFish.transform.position);
                if (distance < minDistanceBetweenFish)
                {
                    validPosition = false;
                    break;
                }
            }

            if (validPosition)
                break;
        }

        if (!validPosition)
            return false; // Couldn't find valid position

        // Spawn the fish
        GameObject newFish = Instantiate(selectedFish.fishPrefab, spawnPos, Quaternion.identity);

        // Configure the fish
        Fish fishComponent = newFish.GetComponent<Fish>();
        if (fishComponent != null && selectedFish.fishData != null)
        {
            fishComponent.fishData = selectedFish.fishData;
            fishComponent.minimumHookLevel = selectedFish.fishData.requiredHookLevel;

            // Random size variation
            float sizeVariation = Random.Range(
                selectedFish.fishData.sizeRange.x,
                selectedFish.fishData.sizeRange.y
            );
            newFish.transform.localScale *= sizeVariation;
        }

        // Add to active fish list
        activeFish.Add(newFish);
        return true;
    }

    // For visualizing spawn area in editor
    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        // Draw minimum distance circles around existing fish
        Gizmos.color = new Color(1, 1, 0, 0.2f); // Semi-transparent yellow
        foreach (GameObject fish in activeFish)
        {
            if (fish != null)
            {
                Gizmos.DrawWireSphere(fish.transform.position, minDistanceBetweenFish);
            }
        }
    }
}