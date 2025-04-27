using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float leftBoundary = -8f;  // Left screen boundary
    [SerializeField] private float rightBoundary = 8f;  // Right screen boundary

    [Header("Fishing")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private Transform hookAttachPoint;
    [SerializeField] private float maxCastPower = 10f;
    [SerializeField] private float castPowerIncreaseRate = 5f;
    [SerializeField] private int hookLevel = 1; // For upgrade system
    [SerializeField] private float defaultReelSpeed = 5f;
    [SerializeField] private float reelSpeedUpgradeMultiplier = 0.2f; // 20% increase per level
    [SerializeField] private int baitLevel = 0; // Added bait level field

    [Header("UI")]
    [SerializeField] private GameObject pauseUI; // Reference to the fishing UI

    // Add property for hook level
    public int HookLevel => hookLevel;

    [Header("Current State")]
    // States
    public bool isFishing = false;
    public bool isChargingCast = false;
    private float currentCastPower = 0f;

    // References
    private Rigidbody2D rb;
    private Animator animator;
    private FishingHook currentHook;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        PauseGame(); // Check for pause input

        // Handle input only if not fishing
        if (!isFishing)
        {
            // Movement
            float horizontalInput = Input.GetAxis("Horizontal");
            Vector2 movement = new Vector2(horizontalInput * moveSpeed, 0);
            rb.velocity = movement;

            // Clamp position within boundaries
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, leftBoundary, rightBoundary);
            transform.position = clampedPosition;

            if (Mathf.Abs(horizontalInput) != 0)
            {
                // Update animation
                animator?.SetFloat("Speed", Mathf.Abs(horizontalInput));
            }
            else
            {
                animator?.SetFloat("Speed", 0);
            }

            // Flip sprite based on direction
            if (horizontalInput != 0)
            {
                transform.localScale = new Vector3(
                    horizontalInput < 0 ? -1 : 1,
                    transform.localScale.y,
                    transform.localScale.z);
            }

            // Start casting
            if (Input.GetKeyDown(KeyCode.Space))
            {
                isChargingCast = true;
                currentCastPower = 0f;
            }

            // Charge cast
            if (isChargingCast)
            {
                currentCastPower += castPowerIncreaseRate * Time.deltaTime;
                currentCastPower = Mathf.Min(currentCastPower, maxCastPower);

                // Release cast
                if (Input.GetKeyUp(KeyCode.Space))
                {
                    CastLine();
                    isChargingCast = false;
                }
            }
        }
        else
        {
            // Check if fishing is complete
            if (Input.GetKeyDown(KeyCode.Space) && currentHook != null)
            {
                RetractLine();
            }
        }
    }

    // Add this after the existing properties
    public float GetCurrentCastPower()
    {
        return currentCastPower / maxCastPower;
    }

    public int GetBaitLevel()
    {
        return baitLevel;
    }

    public void UpgradeHook()
    {
        hookLevel++;
        // Update current hook sprite if one exists
        if (currentHook != null)
        {
            int spriteIndex = Mathf.Clamp(hookLevel - 1, 0, currentHook.sprites.Length - 1);
            currentHook.hookSprite.sprite = currentHook.sprites[spriteIndex];
        }

        Debug.Log($"Hook upgraded to level {hookLevel}");
    }

    public void UpgradeBait()
    {
        baitLevel++;
        Debug.Log($"Bait upgraded to level {baitLevel}");
    }

    public void UpgradeReelSpeed()
    {
        // Update reel speed in future hooks
        defaultReelSpeed *= (1 + reelSpeedUpgradeMultiplier);
        Debug.Log($"Reel speed upgraded to {defaultReelSpeed}");
    }

    // Method to pass new reel speed to hooks when created
    public float GetCurrentReelSpeed()
    {
        return defaultReelSpeed;
    }

    private void CastLine()
    {
        isFishing = true;
        rb.velocity = Vector2.zero; // Stop movement
        animator?.SetBool("IsFishing", true);
        // Instantiate hook
        GameObject hookObj = Instantiate(hookPrefab, hookAttachPoint.position, Quaternion.identity);
        currentHook = hookObj.GetComponent<FishingHook>();

        if (currentHook != null)
        {
            int spriteIndex = Mathf.Clamp(hookLevel - 1, 0, currentHook.sprites.Length - 1);
            currentHook.hookSprite.sprite = currentHook.sprites[spriteIndex];

            currentHook.Initialize(this, hookAttachPoint, currentCastPower);
        }
    }

    void PauseGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Time.timeScale == 1f)
            {
                Time.timeScale = 0f; // Pause the game
                if (pauseUI != null && pauseUI.activeSelf == false)
                {
                    pauseUI.SetActive(true); // Show the pause UI
                }
            }
            else
            {
                Time.timeScale = 1f; // Resume the game
                if (pauseUI != null && pauseUI.activeSelf == true)
                {
                    pauseUI.SetActive(false); // close the pause UI
                }
            }
        }
    }

    public void RetractLine()
    {
        if (currentHook != null)
        {
            currentHook.StartRetracting();
        }
    }

    public void OnFishingComplete(bool caughtFish, FishData fishCaught = null)
    {
        isFishing = false;
        animator?.SetBool("IsFishing", false);

        if (caughtFish && fishCaught != null)
        {
            // Add fish to inventory
            InventoryManager.Instance?.AddFish(fishCaught);
        }
    }

    private void OnDrawGizmos()
    {
        // Draw movement boundaries
        Gizmos.color = Color.yellow;

        // Left boundary
        Gizmos.DrawLine(
            new Vector3(leftBoundary, transform.position.y + 5, 0),
            new Vector3(leftBoundary, transform.position.y - 5, 0)
        );

        // Right boundary
        Gizmos.DrawLine(
            new Vector3(rightBoundary, transform.position.y + 5, 0),
            new Vector3(rightBoundary, transform.position.y - 5, 0)
        );
    }
}