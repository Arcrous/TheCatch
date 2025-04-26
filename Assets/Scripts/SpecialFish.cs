using UnityEngine;

public class SpecialFish : Fish
{
    [Header("Special Effects")]
    [SerializeField] private GameObject glowEffect;
    [SerializeField] private Color specialGlowColor = Color.yellow;
    [SerializeField] private float pulseSpeed = 1.5f;

    private SpriteRenderer glowRenderer;

    protected override void Awake()
    {
        base.Awake();

        // Create glow effect
        if (glowEffect != null)
        {
            GameObject glow = Instantiate(glowEffect, transform);
            glow.transform.localPosition = Vector3.zero;
            glowRenderer = glow.GetComponent<SpriteRenderer>();
            if (glowRenderer != null)
            {
                glowRenderer.color = specialGlowColor;
            }
        }
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Pulsing glow effect
        if (glowRenderer != null)
        {
            float pulse = 0.5f + Mathf.PingPong(Time.time * pulseSpeed, 0.5f);
            Color glowColor = specialGlowColor;
            glowColor.a = pulse;
            glowRenderer.color = glowColor;
        }
    }

    // Special fish are more likely to flee
    public override void DetectHook(FishingHook hook)
    {
        if (Random.value < fishData.skittishness * 1.5f)
        {
            FleeFromHook(hook.transform.position);
        }
    }

    // Move faster when fleeing
    public override void FleeFromHook(Vector2 hookPosition)
    {
        isFleeing = true;

        Vector2 fleeDirection = ((Vector2)transform.position - hookPosition).normalized;
        targetPosition = (Vector2)transform.position + fleeDirection * 3f;

        // Special fish flee faster
        swimSpeed *= 2f;

        Invoke("StopFleeing", 3f);
    }
}