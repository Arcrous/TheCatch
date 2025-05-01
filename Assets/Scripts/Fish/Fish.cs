using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("Fish Data")]
    public FishData fishData;
    public int minimumHookLevel = 1;

    [Header("Movement")]
    [SerializeField] protected float swimSpeed = 2f;
    [SerializeField] protected float changeDirectionTime = 3f;
    [SerializeField] protected float patrolRadius = 3f; // How far from spawn to patrol

    [Header("Boundaries")]
    [SerializeField] protected float minX = -10f;
    [SerializeField] protected float maxX = 10f;
    [SerializeField] protected float minY = -5f;
    [SerializeField] protected float maxY = 5f;

    [Header("Bait Response")]
    [SerializeField] protected float baitAttraction = 1f; // Base interest in bait
    [SerializeField] protected float attractionResistance = 0.5f; // How much fish resists bait (higher = less affected)
    [SerializeField] protected float baitDetectionChance = 0.7f; // Chance to notice bait

    // References
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    // State variables
    protected Vector2 spawnPosition; // Store initial spawn position
    protected Vector2 targetPosition;
    protected float directionChangeTimer;
    protected bool isFleeing = false;
    protected bool isCaught = false;
    protected bool isAttractedToBait = false;
    protected Vector2 baitPosition;
    protected float baitAttractionStrength = 0f;
    protected float distanceThreshold = 0.5f; // How close to target before picking new one

    [Header("Lifetime")]
    // Add these fields at the top with other protected fields
    [SerializeField] public float lifetime = 0f;
    [SerializeField] public float maxLifetime = 60f; // 2 minutes default lifetime
    [SerializeField] protected bool isInCameraView = false;
    [SerializeField] protected Camera gameCamera;

    // Static variables to track all fish positions
    private static int fishCount = 0;
    private int fishID;

    public int MinimumHookLevel => minimumHookLevel;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Assign a unique ID to each fish
        fishID = fishCount++;

        // Set the speed from the fish data
        if (fishData != null)
        {
            swimSpeed = fishData.baseSpeed * Random.Range(0.8f, 1.2f);
            minimumHookLevel = fishData.requiredHookLevel;
        }
    }

    protected virtual void Start()
    {
        // Force fish to start in different regions based on their ID
        ForceDistribution();

        // Store the spawn position for patrolling reference
        spawnPosition = transform.position;

        // Pick a target within patrol radius
        PickNewTargetPosition();

        // Randomize direction change timer
        directionChangeTimer = changeDirectionTime * Random.Range(0.8f, 1.2f);

        // Get the main camera if not assigned
        if (gameCamera == null)
            gameCamera = Camera.main;
    }

    // Force each fish to a different part of the screen
    protected virtual void ForceDistribution()
    {
        // Divide the area into a grid and place fish based on ID
        float areaWidth = maxX - minX;
        float areaHeight = maxY - minY;

        // Calculate which section this fish should be in
        int sections = 4; // 4 quadrants by default
        int row = fishID % sections;
        int col = (fishID / sections) % sections;

        // Add some randomness within the section
        float sectionWidth = areaWidth / sections;
        float sectionHeight = areaHeight / sections;

        // Calculate the section boundaries
        float sectionMinX = minX + (col * sectionWidth);
        float sectionMaxX = sectionMinX + sectionWidth;
        float sectionMinY = minY + (row * sectionHeight);
        float sectionMaxY = sectionMinY + sectionHeight;

        // Position the fish randomly within its section
        float randomX = Random.Range(sectionMinX, sectionMaxX);
        float randomY = Random.Range(sectionMinY, sectionMaxY);

        // Set the position
        transform.position = new Vector3(randomX, randomY, 0);
    }

    protected virtual void Update()
    {
        if (gameCamera != null && spriteRenderer != null)
        {
            Vector3 viewportPoint = gameCamera.WorldToViewportPoint(transform.position);
            isInCameraView = (viewportPoint.x > 0 && viewportPoint.x < 1 &&
                             viewportPoint.y > 0 && viewportPoint.y < 1 &&
                             viewportPoint.z > 0);
        }
    }

    protected virtual bool ShouldDespawn()
    {
        return lifetime > maxLifetime && !isInCameraView && !isCaught;
    }


    protected virtual void FixedUpdate()
    {
        if (isCaught) return;

        // Update lifetime and check for despawn
        lifetime += Time.fixedDeltaTime;
        if (ShouldDespawn())
        {
            // Notify spawner before destroying
            Debug.Log($"Fish {fishID} despawned after {lifetime} seconds.");
            FindObjectOfType<FishSpawner>()?.OnFishDespawned();
            Destroy(gameObject);
            return;
        }

        // If attracted to bait, update target position to move toward bait
        if (isAttractedToBait && !isFleeing)
        {
            // Update target position to gradually move toward bait
            Vector2 directionToBait = (baitPosition - (Vector2)transform.position).normalized;
            float baitInfluence = baitAttractionStrength * baitAttraction * (1f - attractionResistance);

            // Mix current direction with bait influence
            Vector2 currentDirection = (targetPosition - (Vector2)transform.position).normalized;
            Vector2 newDirection = Vector2.Lerp(currentDirection, directionToBait, baitInfluence * Time.deltaTime);

            // Set new target based on blended direction
            targetPosition = (Vector2)transform.position + newDirection * patrolRadius * 0.5f;

            // Decay bait attraction over time
            baitAttractionStrength *= 0.99f;

            // Stop being attracted when below threshold
            if (baitAttractionStrength < 0.1f)
            {
                isAttractedToBait = false;
            }
        }

        // Check if we've reached the target
        float distanceToTarget = Vector2.Distance(rb.position, targetPosition);
        if (distanceToTarget < distanceThreshold)
        {
            if (!isAttractedToBait)
            {
                PickNewTargetPosition();
            }
            else
            {
                // If we reached bait attraction point, update target closer to actual bait
                targetPosition = Vector2.Lerp(targetPosition, baitPosition, 0.5f);
            }
        }

        // Check for direction change timer
        if (!isFleeing && !isAttractedToBait)
        {
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0)
            {
                PickNewTargetPosition();
                directionChangeTimer = changeDirectionTime * Random.Range(0.8f, 1.2f);
            }
        }


        // Calculate movement direction
        Vector2 direction = (targetPosition - rb.position).normalized;

        // Apply movement
        if (rb != null)
        {
            rb.velocity = direction * swimSpeed;

            // Flip sprite based on movement direction
            if (Mathf.Abs(direction.x) > 0.1f && spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        // Update animation
        if (animator != null)
        {
            animator.SetFloat("Speed", rb.velocity.magnitude);
        }

        // Check if fish is too far from spawn point (happens after fleeing)
        float distanceFromSpawn = Vector2.Distance(rb.position, spawnPosition);
        if (distanceFromSpawn > patrolRadius * 1.5f && !isFleeing)
        {
            // Set target back towards spawn area
            Vector2 directionToSpawn = (spawnPosition - rb.position).normalized;
            targetPosition = rb.position + directionToSpawn * patrolRadius;
        }

        // Ensure fish stays within boundaries
        if (transform.position.x < minX || transform.position.x > maxX ||
            transform.position.y < minY || transform.position.y > maxY)
        {
            // If fish is outside boundaries, pick a new target inside boundaries
            PickNewTargetPosition();
        }
    }

    protected virtual void PickNewTargetPosition()
    {
        // Pick a random point within patrol radius of spawn position
        float randomAngle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float randomDistance = Random.Range(0.5f, patrolRadius);

        // Calculate random point around spawn
        Vector2 randomOffset = new Vector2(
            Mathf.Cos(randomAngle) * randomDistance,
            Mathf.Sin(randomAngle) * randomDistance
        );

        // Set target position relative to spawn point
        targetPosition = spawnPosition + randomOffset;

        // Ensure target is within boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX + 1, maxX - 1);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY + 1, maxY - 1);
    }

    public virtual void DetectBait(Vector2 baitPos, int baitLevel, float attractionForce)
    {
        // Skip if already caught, fleeing, or fish is aggressive
        if (isCaught || isFleeing || fishData.isAggressive) return;

        // Check if fish is interested in bait based on random chance and level
        // Higher bait levels have better chance of attracting fish
        float detectionRoll = Random.value;
        float attractionChance = baitDetectionChance * (1 + (baitLevel * 0.2f));

        if (detectionRoll < attractionChance)
        {
            // Set bait attraction parameters
            isAttractedToBait = true;
            baitPosition = baitPos;

            // Attraction strength based on bait level and attractionForce parameter
            baitAttractionStrength = attractionForce * (1 + (baitLevel * 0.2f));

            // Increase speed slightly when moving toward bait
            swimSpeed *= 0.5f;

            // Create intermediate target position to make movement more natural
            Vector2 directionToBait = (baitPosition - (Vector2)transform.position).normalized;
            targetPosition = (Vector2)transform.position + directionToBait * (patrolRadius * 0.5f);
        }
    }

    public virtual void DetectHook(FishingHook hook)
    {
        // Chance to flee based on fish skittishness
        if (Random.value < fishData.skittishness)
        {
            FleeFromHook(hook.transform.position);
        }
    }

    public virtual void FleeFromHook(Vector2 hookPosition)
    {
        isFleeing = true;

        // Move away from hook
        Vector2 fleeDirection = ((Vector2)transform.position - hookPosition).normalized;
        targetPosition = (Vector2)transform.position + fleeDirection * 6f;

        // Make sure target is within boundaries
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX + 1, maxX - 1);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY + 1, maxY - 1);

        // Increase speed temporarily when fleeing
        swimSpeed *= 1.5f;

        // Stop fleeing after a few seconds
        Invoke("StopFleeing", 2f);
    }

    protected virtual void StopFleeing()
    {
        isFleeing = false;
        swimSpeed = fishData.baseSpeed; // Reset to normal speed

        // After fleeing, set target back towards spawn area
        Vector2 directionToSpawn = (spawnPosition - (Vector2)transform.position).normalized;
        targetPosition = (Vector2)transform.position + directionToSpawn * patrolRadius * 0.7f;
    }

    public virtual void Escape()
    {
        // Called when hook fails to catch this fish
        FleeFromHook(transform.position + Vector3.up * 2f);
    }

    public virtual void OnCaught()
    {
        isCaught = true;
        rb.velocity = Vector2.zero;

        // Play caught animation if available
        animator?.SetTrigger("Caught");

        // Disable collider
        GetComponent<Collider2D>().enabled = false;

        // Find and notify the FishSpawner
        var spawner = FindObjectOfType<FishSpawner>();
        if (spawner != null)
        {
            spawner.OnFishCaught(gameObject);
        }
    }

    public virtual bool IsAggressive()
    {
        return fishData.isAggressive;
    }
}