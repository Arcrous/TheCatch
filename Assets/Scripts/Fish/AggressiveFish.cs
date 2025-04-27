using UnityEngine;

public class AggressiveFish : Fish
{
    [Header("Aggressive Behavior")]
    [SerializeField] private float detectionRadius = 4f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float attackDamage = 0.2f;
    [SerializeField] private GameObject attackEffect;

    private bool canAttack = true;
    private float cooldownTimer = 0f;

    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (isCaught) return;

        // Update attack cooldown
        if (!canAttack)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0)
            {
                canAttack = true;
            }
        }

        // Look for hooks to attack
        if (canAttack)
        {
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, detectionRadius);
            foreach (Collider2D collider in colliders)
            {
                if (collider.CompareTag("FishHook"))
                {
                    FishingHook hook = collider.GetComponent<FishingHook>();
                    if (hook != null)
                    {
                        if (minimumHookLevel > hook.player.HookLevel)
                        {
                            // Check if the hook is within attack range
                            float distanceToHook = Vector2.Distance(transform.position, hook.transform.position);

                            //If the hook is too far away, move near it
                            if (distanceToHook > attackRange)
                            {
                                Vector2 direction = (hook.transform.position - transform.position).normalized;
                                targetPosition = hook.transform.position - (Vector3)(direction * attackRange * 0.8f);
                                return;
                            }

                            //If the hook is not too far away, attack it
                            AttackHook(hook);
                            break;
                        }
                    }
                }
            }
        }
    }

    public void AttackHook(FishingHook hook)
    {
        if (!canAttack) return;

        // Move towards hook
        Vector2 direction = (hook.transform.position - transform.position).normalized;
        targetPosition = hook.transform.position;

        // Apply damage to hook/line
        hook.DamageLine(attackDamage);

        // Play attack effect
        if (attackEffect != null)
        {
            Instantiate(attackEffect, transform.position, Quaternion.identity);
        }

        // Set cooldown
        canAttack = false;
        cooldownTimer = attackCooldown;

        // Play attack animation
        animator?.SetTrigger("Attack");
    }

    public override bool IsAggressive()
    {
        return true;
    }

    // Visualize detection radius in editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}