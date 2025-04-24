using UnityEngine;

public class FishingHook : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float descendSpeed = 3f;
    [SerializeField] private float defaultRetractSpeed = 5f;
    [SerializeField] private float maxLineLength = 15f;

    [Header("References")]
    [SerializeField] private LineRenderer fishingLine;
    [SerializeField] private SpriteRenderer hookSprite;
    [SerializeField] private AudioSource hookAudio;
    [SerializeField] private AudioClip catchSound;
    [SerializeField] private AudioClip splashSound;
    [SerializeField] private GameObject splashEffect;

    // References
    private PlayerController player;
    private Transform hookAttachPoint;

    // State variables
    private Vector2 startPosition;
    private float castPower;
    private bool isDescending = true;
    private bool isRetracting = false;
    private bool hasCaughtFish = false;
    private FishData caughtFish;
    //private Fish caughtFishObject;
    private float retractSpeed;
    private float horizontalMovement = 0f;

    public void Initialize(PlayerController playerRef, Transform attachPoint, float power)
    {
        player = playerRef;
        hookAttachPoint = attachPoint;
        startPosition = attachPoint.position;
        castPower = power;
        retractSpeed = defaultRetractSpeed;

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

    private void Update()
    {
        UpdateLinePosition();

        if (isDescending)
        {
            // Move downward with some horizontal drift based on cast power
            transform.Translate(new Vector3(horizontalMovement * Time.deltaTime, -descendSpeed * Time.deltaTime, 0));

            // Slow down horizontal movement over time
            horizontalMovement *= 0.98f;

            // Check if max length reached
            if (Vector2.Distance(startPosition, transform.position) >= maxLineLength)
            {
                StartRetracting();
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
        player.isFishing = false;
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDescending && collision.CompareTag("Fish"))
        {
           /* Fish fish = collision.GetComponent<Fish>();
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
                }
            }*/
        }

        /*// Handle aggressive fish attacking the hook
        if (collision.CompareTag("AggressiveFish"))
        {
            AggressiveFish aggressiveFish = collision.GetComponent<AggressiveFish>();
            if (aggressiveFish != null)
            {
                aggressiveFish.AttackHook(this);
                // This might damage the line or cause other effects
            }
        }*/
    }

   /* private void CatchFish(Fish fish)
    {
        // Catch the fish
        hasCaughtFish = true;
        caughtFish = fish.fishData;
        caughtFishObject = fish;

        // Adjust retract speed based on fish weight
        retractSpeed = defaultRetractSpeed / Mathf.Max(0.5f, fish.fishData.weight);

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
    }*/

    // For aggressive fish interactions
    public void DamageLine(float damageAmount)
    {
        // Implement line damage logic
        // Could slow down retract speed or cause the fish to escape
        retractSpeed *= (1 - damageAmount);

        // Visual feedback
        if (fishingLine != null)
        {
            // Make line "wobble" or change color momentarily
            StartCoroutine(LineWobbleEffect());
        }
    }

    private System.Collections.IEnumerator LineWobbleEffect()
    {
        // Simple visual feedback for line being damaged
        float originalWidth = fishingLine.startWidth;
        fishingLine.startWidth = originalWidth * 1.5f;

        yield return new WaitForSeconds(0.1f);

        fishingLine.startWidth = originalWidth;
    }
}