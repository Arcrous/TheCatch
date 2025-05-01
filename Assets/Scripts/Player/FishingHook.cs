using System.Collections;
using UnityEngine;

public class FishingHook : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float descendSpeed = 3f;
    [SerializeField] private float maxLineLength = 15f;

    [Header("References")]
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] public SpriteRenderer hookSprite;
    [SerializeField] public Sprite[] sprites;
    [SerializeField] private AudioSource hookAudio;
    [SerializeField] private AudioClip catchSound;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private GameObject splashEffect;

    [Header("Bait Settings")]
    [SerializeField] private float baseAttractionRadius = 3f;
    [SerializeField] private float attractionMultiplierPerLevel = 0.5f;
    [SerializeField] private float attractionForce = 1f;
    [SerializeField] private GameObject baitVisualEffect;

    // References
    public PlayerController player;
    private Transform hookAttachPoint;

    // State variables
    private Vector2 startPosition;
    private float castPower;
    private bool isDescending = true;
    private bool isRetracting = false;
    private bool hasCaughtFish = false;
    private FishData caughtFish;
    private Fish caughtFishObject;
    private float retractSpeed;
    private float horizontalMovement = 0f;
    private int baitLevel = 0;
    private float currentAttractionRadius;

    public void Initialize(PlayerController playerRef, Transform attachPoint, float power)
    {
        player = playerRef;
        hookAttachPoint = attachPoint;
        startPosition = attachPoint.position;
        castPower = power;

        // Get bait level from player
        baitLevel = player.GetBaitLevel();

        // Calculate attraction radius based on bait level
        currentAttractionRadius = baseAttractionRadius + (baseAttractionRadius * attractionMultiplierPerLevel * baitLevel);

        // Scale bait visual effect based on attraction radius if it exists
        if (baitVisualEffect != null)
        {
            baitVisualEffect.transform.localScale = Vector3.one * (currentAttractionRadius / baseAttractionRadius);

            // Change color intensity based on bait level
            ParticleSystem particles = baitVisualEffect.GetComponent<ParticleSystem>();
            if (particles != null)
            {
                ParticleSystem.MainModule main = particles.main;
                Color baitColor = Color.white;
                baitColor.a = 0.2f + (0.15f * baitLevel);
                main.startColor = baitColor;
            }
        }

        // Use player's current reel speed instead of default
        retractSpeed = player.GetCurrentReelSpeed();

        // Apply some horizontal movement based on cast power and player direction
        horizontalMovement = (player.transform.localScale.x > 0 ? 1 : -1) * (castPower * 0.1f);

        // Set up fishing line
        if (fishingLine != null)
        {
            fishingLine.positionCount = 2;
            fishingLine.SetPosition(0, startPosition);
            fishingLine.SetPosition(1, transform.position);
        }

        // Play splash sound when hook hits water
        if (hookAudio != null && splashSound != null)
        {
            hookAudio.PlayOneShot(splashSound);
        }

        // Show splash effect
        if (splashEffect != null)
        {
            Instantiate(splashEffect, transform.position, Quaternion.identity);
        }
    }

    private void FixedUpdate()
    {
        UpdateLinePosition();

        if (isDescending)
        {
            // Move downward with some horizontal drift based on cast power
            transform.Translate(new Vector3(-horizontalMovement * Time.deltaTime, -descendSpeed * Time.deltaTime, 0));

            // Slow down horizontal movement over time
            horizontalMovement *= 0.98f;

            // Check if max length reached
            if (Vector2.Distance(startPosition, transform.position) >= maxLineLength)
            {
                StartRetracting();
            }

            // Attract nearby fish if using bait
            if (baitLevel > 0)
            {
                AttractNearbyFish();
            }
        }
        else if (isRetracting)
        {
            // Move back to player's hook attach point
            Vector2 targetPos = hookAttachPoint.position;
            transform.position = Vector2.MoveTowards(
                transform.position,
                targetPos,
                retractSpeed * Time.deltaTime);

            // Check if reached player
            if (Vector2.Distance(targetPos, transform.position) < 0.2f)
            {
                CompleteFishing();
            }
        }
    }

    private void AttractNearbyFish()
    {
        // Find all fish within attraction radius
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, currentAttractionRadius);

        foreach (Collider2D collider in colliders)
        {
            Fish fish = collider.GetComponent<Fish>();
            if (fish != null && !fish.IsAggressive() && !hasCaughtFish)
            {
                // Notify fish about bait (attraction strength based on bait level)
                fish.DetectBait(transform.position, baitLevel, attractionForce);
            }
        }
    }

    private void UpdateLinePosition()
    {
        if (fishingLine != null && hookAttachPoint != null)
        {
            fishingLine.SetPosition(0, hookAttachPoint.position);
            fishingLine.SetPosition(1, transform.position);
        }
    }

    public void StartRetracting()
    {
        isDescending = false;
        isRetracting = true;
    }

    private void CompleteFishing()
    {
        player.OnFishingComplete(hasCaughtFish, caughtFish);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDescending && collision.CompareTag("Fish"))
        {
            Fish fish = collision.GetComponent<Fish>();
            if (fish != null && !hasCaughtFish)
            {
                // Attempt to catch fish based on hook level vs fish difficulty
                if (player.HookLevel >= fish.MinimumHookLevel)
                {
                    // Successfully catch the fish
                    CatchFish(fish);
                }
                else
                {
                    // Failed to catch (hook too weak)
                    fish.Escape();
                    // We could add feedback here (hook wobble animation, etc)
                    StartCoroutine(FailedCatch());
                    StartRetracting();
                }
            }
        }

        // Handle aggressive fish attacking the hook
        if (collision.CompareTag("AggressiveFish"))
        {
            AggressiveFish aggressiveFish = collision.GetComponent<AggressiveFish>();
            if (aggressiveFish != null && !hasCaughtFish)
            {// Attempt to catch fish based on hook level vs fish difficulty
                if (player.HookLevel >= aggressiveFish.MinimumHookLevel)
                {
                    // Successfully catch the fish
                    CatchFish(aggressiveFish);
                }
                else
                {
                    aggressiveFish.AttackHook(this);
                }
                // This might damage the line or cause other effects
            }
        }
    }

    private void CatchFish(Fish fish)
    {
        // Catch the fish
        hasCaughtFish = true;
        caughtFish = fish.fishData;
        caughtFishObject = fish;

        // Adjust retract speed based on fish weight
        retractSpeed = player.GetCurrentReelSpeed() / Mathf.Max(0.5f, fish.fishData.weight);

        // Disable fish's own collider and scripts
        fish.GetComponent<Collider2D>().enabled = false;
        fish.enabled = false;

        // Attach fish to hook
        fish.transform.SetParent(transform);
        fish.transform.localPosition = new Vector3(0, -0.5f, 0); // Position just below hook

        // Play catch sound
        if (hookAudio != null && catchSound != null)
        {
            hookAudio.PlayOneShot(catchSound);
        }

        // Start retracting
        StartRetracting();
    }

    // For aggressive fish interactions
    public void DamageLine(float damageAmount)
    {
        StartCoroutine(FailedCatch());

        // Could slow down retract speed or cause the fish to escape
        retractSpeed *= 1 - damageAmount;
        // Visual feedback
        if (fishingLine != null)
        {
            // Make line "wobble" or change color momentarily
            StartCoroutine(LineWobbleEffect());
            StartRetracting();
        }
    }

    private System.Collections.IEnumerator LineWobbleEffect()
    {
        // Simple visual feedback for line being damaged
        float originalWidth = fishingLine.startWidth;
        fishingLine.startWidth = originalWidth * 1.5f;

        yield return new WaitForSeconds(0.3f);

        fishingLine.startWidth = originalWidth;

        retractSpeed = player.GetCurrentReelSpeed();
    }

    IEnumerator FailedCatch()
    {
        Debug.Log("Failed to catch fish!");
        Color originalColor = fishingLine.startColor;
        float elapsedTime = 0f;
        float transitionDuration = 0.2f;

        // Fade to red
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            Color lerpedColor = Color.Lerp(originalColor, Color.red, t);

            fishingLine.startColor = lerpedColor;
            fishingLine.endColor = lerpedColor;

            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        // Fade back to original
        elapsedTime = 0f;
        while (elapsedTime < transitionDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / transitionDuration;
            Color lerpedColor = Color.Lerp(Color.red, originalColor, t);

            fishingLine.startColor = lerpedColor;
            fishingLine.endColor = lerpedColor;

            yield return null;
        }
    }
}