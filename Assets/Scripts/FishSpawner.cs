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

    private void Update()
    {
        // Clean up list of destroyed fish
        activeFish.RemoveAll(fish => fish == null);

        // Spawn fish on timer if below max count
        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && activeFish.Count < maxFishCount)
        {
            SpawnRandomFish();
            spawnTimer = spawnInterval;
        }
    }

    private void SpawnRandomFish()
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

        // Determine spawn position
        Vector2 spawnPos = new Vector2(
            Random.Range(spawnAreaMin.x, spawnAreaMax.x),
            Random.Range(spawnAreaMin.y, spawnAreaMax.y)
        );

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
    }

    // For visualizing spawn area in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(
            new Vector3((spawnAreaMin.x + spawnAreaMax.x) / 2, (spawnAreaMin.y + spawnAreaMax.y) / 2, 0),
            new Vector3(spawnAreaMax.x - spawnAreaMin.x, spawnAreaMax.y - spawnAreaMin.y, 0)
        );
    }
}