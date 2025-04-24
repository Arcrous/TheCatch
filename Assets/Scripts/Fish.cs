using UnityEngine;

public class Fish : MonoBehaviour
{
    [Header("Fish Data")]
    public FishData fishData;
    public int minimumHookLevel = 1;

    [Header("Movement")]
    [SerializeField] protected float swimSpeed = 2f;
    [SerializeField] protected float wanderRadius = 3f;
    [SerializeField] protected float changeDirectionTime = 3f;

    // References
    protected Rigidbody2D rb;
    protected SpriteRenderer spriteRenderer;
    protected Animator animator;

    // State variables
    protected Vector2 targetPosition;
    protected float directionChangeTimer;
    protected bool isFleeing = false;
    protected bool isCaught = false;

    public int MinimumHookLevel => minimumHookLevel;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Set the sprite from the fish data
        if (fishData != null && spriteRenderer != null)
        {
            spriteRenderer.sprite = fishData.fishSprite;
        }
    }

    protected virtual void Start()
    {
        // Choose initial random direction
        PickNewTargetPosition();
        directionChangeTimer = changeDirectionTime;
    }

    protected virtual void Update()
    {
        if (isCaught) return;

        // Only update direction if not fleeing
        if (!isFleeing)
        {
            // Count down to next direction change
            directionChangeTimer -= Time.deltaTime;
            if (directionChangeTimer <= 0)
            {
                PickNewTargetPosition();
                directionChangeTimer = changeDirectionTime;
            }
        }

        // Move towards target
        if (rb != null)
        {
            Vector2 direction = (targetPosition - rb.position).normalized;
            rb.velocity = direction * swimSpeed;

            // Flip sprite based on movement direction
            if (direction.x != 0 && spriteRenderer != null)
            {
                spriteRenderer.flipX = direction.x < 0;
            }
        }

        // Update animation
        if (animator != null)
        {
            animator.SetFloat("Speed", rb.velocity.magnitude);
        }
    }

    protected virtual void PickNewTargetPosition()
    {
        // Choose a random position within wander radius
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        targetPosition = rb.position + randomDir * Random.Range(1f, wanderRadius);

        // Make sure the fish stays within designated water area
        // This would need to be adjusted based on your level design
        float minX = -8f; // Left boundary
        float maxX = 8f;  // Right boundary
        float minY = -5f; // Top water boundary
        float maxY = -15f; // Bottom water boundary

        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);
    }

    public virtual void DetectHook(FishingHook hook)
    {
        // Default behavior - chance to flee based on fish type
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
        targetPosition = (Vector2)transform.position + fleeDirection * wanderRadius * 2;

        // Increase speed temporarily when fleeing
        swimSpeed *= 1.5f;

        // Stop fleeing after a few seconds
        Invoke("StopFleeing", 2f);
    }

    protected virtual void StopFleeing()
    {
        isFleeing = false;
        swimSpeed = fishData.baseSpeed; // Reset to normal speed
        PickNewTargetPosition();
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
    }

    // For aggressive fish - will be overridden in subclass
    public virtual bool IsAggressive()
    {
        return fishData.isAggressive;
    }
}