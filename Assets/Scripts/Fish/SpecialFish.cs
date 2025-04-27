using UnityEngine;

public class SpecialFish : Fish
{
    [Header("Special Effects")]
    [SerializeField] private Color specialGlowColor = Color.yellow;
    [SerializeField] private float pulseSpeed = 1.5f;
    [SerializeField] private float outlineWidth = 0.01f;
    [SerializeField] private float glowIntensity = 1.5f;

    private Material glowMaterial;
    private SpriteRenderer mainRenderer;



    protected override void Awake()
    {
        base.Awake();
        mainRenderer = GetComponent<SpriteRenderer>();

        // Create material instance
        glowMaterial = new Material(Shader.Find("Sprites/SpriteGlow"));
        mainRenderer.material = glowMaterial;

        // Set initial material properties
        glowMaterial.SetColor("_Color", Color.white);
        glowMaterial.SetColor("_GlowColor", specialGlowColor);
        glowMaterial.SetFloat("_OutlineWidth", outlineWidth);
    }

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        // Pulsing glow effect
        if (glowMaterial != null)
        {
            float pulse = 0.5f + Mathf.PingPong(Time.time * pulseSpeed, 0.5f);
            Color glowColor = specialGlowColor;
            glowColor.a = pulse;
            glowMaterial.SetColor("_GlowColor", glowColor);
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