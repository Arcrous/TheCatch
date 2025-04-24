using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float boundaries = 8f; // Left/right screen boundaries

    [Header("Fishing")]
    [SerializeField] private GameObject hookPrefab;
    [SerializeField] private Transform hookAttachPoint;
    [SerializeField] private float maxCastPower = 10f;
    [SerializeField] private float castPowerIncreaseRate = 5f;

    // States
    private bool isFishing = false;
    private bool isChargingCast = false;
    private float currentCastPower = 0f;

    // References
    private Rigidbody2D rb;
    private Animator animator;
    private FishingHook currentHook;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        //animator = GetComponent<Animator>();
    }

    private void Update()
    {
        // Handle input only if not fishing
        if (!isFishing)
        {
            // Movement
            float horizontalInput = Input.GetAxis("Horizontal");
            Vector2 movement = new Vector2(horizontalInput * moveSpeed, 0);
            rb.velocity = movement;

            // Clamp position within boundaries
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, -boundaries, boundaries);
            transform.position = clampedPosition;

            // Update animation
            animator?.SetFloat("Speed", Mathf.Abs(horizontalInput));

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

                // Visual feedback for charging (could be UI or animation)
                // animator?.SetFloat("CastPower", currentCastPower / maxCastPower);

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
            currentHook.Initialize(this, currentCastPower);
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
            //InventoryManager.Instance?.AddFish(fishCaught);
        }
    }
}