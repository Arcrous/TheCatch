using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class FishSpawner : MonoBehaviour
{
    [System.Serializable]
    public class FishSpawnInfo
    {
        public GameObject fishPrefab;
        public FishData fishData;
        public float spawnWeight = 1f;
    };

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

    [Header("Camera")]
    [SerializeField] private Camera gameCamera;
    [SerializeField] private float cameraBuffer = 2f;
    [SerializeField] private bool debugCameraView = true;

    // List of specific spawn points outside the camera view
    [Header("Spawn Points")]
    [SerializeField] private List<Transform> spawnPoints = new List<Transform>();
    [SerializeField] private bool useSpawnPointsOnly = false;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI specialFishText; // Reference to UI Text component
    [SerializeField] private TextMeshProUGUI specialFishTimer;
    [SerializeField] private float textFadeDuration = 2f; // Duration of text fade
    [SerializeField] private AudioSource specialFishSound; // Sound to play when special fish spawns

    private GameObject activeSpecialFish; // Track the active special fish
    private float spawnTimer;
    private List<GameObject> activeFish = new List<GameObject>();

    private void Awake()
    {
        if (gameCamera == null)
            gameCamera = Camera.main;
    }

    private void Start()
    {
        spawnTimer = spawnInterval;

        // Initial fish population
        for (int i = 0; i < maxFishCount / 2; i++)
        {
            SpawnRandomFish();
        }
    }

    public void OnFishDespawned()
    {
        spawnTimer = 0;
    }

    private void FixedUpdate()
    {
        // Check if special fish was destroyed
        if (activeSpecialFish != null && activeSpecialFish == null)
        {
            activeSpecialFish = null;
        }

        activeFish.RemoveAll(fish => fish == null);

        spawnTimer -= Time.deltaTime;
        if (spawnTimer <= 0 && activeFish.Count < maxFishCount)
        {
            if (SpawnRandomFish())
            {
                spawnTimer = spawnInterval;
            }
            else
            {
                spawnTimer = 0.5f;
            }
        }
    }

    // Brute force check if position is visible to camera
    private bool IsVisibleToCamera(Vector3 worldPos)
    {
        if (gameCamera == null) return false;

        Vector3 viewportPoint = gameCamera.WorldToViewportPoint(worldPos);

        // Add a buffer to the viewport to ensure we're well outside
        float buffer = cameraBuffer * 0.01f; // Convert to viewport space (0-1)

        bool isVisible = (viewportPoint.x > -buffer && viewportPoint.x < 1 + buffer &&
                          viewportPoint.y > -buffer && viewportPoint.y < 1 + buffer &&
                          viewportPoint.z > 0);

        return isVisible;
    }

    private Vector2 GetRandomOffScreenPosition()
    {
        // If using spawn points and we have some defined, use those
        if (useSpawnPointsOnly && spawnPoints.Count > 0)
        {
            // Filter out visible spawn points
            List<Transform> validSpawnPoints = new List<Transform>();
            foreach (Transform point in spawnPoints)
            {
                if (!IsVisibleToCamera(point.position))
                {
                    validSpawnPoints.Add(point);
                }
            }

            // If we have valid spawn points, use one of them
            if (validSpawnPoints.Count > 0)
            {
                int randomIndex = Random.Range(0, validSpawnPoints.Count);
                return validSpawnPoints[randomIndex].position;
            }
            else
            {
                Debug.LogWarning("All spawn points are visible to camera! Trying random position instead.");
                // Fall through to random position logic
            }
        }

        // Initialize variables
        Vector2 spawnPos = Vector2.zero;
        bool positionFound = false;
        int attempts = 0;

        // Brute force approach - try random positions until one is off-screen
        while (!positionFound && attempts < 100)
        {
            attempts++;

            // Generate random position within spawn area
            spawnPos = new Vector2(
                Random.Range(spawnAreaMin.x, spawnAreaMax.x),
                Random.Range(spawnAreaMin.y, spawnAreaMax.y)
            );

            // Check if position is visible to camera
            if (!IsVisibleToCamera(spawnPos))
            {
                positionFound = true;
                Debug.Log($"Found off-screen position at {spawnPos} after {attempts} attempts");
            }
        }

        if (!positionFound)
        {
            Debug.LogWarning("Could not find off-screen position after 100 attempts!");

            // Last resort: pick a point at the far edge of the spawn area
            float edge = Random.Range(0, 4);
            switch ((int)edge)
            {
                case 0: // Top
                    spawnPos = new Vector2(Random.Range(spawnAreaMin.x, spawnAreaMax.x), spawnAreaMax.y);
                    break;
                case 1: // Right
                    spawnPos = new Vector2(spawnAreaMax.x, Random.Range(spawnAreaMin.y, spawnAreaMax.y));
                    break;
                case 2: // Bottom
                    spawnPos = new Vector2(Random.Range(spawnAreaMin.x, spawnAreaMax.x), spawnAreaMin.y);
                    break;
                case 3: // Left
                    spawnPos = new Vector2(spawnAreaMin.x, Random.Range(spawnAreaMin.y, spawnAreaMax.y));
                    break;
            }
        }

        return spawnPos;
    }

    private bool SpawnRandomFish()
    {
        // Select fish type
        float fishTypeRoll = Random.value;
        List<FishSpawnInfo> selectedList;

        if (fishTypeRoll < specialFishChance && activeSpecialFish == null)
            selectedList = specialFish;
        else if (fishTypeRoll < specialFishChance + aggressiveFishChance)
            selectedList = aggressiveFish;
        else
            selectedList = regularFish;

        // If we rolled special but one exists, default to regular
        if (fishTypeRoll < specialFishChance && activeSpecialFish != null)
            selectedList = regularFish;

        if (selectedList.Count == 0)
            selectedList = regularFish;

        if (selectedList.Count == 0) return false; // No fish to spawn

        // Choose specific fish based on weights
        float totalWeight = 0;
        foreach (var fishInfo in selectedList)
        {
            totalWeight += fishInfo.spawnWeight;
        }

        float roll = Random.Range(0, totalWeight);
        float cumulativeWeight = 0;
        FishSpawnInfo selectedFish = selectedList[0];

        foreach (var fishInfo in selectedList)
        {
            cumulativeWeight += fishInfo.spawnWeight;
            if (roll <= cumulativeWeight)
            {
                selectedFish = fishInfo;
                break;
            }
        }

        // Get a position off screen
        Vector2 spawnPos = GetRandomOffScreenPosition();

        // Verify this position is far enough from other fish
        bool validPosition = true;
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

        if (!validPosition)
        {
            Debug.Log("Position was too close to other fish - skipping spawn");
            return false;
        }

        // Double-check the position is off-screen
        if (IsVisibleToCamera(spawnPos))
        {
            Debug.LogError($"FAILED: Position {spawnPos} is still visible to camera!");
            return false; // This prevents spawning in visible areas
        }

        // Spawn the fish
        GameObject newFish = Instantiate(selectedFish.fishPrefab, spawnPos, Quaternion.identity);

        // Configure the fish
        Fish fishComponent = newFish.GetComponent<Fish>();

        // Check if this is a special fish
        if (selectedList == specialFish)
        {
            activeSpecialFish = newFish;
            StartCoroutine(ShowSpecialFishText());
            StartCoroutine(ShowSpecialFishTimer()); // Start timer coroutine
        }

        if (fishComponent != null && selectedFish.fishData != null)
        {
            fishComponent.fishData = selectedFish.fishData;
            fishComponent.minimumHookLevel = selectedFish.fishData.requiredHookLevel;

            float sizeVariation = Random.Range(
                selectedFish.fishData.sizeRange.x,
                selectedFish.fishData.sizeRange.y
            );
            newFish.transform.localScale *= sizeVariation;
        }

        activeFish.Add(newFish);
        return true;
    }

    private System.Collections.IEnumerator ShowSpecialFishText()
    {
        if (specialFishText != null)
        {
            specialFishSound.Play(); // Play sound effect
            specialFishText.gameObject.SetActive(true);

            specialFishText.text = "Special Fish Appeared!";
            specialFishText.color = new Color(specialFishText.color.r, specialFishText.color.g, specialFishText.color.b, 1f);

            // Wait briefly before starting fade
            yield return new WaitForSeconds(2f);

            // Fade out announcement text only
            float elapsedTime = 0f;
            Color startColor = specialFishText.color;
            Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0f);

            while (elapsedTime < textFadeDuration)
            {
                elapsedTime += Time.deltaTime;
                float normalizedTime = elapsedTime / textFadeDuration;
                specialFishText.color = Color.Lerp(startColor, endColor, normalizedTime);
                yield return null;
            }

            specialFishText.gameObject.SetActive(false);
        }
    }

    private System.Collections.IEnumerator ShowSpecialFishTimer()
    {
        if (specialFishTimer != null)
        {
            specialFishTimer.gameObject.SetActive(true);
            // Keep updating timer until fish is gone
            while (activeSpecialFish != null)
            {
                Fish fish = activeSpecialFish.GetComponent<Fish>();
                if (fish != null)
                {
                    float remainingTime = fish.maxLifetime - fish.lifetime;
                    if (remainingTime <= 0)
                    {
                        specialFishTimer.gameObject.SetActive(false);
                        break;
                    }
                    specialFishTimer.text = FormatTime(remainingTime);
                }
                yield return new WaitForSeconds(0.1f);
            }

            specialFishTimer.gameObject.SetActive(false);
        }
    }

    private string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return $"{minutes}:{seconds:00}";
    }

}